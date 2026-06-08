# Crystal Mir2 - 皓石传奇二

> 作者: asm0x1 | 基于 [Suprcode/Crystal](https://github.com/Suprcode/Crystal)

Legend of Mir 2 的开源服务端与客户端引擎，由 LOMCN 社区开发维护。此分支新增 **Docker 化部署**支持，可在 Linux 服务器上一键运行。

---

## 快速开始（Docker）

```bash
# 1. 克隆项目
git clone https://github.com/asm0x1/CrystalMir2.git
cd CrystalMir2

# 2. 下载游戏数据文件
# 从 Crystal.Database 下载最新数据库：
#    https://github.com/Suprcode/Crystal.Database/releases
# 解压后将文件放入对应目录：
#   Maps/*.map   → Maps/
#   Envir/        → Envir/ （NPC脚本、掉落、任务等）
#   Configs/      → Configs/ （含 Setup.ini）

# 3. 启动
docker compose up -d
```

服务器监听 `0.0.0.0:7000`，首次启动自动创建管理员账户。

## 默认管理员账户

| 账号 | 密码 |
|------|------|
| `asm0x1` | `123456` |

首次启动时自动创建，后续不会重复生成。

## 游戏数据下载

本仓库仅包含源码，游戏数据（地图、脚本、配置）需单独下载：

- **[Crystal.Database Releases](https://github.com/Suprcode/Crystal.Database/releases)** — 官方数据库
- **[Crystal.MapEditor](https://github.com/Suprcode/Crystal.MapEditor)** — 地图编辑器

数据文件目录结构：
```
CrystalMir2/
├── Configs/Setup.ini      # 服务器配置
├── Maps/*.map             # 地图文件
├── Envir/                 # 游戏脚本
│   ├── Drops/             # 掉落配置
│   ├── NPCs/              # NPC 脚本
│   ├── Quests/            # 任务
│   └── ...
└── Localization/          # 语言文件
```

## 项目结构

```
CrystalMir2/
├── Shared/                 # 数据协议层
├── Server.Library/         # 核心服务端逻辑
├── Server.Console/         # 跨平台控制台宿主（Docker 入口）
├── Server.MirForms/        # Windows Forms 管理工具
├── Client/                 # 游戏客户端（WinForms + SlimDX）
├── Tools/                  # 辅助工具
├── Components/             # 依赖库（SlimDX 等）
├── Dockerfile              # Docker 多阶段构建
├── docker-compose.yml      # Docker Compose 编排
└── docs/                   # 文档
```

## 构建

需要 .NET 8 SDK。

```bash
# 构建全部项目
dotnet build "Legend of Mir.sln"

# 仅构建 Server.Console（Docker 用）
dotnet publish Server.Console/Server.Console.csproj -c Release
```

## 客户端

Windows 客户端位于 `Client/`，使用 SlimDX (Direct3D9) 渲染。协议为自定义二进制格式，不同 Mir2 服务器间不兼容。

## 文档

- [Docker 部署指南](docs/Docker部署指南.md)
- [GM 命令指南](docs/GM命令指南.md)
- [LOMCN Wiki - Crystal](https://www.lomcn.net/wiki/index.php/Crystal)
- [构建指南](https://www.lomcn.net/wiki/index.php/Getting_Started)

## 相关项目

| 项目 | 链接 |
|------|------|
| Crystal（原版） | [Suprcode/Crystal](https://github.com/Suprcode/Crystal) |
| Crystal.Database | [Suprcode/Crystal.Database](https://github.com/Suprcode/Crystal.Database) |
| Crystal.MapEditor | [Suprcode/Crystal.MapEditor](https://github.com/Suprcode/Crystal.MapEditor) |
| Zircon（Mir3） | [Suprcode/Zircon](https://github.com/Suprcode/Zircon) |

## 协议

[GPL v2](LICENSE) — 仅供学习使用，禁止商用。
