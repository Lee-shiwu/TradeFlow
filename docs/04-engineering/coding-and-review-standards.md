# 编码与代码评审规范

## 1. 通用原则

- 名称表达业务意图。
- 方法保持单一职责。
- 不用注释解释混乱代码，先让代码清晰。
- 避免无业务价值的抽象。
- 不捕获异常后静默忽略。
- 不在日志中记录Token、密码或敏感数据。
- 所有外部I/O支持Cancellation。

## 2. C#规范

- 启用Nullable Reference Types。
- 警告视为错误。
- 使用Async API处理数据库和网络。
- 金额和数量使用`decimal`。
- 时间使用`DateTimeOffset`或明确UTC策略。
- Domain方法表达动作，例如`order.Confirm()`。
- 不允许Controller包含主要业务规则。
- 不允许Application层依赖HTTP类型。
- EF查询默认避免加载不需要的列和导航属性。
- 写操作显式定义事务边界。

## 3. React和TypeScript规范

- 启用TypeScript strict。
- 不用`any`绕过类型系统，例外必须说明。
- API Server State由TanStack Query管理。
- 表单使用React Hook Form和Schema验证。
- 组件区分Container和可复用Presentation责任。
- 页面必须处理Loading、Empty、Error和Success。
- Mutation提交期间防止重复操作。
- 错误根据稳定`code`处理，不解析英文`detail`。
- 权限隐藏只改善体验，安全仍由API保证。
- 组件满足键盘操作和基础无障碍要求。

## 4. 数据访问

- 列表查询必须分页。
- 查询只选择需要的字段。
- 避免N+1。
- 不在循环中逐条`SaveChanges`。
- 唯一性由数据库唯一索引最终保证。
- 并发实体使用`rowversion`。
- 重要SQL和索引通过执行计划验证。

## 5. 代码评审清单

评审者从以下角度检查：

### 业务

- 是否满足Story和规则？
- 非法状态是否被阻止？
- 金额、库存和权限是否正确？

### 安全

- 是否验证Authentication、Permission和Organisation？
- 是否有越权或批量赋值风险？
- 日志和响应是否泄漏敏感数据？

### 数据

- 事务边界是否正确？
- Migration是否安全且可部署？
- 是否需要唯一约束、外键或索引？
- 并发是否处理？

### API和UI

- 状态码和错误码是否稳定？
- Loading、Empty、Warning和Error是否完整？
- 是否防止重复提交？

### 测试和运维

- 正常、失败、边界和权限是否测试？
- 是否增加可诊断日志和Metrics？
- 部署、配置或回滚是否受影响？

## 6. 禁止事项

- 把真实Secret提交到Git。
- 在生产应用启动时自动执行Migration。
- 用前端验证代替后端规则。
- 直接修改已发布的库存流水或发票。
- 无分页返回整张大表。
- 使用空`catch`。
- 为了测试通过而删除关键断言。
- 复制未知来源的大段代码而不能解释。
