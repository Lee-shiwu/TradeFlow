# 从零到生产部署路线

这是整个项目的主执行文档。请严格按顺序推进。每个阶段都包含学习目标、任务、输出、Git检查点和退出条件。

## 路线总览

| 阶段 | 主题 | 最终结果 |
|---|---|---|
| 0 | 立项与准备 | 知道为什么做、做什么、不做什么 |
| 1 | 业务分析 | 业务流程、规则、角色和验收标准明确 |
| 2 | 架构设计 | 模块、数据库、API、安全和部署方向明确 |
| 3 | 工程初始化 | 前后端可编译、测试，GitHub CI通过 |
| 4 | 企业基础能力 | 错误、日志、验证、审计、时间等统一 |
| 5 | 身份和租户 | 登录、权限、企业数据隔离可靠 |
| 6 | 主数据 | 企业、仓库、供应商、客户、产品可管理 |
| 7 | 采购 | 申请、审批、采购订单流程完整 |
| 8 | 库存 | 收货、流水、调拨、盘点和并发可靠 |
| 9 | 销售 | 报价、订单、预留和发货完整 |
| 10 | 发票和GST | 发票、Credit Note和税额计算正确 |
| 11 | 通知和异步处理 | 通知可靠、失败可重试 |
| 12 | 报表和集成 | 管理报表、导入导出、Xero边界完成 |
| 13 | 质量强化 | 安全、性能、并发和恢复经过验证 |
| 14 | CI/CD和环境 | 可重复构建并部署到测试环境 |
| 15 | UAT和数据迁移 | 业务验收和生产数据准备完成 |
| 16 | 生产上线 | 可监控、可回滚、可恢复地上线 |
| 17 | 运维和升级 | 建立长期补丁、事件和迭代机制 |

---

## 阶段0：立项与学习准备

### 你要理解

- 企业项目先确定业务价值和责任，不是先创建Controller。
- Scope用于保护交付边界。
- 成功指标必须可以测量。

### 按顺序执行

1. 阅读本目录的学习入口、学习方法和词典。
2. 完成本地环境检查。
3. 填写项目章程。
4. 定义Stakeholder与RACI。
5. 确定MVP的In Scope和Out of Scope。
6. 填写成功指标。
7. 建立风险登记表。
8. 定义Definition of Ready和Definition of Done。
9. 建立GitHub Project或Issues的初始Backlog。

### 输出文档

- [项目章程](../01-governance/project-charter.md)
- [范围与成功指标](../01-governance/scope-and-success.md)
- [治理、风险和完成标准](../01-governance/governance-risk-and-done.md)

### Git检查点

```text
docs(project): define charter scope and success metrics
docs(project): add governance risks and delivery standards
```

### Exit Gate

- 能用两分钟解释项目解决的业务问题。
- MVP包含和不包含的内容明确。
- 每个主要风险有责任人和应对措施。
- 所有必需工具已经安装并验证。

---

## 阶段1：业务分析与需求拆分

### 你要理解

- User Story描述用户价值，Acceptance Criteria描述如何判断完成。
- 状态机决定业务对象可以如何变化。
- 权限是“动作 + 数据范围”，不只是角色名称。

### 按顺序执行

1. 建立业务词汇表。
2. 确定Organisation、Branch、Department、Warehouse关系。
3. 访谈或模拟访谈采购、仓库、销售、财务用户。
4. 绘制当前流程As-Is。
5. 设计目标流程To-Be。
6. 定义角色和权限矩阵。
7. 定义采购申请、采购订单、销售订单、发票状态机。
8. 为业务规则分配稳定编号，例如`INV-001`。
9. 编写User Story和Given/When/Then验收条件。
10. 按Must、Should、Could、Won't划分优先级。
11. 把MVP拆成可独立演示的垂直切片。

### 第一批垂直切片

```text
企业与仓库
  → 供应商
  → 产品
  → 采购申请
  → 审批
  → 采购订单
  → 收货
  → 库存查询
  → 销售订单
  → 库存预留
  → 发货
```

### 输出文档

- [业务蓝图](../02-business/business-blueprint.md)
- [需求、规则和验收标准](../02-business/requirements-and-acceptance.md)

### Exit Gate

- 主要角色、流程和状态已确认。
- 每个MVP Story都有正常和失败验收场景。
- 创建人能否审批自己的申请等关键问题已经决定。
- 开发者不需要凭感觉猜测核心业务规则。

---

## 阶段2：架构和技术设计

### 你要理解

- 架构用于控制依赖、变化和风险，不是展示复杂名词。
- 第一版采用模块化单体；只有真实需求出现时才拆微服务。
- 数据库、API、安全和运维必须在开发前有共同标准。

### 按顺序执行

1. 确认稳定技术基线。
2. 划分Identity、Organisations、Catalog、Purchasing、Inventory等模块。
3. 定义模块允许的依赖方向。
4. 绘制Context、Container和Component级架构图。
5. 决定数据库Schema、主键、金额、时间和并发规则。
6. 定义REST API、分页、Problem Details和HTTP状态码。
7. 设计Entra ID认证和Policy授权。
8. 设计`OrganisationId`数据隔离。
9. 设计审计、Outbox、幂等和可观测性。
10. 记录ADR及被放弃的替代方案。
11. 对架构进行简单威胁建模。

