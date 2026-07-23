# 企业项目专有名词词典

本词典使用“简单解释 + 在本项目中的例子”。第一次遇到不理解的词时先查这里。

## 业务名词

| 名词 | 简单解释 | 本项目示例 |
|---|---|---|
| Tenant | 使用同一系统的一家独立企业 | Company A与Company B数据必须隔离 |
| Organisation | 企业主体 | 一家新西兰批发公司 |
| Branch | 企业分支机构 | Auckland Branch |
| Warehouse | 存放库存的地点 | Auckland Main Warehouse |
| SKU | 可销售或存储产品的唯一编号 | `NZ-CHAIR-001` |
| Master Data | 被多个流程重复使用的基础数据 | 产品、客户、供应商、仓库 |
| Purchase Requisition | 企业内部的采购申请 | 员工申请购买50台显示器 |
| Purchase Order / PO | 正式发给供应商的采购订单 | 审批后发送给供应商 |
| Goods Receipt | 仓库确认收到商品的记录 | PO分两次收货 |
| Stock on Hand | 仓库实际拥有的数量 | 仓库中有10件 |
| Reserved Stock | 已分配给订单但尚未发货的数量 | 10件中有4件已预留 |
| Available Stock | 当前还能分配的数量 | `10 - 4 = 6` |
| Stock Ledger | 每一次库存变化的不可随意修改记录 | 收货+10、发货-3 |
| Sales Quote | 给客户的报价 | 有有效期但还不是订单 |
| Sales Order | 客户确认购买后的订单 | 需要预留库存 |
| Invoice | 要求客户付款的正式单据 | 发货后生成发票 |
| Credit Note | 对原发票进行冲销的单据 | 退货后减少应收金额 |
| GST | 新西兰商品与服务税 | 税率作为配置数据管理 |
| Approval Matrix | 按金额、部门等决定谁审批的规则 | 超过NZD 1,000由经理审批 |
| Audit Trail | 谁在什么时候做了什么的历史记录 | 用户A在10:31批准PO |
| UAT | 业务用户验收测试 | 仓库主管验证收货流程 |

## 需求和项目管理名词

| 名词 | 简单解释 |
|---|---|
| Stakeholder | 受到项目影响或能决定项目方向的人 |
| Scope | 当前项目承诺实现的边界 |
| MVP | 能产生实际价值的最小可用版本，不等于粗糙Demo |
| User Story | 从用户角度描述需求的短句 |
| Acceptance Criteria | 判断需求是否完成的明确条件 |
| Definition of Ready / DoR | 一个任务可以开始开发前必须具备的条件 |
| Definition of Done / DoD | 一个任务真正完成必须满足的条件 |
| Backlog | 尚未完成并经过排序的需求列表 |
| Sprint | 团队集中完成一批工作的一段固定时间 |
| RACI | Responsible、Accountable、Consulted、Informed责任模型 |
| Risk Register | 记录风险、概率、影响和应对方法的表 |
| Exit Gate | 当前阶段必须达到的退出条件 |
| ADR | Architecture Decision Record，记录架构决定及原因 |

## 后端和架构名词

| 名词 | 简单解释 | 本项目用途 |
|---|---|---|
| Modular Monolith | 一个部署单元，内部按业务模块隔离 | 第一版总体架构 |
| Microservice | 可独立部署和扩展的小型服务 | 只有出现真实需求时才拆分 |
| Domain | 业务问题及其规则 | 采购、库存和销售 |
| Entity | 有唯一身份、会随时间变化的对象 | SalesOrder |
| Value Object | 由值定义、通常不可变的对象 | Money、Address |
| Aggregate | 必须在同一事务中保持一致的一组对象 | SalesOrder及其Lines |
| Invariant | 无论如何都不能被破坏的业务条件 | 可用库存不能小于零 |
| Use Case | 用户希望系统完成的一项操作 | ConfirmSalesOrder |
| Dependency Injection / DI | 由框架提供依赖对象 | 注入DbContext或Clock |
| Middleware | 处理所有HTTP请求的管道组件 | 异常处理、日志、认证 |
| DTO | API层传输的数据结构 | CreateProductRequest |
| Mapping | 在DTO、Domain和数据库对象间转换 | Request转Command |
| Repository | 对数据访问进行抽象的模式 | 只在能表达业务意图时使用 |
| Unit of Work | 将一组数据库修改作为一个工作单元提交 | EF Core DbContext |
| Domain Event | 领域中已经发生的重要事实 | PurchaseOrderApproved |
| Integration Event | 向模块或外部系统发布的事件 | InvoiceCreated |
| Outbox Pattern | 业务数据和待发送消息在同一事务保存 | 防止订单成功但消息丢失 |
| Idempotency | 同一请求执行多次，结果仍等同于一次 | 防止重复确认订单 |

