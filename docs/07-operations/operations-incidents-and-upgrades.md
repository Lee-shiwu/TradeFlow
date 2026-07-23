# 运维、事件与版本升级

## 1. 可观测性

系统必须提供三类信息：

### Logs

回答“发生了什么”。包含：

- Timestamp
- Level
- TraceId
- UserId或安全替代标识
- OrganisationId
- Operation
- Result
- Error Code

### Metrics

回答“系统整体表现如何”：

- 请求量。
- 4xx和5xx比例。
- P50/P95/P99延迟。
- 数据库连接和CPU。
- Outbox积压。
- 后台任务失败数。
- 登录和权限拒绝趋势。

### Traces

回答“一次请求经过哪里、慢在哪里”。

## 2. 告警

告警必须可执行，至少说明：

- 发生了什么。
- 影响哪个环境和功能。
- 当前值和阈值。
- Dashboard和Runbook链接。
- 谁负责响应。

避免为每一个单独404发送告警。

## 3. 备份与恢复

需要由业务确认：

```text
RPO：最多能接受丢失多长时间的数据
RTO：最多多久必须恢复服务
```

季度恢复演练：

1. 选择一个备份点。
2. 恢复到隔离环境。
3. 验证Schema和记录。
4. 执行关键业务查询。
5. 测量实际RTO。
6. 记录问题和改进。

“有备份”不等于“能够恢复”。

## 4. Incident响应

### Severity

| 等级 | 示例 |
|---|---|
| SEV-1 | 跨租户泄漏、数据破坏、全站不可用 |
| SEV-2 | 核心模块大面积不可用 |
| SEV-3 | 部分用户受影响且有替代方案 |
| SEV-4 | 轻微问题 |

### 处理步骤

1. Detect：确认告警真实。
2. Declare：确定等级和Incident Commander。
3. Contain：停止进一步损害，例如暂停写入或关闭Feature Flag。
4. Diagnose：使用Trace、Logs、Metrics和最近部署定位。
5. Recover：Rollback、Roll-forward或业务补偿。
6. Validate：技术与业务共同确认恢复。
7. Communicate：持续更新Stakeholder。
8. Review：完成无责Postmortem。

### Postmortem

记录：

- 用户影响。
- 时间线。
- 直接原因和系统性原因。
- 哪些防护有效或失效。
- 修复任务、Owner和截止日期。

## 5. 依赖更新策略

### Patch

- 每月评估。
- 安全补丁提高优先级。
- 在独立分支更新。
- 运行完整CI和相关回归。
- 先进入非生产环境。

### Minor

- 检查Release Notes和弃用项。
- 一次只更新相关依赖组。
- 验证编译、测试和UI。

### Major

1. 创建独立Epic和分支。
2. 阅读所有Breaking Changes。
3. 建立兼容性和回滚计划。
4. 在接近生产的环境验证。
5. 不与大型业务功能同一Release上线。

## 6. .NET和Node策略

- 只使用仍受支持的LTS。
- `global.json`固定.NET SDK范围。
- Node使用LTS并通过配置固定主版本。
- NuGet使用Central Package Management。
- pnpm lock file必须提交。
- 不允许生产安装时静默漂移版本。

## 7. SQL Server与Migration

- 定期应用受支持的安全更新。
- Azure SQL底层补丁由平台处理，应用仍负责查询、索引和容量。
- 每季度评审慢查询和缺失索引建议。
- 大版本升级先执行兼容性评估和恢复演练。
- 已部署Migration不可重写。

## 8. 固定运维节奏

| 周期 | 任务 |
|---|---|
| 每日 | 告警、失败任务、Outbox和集成检查 |
| 每周 | 错误趋势、慢查询、容量和未关闭Incident |
| 每月 | 安全补丁、依赖、成本和访问复核 |
| 每季度 | 恢复演练、权限复核、性能基线 |
| 每年 | LTS、大版本、架构和灾难恢复评审 |
