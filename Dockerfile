# ============================================================
# Crystal Mir2 Server - Dockerfile
# 作者: asm0x1
#
# 多阶段构建：SDK 编译 → aspnet 运行时
# 参考 Zircon Legend Server 的 Docker 化方案
# ============================================================

# ---- Stage 1: Build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 先复制 csproj 文件，利用 Docker 层缓存加速 restore
# 注意：不 restore 整个 sln，因为 PatcherWebSite 是 .NET Framework 项目，跨平台 MSBuild 无法处理
COPY ["Server.Console/Server.Console.csproj", "Server.Console/"]
COPY ["Server/Server.Library.csproj", "Server/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# 还原 NuGet 依赖（只还原 Server.Console 及其传递依赖）
RUN dotnet restore "Server.Console/Server.Console.csproj"

# 复制全部源码
COPY . .

# 发布 Server.Console 项目
RUN dotnet publish "Server.Console/Server.Console.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ---- Stage 2: Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# 作者: asm0x1
LABEL maintainer="asm0x1" \
      description="Crystal Mir2 Server - 皓石传奇二服务端" \
      version="1.0"

WORKDIR /app

# 从构建阶段复制发布产物
COPY --from=build /app/publish .

# 预创建运行时需要的目录
RUN mkdir -p Configs Maps Envir Localization Logs \
    && mkdir -p "Back Up/Database" "Back Up/Accounts"

# 暴露端口
# 7000 - 游戏客户端连接端口
# 3000 - 服务器状态查询端口（可选）
EXPOSE 7000 3000

# 启动服务器
ENTRYPOINT ["dotnet", "Server.Console.dll"]
