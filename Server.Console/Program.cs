using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using log4net;
using Server;
using Server.MirDatabase;
using Server.MirEnvir;

/*
 * Crystal Mir2 - 服务器控制台入口
 * 作者: asm0x1
 *
 * 这是一个跨平台控制台应用，用于替代原有的 Server.MirForms (WinForms) 宿主，
 * 使 Mirror 2 游戏服务器可以在 Linux Docker 容器中运行。
 *
 * 启动流程参考 Zircon Legend Server 的设计模式。
 */

// ============================================================
// 1. 打印启动横幅
// ============================================================
var version = Assembly.GetExecutingAssembly().GetName().Version;
Console.Title = $"Crystal Mir2 Server v{version}";
Console.WriteLine("╔══════════════════════════════════════════════════════╗");
Console.WriteLine("║         Crystal Mir2 Server - 皓石传奇二            ║");
Console.WriteLine($"║         Version: {version?.ToString() ?? "DEV",-35}║");
Console.WriteLine("║         Author:  asm0x1                              ║");
Console.WriteLine("║         开源项目，仅供学习使用，禁止商用              ║");
Console.WriteLine("╚══════════════════════════════════════════════════════╝");
Console.WriteLine();

// ============================================================
// 2. 初始化基础环境
// ============================================================
Packet.IsServer = true;

// 配置 log4net
var entryAssembly = Assembly.GetEntryAssembly();
if (entryAssembly != null)
{
    var logRepository = LogManager.GetRepository(entryAssembly);
    log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
}

Console.WriteLine("[初始化] 正在加载服务器配置...");

try
{
    Settings.Load();
}
catch (Exception ex)
{
    Console.WriteLine($"[错误] 配置加载失败: {ex.Message}");
    Console.WriteLine("请确保 Configs/Setup.ini 文件存在且格式正确。");
    return 1;
}

// ============================================================
// 3. 配置校验与自动修正（参考 Zircon Legend Server）
// ============================================================
Console.WriteLine("[校验] 正在检查配置参数...");

// 验证 IP 地址
if (string.IsNullOrWhiteSpace(Settings.IPAddress))
{
    Console.WriteLine("[修正] IP地址为空，自动设置为 0.0.0.0（监听所有网卡）");
    Settings.IPAddress = "0.0.0.0";
}

// 验证端口范围
if (Settings.Port < 1 || Settings.Port > 65535)
{
    Console.WriteLine($"[修正] 游戏端口无效({Settings.Port})，自动设置为 7000");
    Settings.Port = 7000;
}

// 验证版本检查设置（Docker 环境建议关闭版本校验）
if (Settings.CheckVersion)
{
    Console.WriteLine("[提示] 客户端版本校验已启用，Docker 环境下建议关闭。");
    Console.WriteLine("       可在 Configs/Setup.ini 中设置 CheckVersion=False");
}

// 设置 GC 为低延迟模式（适合游戏服务器）
GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
Console.WriteLine("[优化] GC 模式已设置为 SustainedLowLatency");

Console.WriteLine($"[信息] 游戏端口: {Settings.Port}");
Console.WriteLine($"[信息] 绑定地址: {Settings.IPAddress}");
Console.WriteLine($"[信息] 地图路径: {Settings.MapPath}");
Console.WriteLine($"[信息] 数据库路径: {Envir.DatabasePath}");
Console.WriteLine($"[信息] 账户数据路径: {Envir.AccountPath}");
Console.WriteLine();

// ============================================================
// 4. 注册优雅关闭处理（必须在启动服务器之前注册）
// ============================================================
var shutdownEvent = new ManualResetEventSlim(false);

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // 阻止进程立即终止
    Console.WriteLine();
    Console.WriteLine("[关闭] 收到 Ctrl+C 信号，正在优雅关闭服务器... 作者: asm0x1");
    shutdownEvent.Set();
};

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    Console.WriteLine("[关闭] 收到进程退出信号，正在保存数据... 作者: asm0x1");
    ShutdownServer();
};

// SIGTERM 处理（Docker 停止容器时发送的信号）
// 在 Linux 上，AppDomain.ProcessExit 可能不会捕获 SIGTERM，
// 需要通过 AssemblyLoadContext 或 PosixSignalRegistration 处理
if (OperatingSystem.IsLinux())
{
    // PosixSignalRegistration 在 .NET 6+ 中可用
    var registration = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
    {
        Console.WriteLine("[关闭] 收到 SIGTERM 信号，正在优雅关闭服务器... 作者: asm0x1");
        ctx.Cancel = true;
        shutdownEvent.Set();
    });

    PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx =>
    {
        Console.WriteLine("[关闭] 收到 SIGINT 信号，正在优雅关闭服务器... 作者: asm0x1");
        ctx.Cancel = true;
        shutdownEvent.Set();
    });
}

// ============================================================
// 5. 启动服务器
// ============================================================
Console.WriteLine("[启动] 正在加载游戏数据并启动服务器...");