### 输出文档

- [架构与ADR](../03-architecture/architecture-and-decisions.md)
- [API与数据库标准](../03-architecture/api-and-database-standards.md)
- [安全与测试策略](../05-quality/security-and-test-strategy.md)

### Exit Gate

- 每个模块负责什么、不能做什么都清楚。
- API和数据库有统一规则。
- 身份、权限和租户隔离有完整方案。
- 每个重要技术选择都有原因和后果。

---

## 阶段3：仓库和工程初始化

### 你要理解

- 工程骨架必须让任何开发者从全新Clone开始成功运行。
- CI中的命令必须与本地一致。
- 依赖版本必须锁定，Secrets不能进入Git。

### 按顺序执行

1. 创建GitHub Repository并保护`main`。
2. 添加README、`.gitignore`、`.editorconfig`和`.gitattributes`。
3. 创建`.NET 10` Solution。
4. 创建API、BuildingBlocks和首批Module项目。
5. 创建Unit、Integration、Architecture测试项目。
6. 创建React、JavaScript（JSX）和Vite前端。
7. 配置Oxlint、Formatter、JSDoc数据结构说明和测试。
8. 创建SQL Server 2022本地Docker Compose。
9. 创建`global.json`、Central Package Management和lock files。
10. 实现`/health`和前端API状态页。
11. 添加最小后端、前端和E2E测试。
12. 添加GitHub Actions。
13. 从全新目录执行一次Clone-to-Run验证。
14. 通过Pull Request合并并标记`v0.1.0`。

### 操作规范

- 使用[仓库与Git工作流](../04-engineering/repository-and-git-workflow.md)。
- 使用[编码和评审规范](../04-engineering/coding-and-review-standards.md)。

### Exit Gate

- 后端和前端能编译。
- SQL Server本地环境可启动。
- 所有初始测试通过。
- GitHub Actions通过。
- 新开发者只根据README即可运行项目。

---

## 阶段4：企业基础能力

### 实现顺序

1. 全局异常处理中间件。
2. Problem Details和稳定业务错误码。
3. FluentValidation或等价验证边界。
4. Serilog结构化日志。
5. Trace ID和OpenTelemetry。
6. UTC时间抽象和Auckland显示策略。
7. Current User和Current Organisation抽象。
8. 审计拦截器或显式审计服务。
9. Health、Readiness和Liveness检查。
10. API版本、分页和排序公共模型。

### 必测场景

- 未处理异常不泄漏堆栈或连接字符串。
- 每个错误响应包含`traceId`。
- 请求验证失败返回稳定字段结构。
- 日志可以按一次请求关联。

### Exit Gate

- 后续模块不需要自行发明错误、日志和验证方式。
- 基础能力具有自动化测试和使用示例。

---

## 阶段5：身份、权限和租户隔离

### 实现顺序

1. 本地开发认证方案。
2. Entra ID应用注册说明。
3. 用户档案与外部身份关联。
4. Organisation和Membership。
5. Permission常量与Policy。
6. 角色作为权限集合。
7. Branch和Warehouse数据范围。
8. 每个数据库查询的Organisation过滤。
9. 权限拒绝和跨租户测试。

### Exit Gate

- A企业用户无法读取或修改B企业数据。
- 前端隐藏按钮之外，API仍独立验证权限。
- 禁用成员无法继续进行新业务操作。

---

## 阶段6：主数据

按以下顺序一次完成一个垂直切片：

1. Organisation与Branch。
2. Warehouse。
3. Supplier。
4. Customer。
5. Product Category。
6. Product与SKU。
7. Tax Rate与Price List。
8. CSV导入和错误报告。

每个切片必须包含列表、详情、新增、编辑、停用、分页、唯一性、并发和审计测试。

### Exit Gate

- 采购和销售需要的基础数据已经稳定。
- 重要数据使用停用而不是随意物理删除。
- 并发编辑不会静默覆盖。

---

## 阶段7：采购

### 实现顺序

1. 创建和编辑Draft采购申请。
2. 提交申请。
3. 计算审批级别。
4. 审批和拒绝。
5. 防止自我审批。
6. 从申请创建采购订单。
7. 发送、取消和关闭采购订单。
8. 审批通知和审计。

### 核心测试

- 无明细不能提交。
- 金额决定正确审批人。
- 非法状态转换被拒绝。
- 重复提交具有幂等性。
- 跨租户访问被拒绝。

---

## 阶段8：库存

### 实现顺序

1. 采购订单收货。
2. 不可变库存流水。
3. On Hand、Reserved、Available查询。
4. 分批收货和超量收货验证。
5. 仓库调拨。
6. 库存调整及审批。
7. 盘点。
8. 低库存规则。

### 核心要求

- 库存不能只保存在Product表的一个数字中。
- 收货、发货、调拨和调整均产生Stock Transaction。
- 事务和并发测试必须使用真实SQL Server。

