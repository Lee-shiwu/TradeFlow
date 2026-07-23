# NZ Enterprise TradeFlow

面向新西兰中型贸易企业的采购、库存、销售、发票与审批管理平台。

本仓库同时是一套企业项目实战培训材料。目标不是复制代码，而是按照真实团队的工作方式，独立完成：

```text
业务分析 → 架构设计 → 开发 → 测试 → GitHub CI/CD → 部署 → 运维升级
```

## 从这里开始

1. 阅读[学习入口](docs/00-start-here/README.md)。
2. 按照[从零到生产部署路线](docs/00-start-here/roadmap-zero-to-production.md)逐阶段执行。
3. 不理解的词先查[专有名词词典](docs/00-start-here/glossary.md)。
4. 每个阶段达到 Exit Gate 后才能进入下一阶段。

## 稳定技术基线

- Backend：C#、.NET 10 LTS、ASP.NET Core 10、EF Core 10
- Frontend：React 19.2、TypeScript、Vite、Material UI、TanStack Query
- Database：SQL Server 2022；生产环境使用 Azure SQL Database
- Identity：Microsoft Entra ID
- Testing：xUnit、Testcontainers、Vitest、Testing Library、Playwright
- Delivery：Git、GitHub、GitHub Actions、Docker、Azure
- Observability：Serilog、OpenTelemetry、Application Insights

具体补丁版本由锁文件固定。只使用正式稳定版本，不使用 Preview、RC 或实验版本。

## 文档目录

| 目录 | 内容 |
|---|---|
| `docs/00-start-here` | 学习方法、总路线、环境准备、词典 |
| `docs/01-governance` | 项目章程、范围、成功指标、风险和完成标准 |
| `docs/02-business` | 业务蓝图、角色权限、状态机、需求和验收标准 |
| `docs/03-architecture` | 系统架构、技术决策、API和数据库标准 |
| `docs/04-engineering` | 仓库、Git、编码和代码评审规范 |
| `docs/05-quality` | 测试、安全、性能和质量门禁 |
| `docs/06-delivery` | 环境、CI/CD、部署和数据迁移 |
| `docs/07-operations` | 监控、备份、事件处理和版本升级 |
| `docs/templates` | User Story、ADR、发布和检查清单模板 |

## 当前状态

当前处于：`阶段 0——项目立项与学习准备`。

正式业务代码应在阶段 0～2 的文档完成评审后开始创建。
