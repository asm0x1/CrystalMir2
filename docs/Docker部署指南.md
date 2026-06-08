# Crystal Mir2 Server - Docker 部署指南

> 作者: asm0x1

本文档介绍如何使用 Docker 部署 Crystal Mir2 游戏服务器。

## 前置要求

- **Docker** >= 20.10
- **Docker Compose** >= 2.0
- 游戏数据文件（配置、地图、脚本等）

## 快速开始

### 1. 准备游戏数据

在项目根目录下创建以下目录，并放入对应的游戏数据：

```
CrystalMir2/
├── Configs/          # 配置文件
│   ├── Setup.ini     # 主配置文件（必需）
│   ├── ExpList.ini   # 经验表
│   └── ...           # 其他 ini 配置文件
├── Maps/             # 地图文件（*.map）
├── Envir/            # 游戏脚本和数据
│   ├── NPCs/         # NPC 脚本
│   ├── Drops/        # 掉落配置
│   ├── Quests/       # 任务配置
│   └── ...
├── Localization/     # 语言文件
│   ├── English.json
│   └── Chinese.json
```

### 2. 关键配置项说明

编辑 `Configs/Setup.ini`，以下是与 Docker 部署相关的关键配置：

| 配置项 | 说明 | 建议值 |
|--------|------|--------|
| `IPAddress` | 服务器监听地址 | `0.0.0.0`（监听所有网卡） |
| `Port` | 游戏客户端连接端口 | `7000` |
| `VersionCheck` | 客户端版本校验 | `0`（关闭，方便不同客户端连接） |

> 注意：`IPAddress` 必须设为 `0.0.0.0`，否则 Docker 容器内的服务无法被外部访问。

### 3. 构建并启动

```bash
# 构建镜像并启动容器（前台运行，可以看到日志）
docker compose up --build

# 后台运行
docker compose up -d --build
```

启动成功后，你应该看到类似以下输出：

```
╔══════════════════════════════════════════════════════╗
║         Crystal Mir2 Server - 皓石传奇二            ║
║         Version: 1.0.0.0                             ║
║         Author:  asm0x1                              ║
║         开源项目，仅供学习使用，禁止商用              ║
╚══════════════════════════════════════════════════════╝

[初始化] 正在加载服务器配置...
[校验] 正在检查配置参数...
[优化] GC 模式已设置为 SustainedLowLatency
[信息] 游戏端口: 7000
[信息] 绑定地址: 0.0.0.0
[启动] 服务器主循环已启动！网络监听中...
[运行] 服务器正在运行中... 按 Ctrl+C 优雅关闭

[状态 #1] 运行时间: 00:00:10 | 在线玩家: 0
[状态 #2] 运行时间: 00:00:20 | 在线玩家: 0
```

## 常用命令

```bash
# 查看日志
docker compose logs -f

# 查看最近 100 行日志
docker compose logs --tail 100

# 停止服务器
docker compose down

# 停止服务器并删除数据卷（危险！会清除所有玩家数据）
docker compose down -v

# 重启服务器
docker compose restart

# 进入容器内部
docker exec -it crystal-mir2 /bin/bash
```

## 目录挂载说明

| 容器内路径 | 挂载方式 | 说明 |
|-----------|----------|------|
| `/app/Configs` | bind mount | 配置文件，修改后重启容器生效 |
| `/app/Maps` | bind mount | 地图文件，较大（~800MB） |
| `/app/Envir` | bind mount | NPC脚本、掉落、任务等 |
| `/app/Localization` | bind mount | 语言文件 |
| `/app/Logs` | bind mount | 日志输出，方便在宿主机查看 |
| `/app/Server.MirDB` | named volume | 游戏数据库（持久化） |
| `/app/Server.MirADB` | named volume | 玩家账户数据（持久化） |

## 端口说明

| 端口 | 协议 | 说明 |
|------|------|------|
| `7000` | TCP | 游戏客户端连接端口 |
| `3000` | TCP | 服务器状态查询端口（可选） |

可在 `.env` 文件中自定义映射端口：

```bash
# .env 文件
GAME_PORT=17000
STATUS_PORT=13000
```

## 备份与恢复

### 备份数据

```bash
# 备份数据库卷
docker run --rm -v crystal-mir2-db:/data -v $(pwd)/backup:/backup alpine tar czf /backup/mirdb-$(date +%Y%m%d).tar.gz -C /data .

# 备份账户数据卷
docker run --rm -v crystal-mir2-accounts:/data -v $(pwd)/backup:/backup alpine tar czf /backup/miradb-$(date +%Y%m%d).tar.gz -C /data .

# 备份配置文件
tar czf backup/configs-$(date +%Y%m%d).tar.gz Configs/ Envir/ Maps/ Localization/
```

### 恢复数据

```bash
# 恢复数据库卷
docker run --rm -v crystal-mir2-db:/data -v $(pwd)/backup:/backup alpine tar xzf /backup/mirdb-20250101.tar.gz -C /data

# 恢复账户数据卷
docker run --rm -v crystal-mir2-accounts:/data -v $(pwd)/backup:/backup alpine tar xzf /backup/miradb-20250101.tar.gz -C /data
```

## 常见问题

### 1. 服务器启动后立刻退出

检查 `Configs/Setup.ini` 是否存在，以及配置是否正确。查看日志：

```bash
docker compose logs --tail 50
```

### 2. 客户端无法连接

- 确认 `Configs/Setup.ini` 中 `IPAddress=0.0.0.0`
- 确认宿主机防火墙允许对应端口
- 检查 `docker compose ps` 确认端口映射正确

### 3. 地图文件加载失败

确认 `Maps/` 目录中有对应的 `.map` 文件。地图文件通常较大，需要从游戏数据包中获取。

### 4. 容器内中文乱码

确保 `docker-compose.yml` 中已配置：

```yaml
environment:
  - TZ=Asia/Shanghai
  - DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
```

## 技术架构

```
┌─────────────────────────────────────────┐
│              Docker Container            │
│  ┌───────────────────────────────────┐  │
│  │     Server.Console (.NET 8)       │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │   Server.Library             │  │  │
│  │  │  ┌───────────────────────┐  │  │  │
│  │  │  │ Envir (Game Loop)     │  │  │  │
│  │  │  │ Map / Pathfinding     │  │  │  │
│  │  │  │ Monster AI / NPC      │  │  │  │
│  │  │  │ Combat / Magic        │  │  │  │
│  │  │  └───────────────────────┘  │  │  │
│  │  │  MirNetwork (TCP 7000)      │  │  │
│  │  │  MirDatabase (Binary)       │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
│                   │                      │
│   Volume Mounts   │    Port Mapping      │
│   ┌──────────┐    │    ┌──────────┐      │
│   │ Configs  │    │    │  :7000   │      │
│   │ Maps     │    │    │  :3000   │      │
│   │ Envir    │    │    └──────────┘      │
│   │ MirDB    │    │                      │
│   │ MirADB   │    │                      │
│   └──────────┘    │                      │
└─────────────────────────────────────────┘
```
