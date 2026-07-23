# 新手学习与实施方法

## 角色转换

在这个项目中，你需要轮流承担四种角色：

| 角色 | 需要思考的问题 |
|---|---|
| Business Analyst | 企业为什么需要这个功能？规则是什么？ |
| Developer | 如何用可靠、清晰的代码实现？ |
| Tester | 哪些正常、异常和边界情况可能失败？ |
| Operator | 部署后如何监控、恢复和升级？ |

## 一个功能的标准训练方式

以“确认销售订单时库存不足”为例。

### 第一步：描述业务

销售人员确认订单前，系统必须保证每个仓库有足够的可用库存，防止企业承诺无法发出的商品。

### 第二步：写出规则

```text
SAL-001：只有 Draft 状态订单可以确认。
SAL-002：停用客户不能创建新订单。
SAL-003：可用库存必须大于或等于需要预留的数量。
SAL-004：两个用户并发确认时不能产生负库存。
```

### 第三步：确定数据

需要读取：

- SalesOrder
- SalesOrderLine
- Product
- WarehouseStock
- StockReservation

需要写入：

- SalesOrder 状态
- StockReservation
- AuditEntry
- OutboxMessage

### 第四步：确定后端处理

```text
认证用户
  → 验证权限和 OrganisationId
  → 开始数据库事务
  → 读取订单和库存
  → 验证订单状态
  → 验证实时可用库存
  → 创建库存预留
  → 更新订单状态
  → 写审计和Outbox
  → 提交事务
```

### 第五步：确定失败方式

库存不足时返回 HTTP `409 Conflict`：

```json
{
  "title": "Insufficient stock",
  "status": 409,
  "code": "INV_STOCK_INSUFFICIENT",
  "detail": "SKU-100 has only 3 units available.",
  "traceId": "00-abcd..."
}
```

React 使用 TanStack Query 的 mutation 捕获错误，将业务错误显示为警告，并保留用户已输入的数据。

### 第六步：确定测试

- 足够库存时成功。
- 库存不足时返回409。
- 无权限时返回403。
- 订单不存在时返回404。
- 订单已经确认时返回409。
- 两个并发请求只有一个成功。
- A企业不能预留B企业的库存。

### 第七步：提交Git

```text
feat(sales): reserve stock when confirming an order
test(sales): cover insufficient and concurrent stock allocation
```

## 如何判断自己真正掌握

完成一个功能以后，在不看代码的情况下回答：

1. 这项功能解决什么业务问题？
2. 最重要的三条业务规则是什么？
3. 数据库事务从哪里开始、在哪里结束？
4. 为什么使用这个HTTP状态码？
5. 前后端分别负责什么验证？
6. 哪个测试最能防止生产事故？
7. 如果部署失败如何回滚？

不能回答时，回到相应文档或代码重新学习。
