# 系统架构与决策

## 1. 架构目标

- 支持真实企业流程和审计。
- 让小团队能够理解和维护。
- 在需要前保持单一部署单元。
- 通过模块边界降低耦合。
- 对失败、并发、安全和升级有明确处理。

## 2. 系统上下文

```text
Employees ──浏览器──> React Web ──HTTPS──> ASP.NET Core API
                                             │
                                             ├── Azure SQL
                                             ├── Blob Storage
                                             ├── Entra ID
                                             ├── Email Provider
                                             └── Xero
```

## 3. 部署容器

| 容器 | 责任 |
|---|---|
| React Web | 页面、交互、客户端体验和API调用 |
| ASP.NET Core API | 认证授权、业务规则、事务和集成 |
| Background Worker | Outbox、邮件、同步和定时任务 |
| Azure SQL | 事务业务数据 |
| Blob Storage | 发票、附件和导出文件 |
| Application Insights | 日志、指标和Trace |

API和Worker初期可来自同一代码库和发布Artifact，但可以作为不同进程运行。

## 4. 模块边界

| 模块 | 拥有的数据和行为 |
|---|---|
| Identity | 用户外部身份、本地档案 |
| Organisations | 企业、分支、成员、角色、权限 |
| Catalog | 产品、分类、税率、价格 |
| Purchasing | 采购申请、审批、采购订单 |
| Inventory | 仓库库存、预留、流水、盘点 |
| Sales | 报价、订单、发货 |
| Invoicing | 发票、Credit Note |
| Notifications | 邮件和站内通知 |
| Auditing | 不可变审计记录 |
| Reporting | 跨模块只读查询 |
| Integrations | Xero、Webhook和导入导出 |

模块通过公开Contract或Event协作。禁止模块直接修改其他模块拥有的表。

## 5. 模块内部结构

```text
Module/
├── Domain/
│   ├── Entities
│   ├── ValueObjects
│   ├── Rules
│   └── Events
├── Application/
│   ├── Commands
│   ├── Queries
│   └── Contracts
├── Infrastructure/
│   ├── Persistence
│   └── Integrations
└── Endpoints/
```

依赖方向：

```text
Endpoints → Application → Domain
Infrastructure → Application/Domain contracts
Domain → 不依赖数据库、HTTP或UI
```

## 6. 数据一致性

- 单个Aggregate在一个数据库事务中保持一致。
- 库存预留、订单状态、审计和Outbox在同一事务提交。
- 跨外部系统采用最终一致性。
- 外部网络调用不得放在长数据库事务内部。
- Consumer必须能够安全处理重复消息。

## 7. 错误与可观测性

- 预期业务失败返回Problem Details。
- 未预期异常由全局Middleware转换为安全错误。
- 每次请求生成或传播Trace ID。
- 日志使用结构化字段，不通过字符串拼接隐藏信息。
- Metrics至少包含请求量、错误率、延迟、数据库和后台任务。

## 8. 初始ADR

### ADR-001：模块化单体

- 状态：Accepted
- 决定：第一版使用模块化单体。
- 原因：团队和规模尚不需要微服务的部署、网络和一致性成本。
- 后果：必须用项目引用、架构测试和数据库Schema保持边界。

### ADR-002：SQL Server 2022与Azure SQL

- 状态：Accepted
- 决定：本地和集成测试使用SQL Server 2022，生产使用Azure SQL。
- 原因：与.NET、Entra和Azure生态一致，适合目标企业。
- 后果：需要管理T-SQL、索引、Migration、连接弹性和云成本。

### ADR-003：React SPA

- 状态：Accepted
- 决定：企业内部管理界面使用React SPA。
- 原因：交互密集、无需SEO，前后端职责清晰。
- 后果：必须处理Token、CORS、客户端路由和加载状态。

### ADR-004：Outbox

- 状态：Accepted
- 决定：业务事件先与业务数据一起保存到Outbox，再异步发送。
- 原因：避免数据库成功但消息丢失。
- 后果：需要Worker、重试、幂等和监控。

### ADR模板

后续决策复制[模板](../templates/project-templates.md)中的ADR部分，保存在`docs/adr/`。