---

## 阶段9：销售

### 实现顺序

1. 报价及有效期。
2. 报价转销售订单。
3. 价格、折扣和税额计算。
4. 确认订单。
5. 原子库存预留。
6. Picking List。
7. 发货和库存扣减。
8. 取消和释放预留。

### 核心要求

- 两个请求竞争最后库存时只有一个成功。
- 库存不足返回`409 Conflict`。
- 前端保留输入并给出可执行提示。

---

## 阶段10：发票和GST

### 实现顺序

1. 从已发货数量生成发票。
2. 区分含税和未税金额。
3. 以配置方式应用GST。
4. 发票编号和唯一性。
5. Credit Note。
6. PDF生成和存储。
7. 付款状态基础记录。

### 核心要求

- 金额使用`decimal`，定义统一舍入点。
- 已发正式发票不直接修改，通过Credit Note修正。
- 使用表格驱动测试验证税额和舍入。

---

## 阶段11：通知和异步处理

### 实现顺序

1. Outbox表和同事务写入。
2. 后台Publisher。
3. Email和站内通知接口。
4. 失败重试、退避和最大次数。
5. Dead-letter或人工补偿流程。
6. 幂等Consumer。

### Exit Gate

- 邮件服务故障不会撤销已成功的业务事务。
- 重启服务不会丢失待发送消息。
- 重复处理不会重复创建业务结果。

---

## 阶段12：报表、导入导出和集成

### 实现顺序

1. 销售、采购和库存Dashboard。
2. 分页明细报表。
3. CSV/Excel导出。
4. 安全的批量导入。
5. Xero集成边界与模拟实现。
6. Webhook签名和重放保护。
7. 集成同步状态与人工重试界面。

不要让报表直接破坏业务写模型性能。复杂报表应使用专门查询和索引。

---

## 阶段13：质量强化

### 执行

1. 对照威胁模型进行安全测试。
2. 执行权限矩阵自动化测试。
3. 测试并发库存和重复请求。
4. 测量关键API的P50、P95和P99。
5. 检查慢查询、执行计划和N+1。
6. 执行依赖和Secret扫描。
7. 测试文件上传限制。
8. 测试备份恢复。
9. 完整执行端到端回归。

### Exit Gate

- 所有Critical和High缺陷关闭。
- 性能达到已定义SLO。
- 已知风险记录且由负责人接受。

---

## 阶段14：CI/CD和测试环境

使用[CI/CD与环境](../06-delivery/cicd-and-environments.md)。

### 执行

1. 创建Development、Test、Staging、Production环境。
2. 使用Infrastructure as Code创建云资源。
3. Build一次并产生不可变Artifact。
4. 自动部署到Development/Test。
5. 对Staging和Production设置审批。
6. 独立执行数据库Migration Job。
7. 部署后自动Smoke Test。
8. 验证Rollback和Roll-forward。

---

## 阶段15：UAT和数据迁移

### 执行

1. 业务用户根据Acceptance Criteria执行UAT。
2. 将缺陷分级并修复。
3. 冻结生产发布范围。
4. 清理旧Excel数据。
5. 建立字段映射和数据转换规则。
6. 至少执行两次试迁移。
7. 核对记录数量、库存数量、未结订单和金额。
8. 形成业务签字记录。

使用[部署和迁移运行手册](../06-delivery/deployment-and-migration-runbook.md)。

---

## 阶段16：生产上线

### 上线前

1. 完成Go/No-Go会议。
2. 确认备份和恢复点。
3. 确认监控、告警和联系人。
4. 确认Migration向后兼容。
5. 确认上一版本Artifact可用。
6. 通知用户维护窗口。

### 上线中

1. 暂停旧系统写入。
2. 完成最终数据迁移。
3. 执行数据库Migration。
4. 部署应用。
5. 执行Smoke Test。
6. 业务负责人验证关键流程。
7. 开放用户访问。

### 上线后

1. 观察错误率、延迟和数据库指标。
2. 检查Outbox和后台任务。
3. 核对第一批采购、库存和销售记录。
4. 进入Hypercare支持期。
5. 创建GitHub Release和Tag。

---

## 阶段17：运维、升级和持续迭代

使用[运维、事件与版本升级](../07-operations/operations-incidents-and-upgrades.md)。

### 固定节奏

- 每日：检查告警、失败任务和关键集成。
- 每周：评审错误趋势、慢查询和容量。
- 每月：评估安全补丁和依赖更新。
- 每季度：恢复演练、权限复核、索引评审。
- 每年：评估LTS、数据库和基础设施大版本。

每项新需求重新进入：

```text
业务分析 → 设计 → 垂直切片 → 测试 → PR → 部署 → 观察
```

## 完成项目的最终证据

- 业务人员能够独立完成采购到收货、销售到发货流程。
- GitHub保存代码、PR、Release和部署历史。
- 数据库可备份并在目标RTO内恢复。
- 生产问题能通过Trace ID定位。
- 新开发者只依赖仓库文档即可启动。
- 依赖升级和数据库Migration有安全流程。
- 你能够不看教程解释每个关键设计的原因。