## API和Web名词

| 名词 | 简单解释 |
|---|---|
| API | 前端或其他系统调用后端的接口 |
| Endpoint | 一个具体API地址和HTTP方法 |
| REST | 按资源和HTTP语义组织API的风格 |
| HTTP Status Code | 表示请求结果的标准数字，如200、400、409 |
| Problem Details | API返回结构化错误的标准格式 |
| Authentication | 证明“你是谁” |
| Authorization | 判断“你能做什么” |
| OAuth 2.0 | 授权框架 |
| OpenID Connect / OIDC | 在OAuth 2.0之上提供用户登录身份 |
| Access Token | 调用API时携带的短期凭证 |
| CORS | 控制哪些Web来源可以调用API |
| Pagination | 分页读取大量数据 |
| Rate Limiting | 限制请求频率，保护系统 |
| Trace ID | 串联一次请求相关日志的标识 |
| SPA | 页面主要由浏览器中的JavaScript运行的应用 |
| Server State | 来自API的数据，如产品列表 |
| Client State | 浏览器内部状态，如侧边栏是否展开 |

## 数据库名词

| 名词 | 简单解释 |
|---|---|
| Primary Key | 一行数据的唯一标识 |
| Foreign Key | 保证两张表引用关系有效的约束 |
| Unique Index | 保证某些字段组合不重复 |
| Composite Index | 由多个列组成的索引 |
| Transaction | 一组操作全部成功或全部失败 |
| Isolation Level | 并发事务彼此能看到什么数据的规则 |
| Optimistic Concurrency | 保存时检查数据是否已被别人修改 |
| `rowversion` | SQL Server用于并发检测的二进制版本值 |
| Migration | 受版本控制的数据库结构变更 |
| Schema | 数据库中用于组织表的命名空间 |
| N+1 Query | 先查一批数据，再为每条数据额外查询一次的性能问题 |
| Execution Plan | SQL Server执行查询的方法 |
| Seed Data | 环境初始化时需要的基础数据 |
| Soft Delete | 标记为已删除但保留数据 |

## 测试与交付名词

| 名词 | 简单解释 |
|---|---|
| Unit Test | 只验证一个小型规则，不连接真实外部系统 |
| Integration Test | 验证多个组件和真实数据库如何协作 |
| End-to-End / E2E | 从浏览器开始验证完整用户流程 |
| Test Double | 测试中替代真实依赖的对象 |
| Testcontainers | 测试时启动真实数据库容器 |
| Smoke Test | 部署后快速确认关键功能能运行 |
| Regression | 新改动破坏了原本正常的功能 |
| CI | 每次提交自动编译、测试和检查 |
| CD | 自动把合格版本交付到环境 |
| Artifact | CI构建产生、可部署且不可变的文件或镜像 |
| Environment | Development、Test、Staging、Production等运行区域 |
| Rollback | 将应用恢复到上一个可用版本 |
| Roll-forward | 通过发布修正版解决生产问题 |
| Blue/Green | 同时保留新旧环境，切换流量完成发布 |
| Feature Flag | 不重新部署就能控制功能开关 |

## Git名词

| 名词 | 简单解释 |
|---|---|
| Repository | Git管理的项目目录 |
| Commit | 一次有意义的版本记录 |
| Branch | 独立开发线路 |
| Pull Request / PR | 请求评审并合并代码 |
| Merge Conflict | 两个改动修改相同位置产生冲突 |
| Squash Merge | 将分支多个Commit整理成一个合并到main |
| Tag | 指向特定Commit的版本标记，如`v1.0.0` |
| Semantic Versioning | 使用`Major.Minor.Patch`表达版本变化 |
| Protected Branch | 禁止绕过评审直接修改的分支 |

## 运维名词

| 名词 | 简单解释 |
|---|---|
| Logging | 记录系统发生了什么 |
| Metrics | 可统计的数值，如错误率和响应时间 |
| Tracing | 跟踪一次请求经过了哪些组件 |
| Observability | 通过日志、指标和Trace理解系统内部状态 |
| SLA | 对客户承诺的服务可用性目标 |
| SLO | 团队内部衡量可靠性的目标 |
| RPO | 最多可以接受丢失多长时间的数据 |
| RTO | 发生故障后最多多久必须恢复 |
| Runbook | 可重复执行的操作步骤 |
| Incident | 影响用户或生产服务的异常事件 |
| Postmortem | 故障后无责复盘原因和改进措施 |
| Patch | 通常包含缺陷或安全修复的小版本更新 |
| LTS | Long Term Support，长期支持版本 |
