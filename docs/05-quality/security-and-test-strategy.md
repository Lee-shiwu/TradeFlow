# 安全与测试策略

## 1. 安全目标

- 正确识别用户。
- 只允许用户执行被授权的操作。
- 严格隔离不同Organisation的数据。
- 保护Secrets和敏感数据。
- 保证库存、审批和财务历史不被非法修改。
- 让安全事件可以追踪和响应。

## 2. 安全控制

### 身份认证

- 生产使用Microsoft Entra ID和OIDC。
- 强制组织要求的MFA和Conditional Access。
- API验证Issuer、Audience、签名和Token有效期。
- Client Secret绝不放入React前端。

### 授权

- 使用Policy和Permission。
- 每个写Endpoint明确要求权限。
- 敏感读操作同样检查权限。
- 前端按钮隐藏不构成安全控制。

### 租户隔离

- 每个业务请求解析当前Organisation。
- 所有查询和写入验证Organisation。
- 资源在其他Organisation存在时仍返回404或安全拒绝。
- 建立跨租户自动化测试套件。

### 输入与输出

- 服务端验证长度、格式、范围和业务状态。
- 使用参数化查询或EF Core。
- 限制上传大小、类型和内容。
- 错误响应不包含堆栈、SQL或连接字符串。
- 对导出和日志中的个人信息进行最小化。

### Secrets

- 本地使用User Secrets或不提交的环境文件。
- CI/CD使用GitHub Environments和OIDC。
- 生产使用Azure Key Vault和Managed Identity。
- 定期轮换并监控访问。

## 3. 测试金字塔

### Unit Test

验证纯业务规则：

- 金额计算。
- 状态转换。
- 审批级别。
- GST舍入。
- 可用库存计算。

特点：快、独立、不连接真实数据库。

### Integration Test

使用真实SQL Server容器验证：

- EF Mapping和Migration。
- 唯一约束、外键和`rowversion`。
- 事务。
- Organisation过滤。
- 并发库存。
- API认证和错误响应。

关键数据行为不能只依赖InMemory数据库。

### Component Test

验证React组件：

- 表单验证。
- Loading和错误显示。
- 权限控制的UI。
- API响应到页面的映射。

### End-to-End Test

使用Playwright验证少量高价值流程：

- 登录后创建采购申请并提交。
- 经理审批。
- 仓库收货并看到库存。
- 创建销售订单并预留。
- 库存不足时看到警告。

## 4. 每个Story最低测试集合

- 一个正常场景。
- 一个字段或业务验证失败。
- 一个权限失败。
- 一个跨Organisation失败。
- 一个非法状态失败。
- 数据敏感时增加并发测试。
- 集成敏感时增加重试和幂等测试。

## 5. 非功能测试

### 性能

- 测量P50、P95和P99。
- 记录测试数据规模。
- 检查慢SQL和执行计划。
- 关注分页、报表和批量导入。

### 恢复

- 数据库恢复。
- Blob恢复或版本保护。
- Artifact重新部署。
- Outbox重放。

### 安全

- 依赖漏洞扫描。
- Secret扫描。
- 静态分析。
- 权限矩阵测试。
- 文件上传测试。
- 限流测试。

## 6. CI质量门禁

Pull Request必须通过：

```text
Format/Lint
  → Compile
  → Unit Tests
  → Architecture Tests
  → Integration Tests
  → Frontend Tests
  → Dependency/Secret Scan
  → Production Build
```

Critical或High问题不能以“以后修复”为由进入生产，除非有正式风险接受。

## 7. 测试数据

- 使用Builder或Fixture创建最小数据。
- 测试互相独立。
- 不依赖执行顺序。
- 不使用真实客户数据。
- 时间相关测试注入Clock。
- 随机数据失败时必须可以复现。
