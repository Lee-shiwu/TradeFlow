# API与数据库标准

## 1. API标准

### 路径和版本

```text
GET    /api/v1/products
GET    /api/v1/products/{id}
POST   /api/v1/products
PUT    /api/v1/products/{id}
POST   /api/v1/products/{id}/deactivate
```

业务动作可以使用动作Endpoint，不必强行伪装成CRUD：

```text
POST /api/v1/purchase-requisitions/{id}/submit
POST /api/v1/purchase-requisitions/{id}/approve
POST /api/v1/sales-orders/{id}/confirm
```

### 状态码

| 状态 | 用途 |
|---:|---|
| 200 | 成功查询或更新 |
| 201 | 成功创建 |
| 204 | 成功且无需响应内容 |
| 400 | 请求格式或字段验证失败 |
| 401 | 未认证 |
| 403 | 已认证但无权限 |
| 404 | 资源在当前Organisation不存在 |
| 409 | 状态、并发、唯一性或库存冲突 |
| 429 | 请求频率超过限制 |
| 500 | 未预期服务器错误 |

### Problem Details

```json
{
  "type": "https://tradeflow.example/problems/insufficient-stock",
  "title": "Insufficient stock",
  "status": 409,
  "code": "INV_STOCK_INSUFFICIENT",
  "detail": "SKU-100 has only 3 units available.",
  "errors": {
    "quantity": ["Requested 5, available 3."]
  },
  "traceId": "00-abcd"
}
```

### 分页

- 默认`page=1`、`pageSize=20`。
- 最大`pageSize=100`。
- 排序字段使用白名单。
- 搜索输入限制长度。
- 大数据导出使用后台任务，不通过超大分页完成。

### API兼容性

- 增加可选字段通常向后兼容。
- 删除、重命名或改变字段含义属于Breaking Change。
- Breaking Change需要新API版本或明确迁移窗口。
- 不把EF Entity直接作为API响应。

## 2. 数据库标准

### Schema

```text
identity
organisation
catalog
purchasing
inventory
sales
invoicing
notification
audit
integration
```

### 通用字段

根据实体性质选择：

```text
Id
OrganisationId
CreatedAtUtc
CreatedBy
LastModifiedAtUtc
LastModifiedBy
RowVersion
IsActive
```

不可变流水通常没有`LastModified`。

### 数据类型

| 数据 | SQL Server类型建议 |
|---|---|
| 标识 | `uniqueidentifier` |
| 金额 | `decimal(19,4)` |
| 数量 | `decimal(18,4)` |
| UTC时间 | `datetime2` |
| 并发 | `rowversion` |
| 短文本 | 有最大长度的`nvarchar` |
| 布尔值 | `bit` |

禁止无原因使用`nvarchar(max)`，禁止用`float`保存金额。

### 约束和索引

- 外键由数据库强制。
- `OrganisationId + SKU`建立唯一索引。
- 高频过滤首先考虑`OrganisationId`。
- 索引由真实查询和执行计划驱动。
- 唯一性同时由数据库保护，不能只依赖前端检查。
- Check Constraint保护简单、稳定的数据规则。

### Migration规则

1. 一个业务PR包含对应Migration。
2. Migration名称表达业务变化。
3. 已部署Migration不得改写或删除。
4. 生产应用启动时不自动Migration。
5. Migration由独立部署Job执行。
6. 重要变化使用Expand-Migrate-Contract。
7. 大表变化先评估锁、耗时和回滚。

### 数据生命周期

- 产品、客户和供应商通常停用而不是删除。
- Draft且无下游引用的数据可以按规则删除。
- 库存、审批、发票和审计历史不可直接物理删除。
- 数据保留和隐私规则由业务与法律要求共同决定。
