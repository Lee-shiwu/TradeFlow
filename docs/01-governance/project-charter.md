# 项目章程

状态：Draft  
Owner：Product Owner  
最后更新：填写日期

## 1. 项目名称

NZ Enterprise TradeFlow。

## 2. 背景和问题

目标企业当前通过Excel、邮件和纸质流程管理采购、库存与销售，产生以下问题：

- 审批记录分散，无法可靠审计。
- 库存数据更新不及时。
- 客户、供应商和产品重复维护。
- 销售无法获得实时可用库存。
- 财务需要重复录入发票和GST数据。
- 管理层缺少及时、可信的运营报表。

## 3. 项目目标

建设一个多企业、可审计、可持续维护的Web平台，统一：

- 采购申请与审批。
- 采购订单与收货。
- 库存流水、调拨与盘点。
- 报价、销售订单、预留与发货。
- 发票、Credit Note和GST基础数据。
- 权限、通知、报表和外部集成。

## 4. 目标用户

- Organisation Administrator
- Purchasing Officer
- Purchasing Manager
- Warehouse Operator
- Sales Representative
- Sales Manager
- Finance Officer
- Auditor

## 5. 可衡量业务结果

- 关键库存准确率达到99%或更高。
- 100%的采购审批可以追踪审批人和时间。
- 100%的库存变化存在对应库存流水。
- 跨企业数据访问测试必须全部通过。
- 高频页面P95响应时间达到约定目标。
- 生产数据库恢复演练成功。

## 6. 核心约束

- 使用稳定、受支持的技术版本。
- 使用Git和GitHub管理所有版本。
- 第一版采用模块化单体。
- 生产数据库采用Azure SQL Database。
- 默认业务区域为新西兰。
- 时间以UTC存储，以Pacific/Auckland展示。
- 不把Secrets或生产数据提交到Git。

## 7. Stakeholder与责任

| 角色 | 主要责任 | 姓名 |
|---|---|---|
| Sponsor | 业务方向和预算 | 待填写 |
| Product Owner | 范围、优先级和验收 | 待填写 |
| Business Manager | 业务规则和流程 | 待填写 |
| Technical Lead | 架构和技术质量 | 待填写 |
| Developer | 实现、测试和文档 | 待填写 |
| Operations | 环境、部署和生产支持 | 待填写 |

## 8. 主要里程碑

| 里程碑 | 完成条件 | 目标日期 |
|---|---|---|
| M0 项目批准 | 章程、范围、风险批准 | 待填写 |
| M1 业务蓝图 | MVP Story和规则批准 | 待填写 |
| M2 技术地基 | CI和本地环境可运行 | 待填写 |
| M3 主数据 | 主数据端到端完成 | 待填写 |
| M4 采购库存 | 采购到收货完成 | 待填写 |
| M5 销售发票 | 销售到发票完成 | 待填写 |
| M6 UAT | 关键场景验收通过 | 待填写 |
| M7 Production | 生产部署和验证完成 | 待填写 |

## 9. 批准

| 角色 | 决定 | 日期 |
|---|---|---|
| Product Owner | Pending | |
| Business Manager | Pending | |
| Technical Lead | Pending | |
