# 本地开发启动

## 第一次配置

在项目根目录执行：

```powershell
Copy-Item deploy/local/.env.example deploy/local/.env
```

将`.env`中的SQL Server密码替换为只用于本机开发的强密码。`.env`已被Git忽略。

然后把同一个密码保存到.NET User Secrets：

```powershell
dotnet user-secrets set `
  "ConnectionStrings:TradeFlowDatabase" `
  "Server=localhost,14330;Database=TradeFlow;User Id=sa;Password=你的本地密码;TrustServerCertificate=True" `
  --project backend/src/TradeFlow.Api/TradeFlow.Api.csproj
```

不要把连接字符串写入已提交的`appsettings`文件。

## 每次启动

### 1. SQL Server

```powershell
docker compose `
  --file deploy/local/docker-compose.yml `
  --env-file deploy/local/.env `
  up -d
```

查看状态：

```powershell
docker compose `
  --file deploy/local/docker-compose.yml `
  --env-file deploy/local/.env `
  ps
```

### 2. API

打开第二个终端：

```powershell
dotnet run `
  --project backend/src/TradeFlow.Api/TradeFlow.Api.csproj `
  --launch-profile http
```

地址：

```text
http://localhost:5080/health/live
http://localhost:5080/health/ready
http://localhost:5080/openapi/v1.json
```

### 3. React

打开第三个终端：

```powershell
Set-Location frontend
pnpm dev
```

访问：

```text
http://localhost:5173
```

Vite会把`/api`和`/health`代理到本地API。

## 常用验证

后端：

```powershell
dotnet restore TradeFlow.sln --locked-mode
dotnet build TradeFlow.sln --no-restore
dotnet test TradeFlow.sln --no-build
```

前端：

```powershell
Set-Location frontend
pnpm install --frozen-lockfile
pnpm lint
pnpm typecheck
pnpm test:run
pnpm build
```

## 停止本地数据库

停止但保留数据：

```powershell
docker compose `
  --file deploy/local/docker-compose.yml `
  --env-file deploy/local/.env `
  stop
```

不要随意使用`down --volumes`，它会删除本地数据库Volume。
