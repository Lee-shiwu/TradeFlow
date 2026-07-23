# 项目治理、风险与完成标准

## 工作节奏

- 每个Story只由一个明确Owner推进。
- 所有工作通过GitHub Issue跟踪。
- 所有代码通过Pull Request进入`main`。
- 每周评审Backlog、风险、缺陷和依赖。
- 每个Release进行Demo、验收和回顾。

## Definition of Ready

Story开始开发前必须满足：

- 用户和业务价值明确。
- 正常、失败和边界验收条件完整。
- 业务规则编号存在。
- 权限和数据范围明确。
- 对数据库、API和UI影响已识别。
- 外部依赖和阻塞已识别。
- 规模足够小，能在一个短周期内完成。

## Definition of Done

- 代码符合编码规范。
- 后端再次验证所有关键业务规则。
- 权限和Organisation隔离已实现。
- Migration经过评审。
- API契约和错误码已记录。
- 页面包含Loading、Empty、Success、Warning和Error状态。
- 单元、集成和必要E2E测试通过。
- 日志不包含Secrets或敏感数据。
- Pull Request评审完成。
- GitHub Actions全部通过。
- 测试环境部署并验收。
- 相关用户和运行文档已更新。

## 风险登记表

| ID | 风险 | 概率 | 影响 | 应对措施 | Owner | 状态 |
|---|---|---:|---:|---|---|---|
| R-001 | MVP持续扩大 | 高 | 高 | Scope和变更审批 | Product Owner | Open |
| R-002 | 库存并发导致超卖 | 中 | 高 | 事务、并发和真实DB测试 | Tech Lead | Open |
| R-003 | 旧数据质量低 | 高 | 高 | Profiling、清理、试迁移 | Business Owner | Open |
| R-004 | 权限或租户隔离缺陷 | 中 | 高 | Policy、过滤、自动化测试 | Security Owner | Open |
| R-005 | 外部集成失败 | 中 | 中 | Outbox、重试、人工补偿 | Integration Owner | Open |
| R-006 | 不受支持的依赖 | 中 | 高 | LTS策略和月度检查 | Tech Lead | Open |
| R-007 | 无法恢复数据库 | 低 | 极高 | 定期备份恢复演练 | Operations | Open |

## 决策等级

| 决策 | 决策人 | 需要记录 |
|---|---|---|
| User Story细节 | Product Owner | Issue/Story |
| 业务规则 | Business Manager | Business Rules |
| 架构和技术 | Technical Lead | ADR |
| 安全例外 | Security Owner | 风险接受记录 |
| 生产发布 | Product Owner + Operations | Release Record |

## 缺陷等级

| 等级 | 含义 | 示例 |
|---|---|---|
| Critical | 数据泄漏、数据破坏、系统不可用 | 跨租户读取、库存账损坏 |
| High | 核心流程无法完成 | 无法收货或发货 |
| Medium | 有替代办法但影响效率 | 导出失败 |
| Low | 轻微显示或体验问题 | 标签间距错误 |
