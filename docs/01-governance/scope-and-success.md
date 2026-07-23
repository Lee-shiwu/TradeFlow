# 项目范围与成功指标

## MVP In Scope

### 平台能力

- Organisation、Branch、Warehouse。
- Entra ID登录。
- Membership、Role、Permission、Data Scope。
- 审计、通知、日志和监控。

### 主数据

- Supplier、Customer、Product Category、Product、Tax Rate、Price List。
- 分页、搜索、导入、导出、停用和并发控制。

### 采购

- Purchase Requisition。
- 审批矩阵。
- Purchase Order。
- 分批Goods Receipt。

### 库存

- Stock Ledger。
- On Hand、Reserved和Available。
- Transfer、Adjustment和Stocktake。

### 销售和发票

- Quote、Sales Order、Reservation、Picking和Shipment。
- Invoice、Credit Note和GST。

### 质量和交付

- 自动化测试。
- GitHub Actions。
- Azure测试和生产环境。
- 数据迁移、备份、恢复和运行手册。

## Out of Scope

- 完整会计总账和工资。
- 制造MRP。
- 原生手机应用。
- 高级CRM营销自动化。
- AI销售预测。
- 跨国家复杂税务引擎。
- 第一版微服务拆分。

Out of Scope需求应进入Backlog，不得在没有范围变更批准的情况下加入当前Release。

## 技术成功指标

| 指标 | 目标 | 测量方法 |
|---|---|---|
| API可用性 | 按项目SLO填写 | Application Insights |
| 常用API P95 | 小于约定值 | Performance Test与生产Metrics |
| Critical安全问题 | 0 | 安全扫描和评审 |
| 跨租户访问失败 | 100%被拒绝 | 自动化权限测试 |
| Migration可重复执行 | 全部环境成功 | CI和Staging演练 |
| 恢复能力 | 达到RPO/RTO | 季度恢复演练 |
| CI成功率 | main始终为绿 | GitHub Actions |

## 业务成功指标

| 指标 | 基线 | 目标 | Owner |
|---|---:|---:|---|
| 库存准确率 | 待测量 | ≥99% | Warehouse Manager |
| 平均采购审批时间 | 待测量 | 待确认 | Purchasing Manager |
| 无审批采购比例 | 待测量 | 0% | Finance |
| 重复订单比例 | 待测量 | <0.1% | Sales Manager |
| 可追踪库存变化 | 待测量 | 100% | Auditor |

## 范围变更流程

1. 创建GitHub Issue并描述业务价值。
2. 说明对范围、时间、成本、数据和安全的影响。
3. Product Owner决定接受、延后或拒绝。
4. 接受后更新Scope、Backlog和Release计划。
5. 重要变化创建ADR或业务决策记录。