try
{
    Envir.Main.Start();
}
catch (Exception ex)
{
    Console.WriteLine($"[错误] 服务器启动失败: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    return 1;
}

// 等待服务器线程启动完成
Thread.Sleep(500);

if (!Envir.Main.Running)
{
    Console.WriteLine("[错误] 服务器启动失败，请检查上方日志。");
    Console.WriteLine("常见原因：");
    Console.WriteLine("  - Configs/Setup.ini 配置不正确");
    Console.WriteLine("  - Server.MirDB 数据库文件缺失或损坏");
    Console.WriteLine("  - Maps/ 地图文件缺失");
    Console.WriteLine("  - 端口被占用");
    return 1;
}

Console.WriteLine("[启动] 服务器主循环已启动！网络监听中...");
Console.WriteLine();

// ============================================================
// 5.5 首次启动时自动创建默认管理员账户（作者: asm0x1）
//     仅在 AccountList 为空（数据库全新）时创建，后续启动不会重复执行
// ============================================================
if (Envir.Main.AccountList.Count == 0)
{
    CreateDefaultAdminAccount();
}
else
{
    Console.WriteLine($"[账户] 已加载 {Envir.Main.AccountList.Count} 个账户，跳过初始账户创建。");
}

// ============================================================
// 6. 后台日志轮询（参考 Zircon Legend Server 模式）
//    每 200ms 从 MessageQueue 读取日志并输出到控制台
// ============================================================
var logCts = new CancellationTokenSource();
var logTask = Task.Run(async () =>
{
    var queue = MessageQueue.Instance;

    while (!logCts.Token.IsCancellationRequested)
    {
        // 处理服务器消息日志
        while (queue.MessageLog.TryDequeue(out var msg))
        {
            Console.Write(msg);
        }

        // 处理调试日志
        while (queue.DebugLog.TryDequeue(out var msg))
        {
            Console.Write(msg);
        }

        // 处理聊天日志（生产环境可按需开启）
        while (queue.ChatLog.TryDequeue(out var msg))
        {
            Console.Write(msg);
        }

        try
        {
            await Task.Delay(200, logCts.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }

    // 排空剩余的日志
    while (queue.MessageLog.TryDequeue(out var msg)) Console.Write(msg);
    while (queue.DebugLog.TryDequeue(out var msg)) Console.Write(msg);
    while (queue.ChatLog.TryDequeue(out var msg)) Console.Write(msg);
});

// ============================================================
// 7. 主线程 —— 定期显示服务器状态，等待关闭信号
// ============================================================
var startTime = DateTime.Now;
var statusTick = 0;

Console.WriteLine("[运行] 服务器正在运行中... 按 Ctrl+C 优雅关闭");
Console.WriteLine();

// 每 10 秒显示一次状态，直到收到关闭信号
while (!shutdownEvent.Wait(10000))
{
    statusTick++;
    var uptime = DateTime.Now - startTime;
    var playerCount = Envir.Main.Players?.Count ?? 0;
    Console.WriteLine($"[状态 #{statusTick}] 运行时间: {uptime:dd\\.hh\\:mm\\:ss} | 在线玩家: {playerCount}");
}

// ============================================================
// 8. 优雅关闭
// ============================================================
Console.WriteLine("[关闭] 正在停止服务器...");
ShutdownServer();

// 停止日志轮询
logCts.Cancel();
try { await logTask; } catch (OperationCanceledException) { }

Console.WriteLine("[关闭] 服务器已停止。感谢使用 Crystal Mir2 Server！作者: asm0x1");
return 0;

// ============================================================
// 辅助方法
// ============================================================

/// <summary>
/// 自动创建默认管理员账户 asm0x1 / 123456
/// 作者: asm0x1
/// </summary>
static void CreateDefaultAdminAccount()
{
    const string defaultAdminId = "asm0x1";
    const string defaultAdminPassword = "123456";

    try
    {
        // 等待服务器完全初始化
        Thread.Sleep(2000);

        // 检查账户是否已存在
        var existingAccount = Envir.Main.GetAccount(defaultAdminId);
        if (existingAccount != null)
        {
            Console.WriteLine($"[账户] 管理员账户 '{defaultAdminId}' 已存在。");
            // 确保管理员权限
            if (!existingAccount.AdminAccount)
            {
                existingAccount.AdminAccount = true;
                Console.WriteLine($"[账户] 已为 '{defaultAdminId}' 授予管理员权限。");
            }
            return;
        }

        // 创建新管理员账户
        var account = new AccountInfo
        {
            Index = ++Envir.Main.NextAccountID,
            AccountID = defaultAdminId,
            Password = defaultAdminPassword,
            UserName = "Administrator",
            CreationIP = "127.0.0.1",
            CreationDate = DateTime.Now,
            AdminAccount = true
        };

        lock (Envir.AccountLock)
        {
            Envir.Main.AccountList.Add(account);
        }

        Console.WriteLine($"[账户] 默认管理员账户已创建: {defaultAdminId}/{defaultAdminPassword}");
        Console.WriteLine($"[账户] 管理员权限已启用。请登录后尽快修改密码！");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[错误] 创建管理员账户失败: {ex.Message}");
    }
}

static void ShutdownServer()
{
    if (!Envir.Main.Running) return;

    try
    {
        Envir.Main.Stop();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[错误] 服务器关闭异常: {ex.Message}");
    }

    try
    {
        Settings.Save();
        Console.WriteLine("[关闭] 配置已保存。");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[错误] 配置保存失败: {ex.Message}");
    }
}
