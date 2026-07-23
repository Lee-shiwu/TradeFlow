# 本地开发环境检查清单

本文件只定义需要安装和验证的工具。具体安装命令应以各产品官方文档为准。

## 必需工具

| 工具 | 用途 | 版本策略 |
|---|---|---|
| Git | 版本控制 | 当前稳定版 |
| GitHub账号 | 远程仓库、PR、Actions | 启用MFA |
| .NET SDK | 后端编译和测试 | .NET 10 LTS，固定在`global.json` |
| Node.js | 前端工具运行时 | Node.js 24 LTS |
| pnpm | 前端包管理 | 固定主版本并提交lock file |
| Docker Desktop | 本地SQL Server和集成测试 | 稳定版 |
| Visual Studio或Rider | C#开发 | 稳定版 |
| VS Code | 前端和文档，可选 | 稳定版 |
| SQL管理工具 | 查询和检查数据库 | SSMS或Azure Data Studio替代工具 |

## 验证命令

```powershell
git --version
dotnet --info
node --version
pnpm --version
docker version
docker compose version
```

## Git基础配置

```powershell
git config --global user.name "Your Name"
git config --global user.email "your-github-email@example.com"
git config --global init.defaultBranch main
```

邮箱应与GitHub账号设置匹配。企业设备应使用组织要求的签名、SSO和MFA策略。

## 本地安全规则

- 不把密码写进README。
- 不提交`.env`。
- 不提交真实连接字符串。
- 不在前端保存Client Secret。
- 不把生产数据复制到个人电脑。
- 使用`.env.example`说明变量名称，但只放示例值。

## 环境验收

- 所有验证命令成功。
- Docker能够启动测试容器。
- GitHub账户启用了MFA。
- 可以创建本地Commit。
- 可以通过SSH或HTTPS访问自己的GitHub仓库。
- 机器重启后工具仍然可用。
