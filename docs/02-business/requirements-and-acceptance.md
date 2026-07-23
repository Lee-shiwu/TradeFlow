# 需求、业务规则和验收标准

## 1. Story模板

```text
ID：
标题：

作为 <角色>
我希望 <能力>
从而 <业务价值>

前置条件：
业务规则：
正常验收场景：
失败验收场景：
权限：
数据影响：
审计要求：
非功能要求：
```

## 2. Given/When/Then

```gherkin
Given <系统当前状态>
When <用户执行动作>
Then <可验证结果>
```

验收条件必须描述外部可观察结果，不要写“调用某个Service方法”。

## 3. 示例：提交采购申请

### Story

```text
ID：PUR-101

作为 Purchasing Officer
我希望提交一份完整采购申请
从而让经理根据支出规则审批
```

### 规则

```text
PUR-001：申请至少包含一个明细。
PUR-002：数量必须大于零。
PUR-003：供应商必须处于Active。
PUR-004：只有Draft可以提交。
PUR-005：创建人不能审批自己的申请。
```

### 正常场景

```gherkin
Given 采购申请处于Draft
And 至少包含一个有效明细
When 有权限用户提交
Then 状态变为Submitted
And 保存提交人和UTC时间
And 创建审批任务
And 写入审计记录
```

### 失败场景

```gherkin
Given 采购申请没有明细
When 用户提交
Then API返回400
And code等于PUR_REQUISITION_EMPTY
And 状态仍为Draft
```

## 4. 示例：确认销售订单

```text
SAL-001：只有Draft订单可以确认。
SAL-002：Customer必须Active。
SAL-003：确认时重新读取可用库存。
SAL-004：库存不足返回409。
SAL-005：重复请求不能重复预留。
SAL-006：跨Organisation库存不能参与计算。
```

并发场景：

```gherkin
Given 仓库只剩5件可用库存
And 两个订单分别请求5件
When 两个用户同时确认订单
Then 只有一个确认成功
And 另一个返回409
And 最终Available不小于0
```

## 5. Backlog拆分规则

一个Story应当：

- 由一个主要角色发起。
- 产生一个可演示业务结果。
- 包含数据库、后端、前端和测试。
- 可以单独部署而不破坏现有功能。
- 通常在几天内完成，而不是持续数周。

过大的Story应沿业务动作拆分：

```text
不要：完成整个采购模块

应当：
创建采购申请
编辑采购申请
提交采购申请
审批采购申请
生成采购订单
```

## 6. 错误目录

稳定错误码使用`模块_资源_原因`：

| Code | HTTP | 含义 |
|---|---:|---|
| `PUR_REQUISITION_EMPTY` | 400 | 采购申请没有明细 |
| `PUR_INVALID_STATUS` | 409 | 当前状态不允许操作 |
| `PUR_SELF_APPROVAL_NOT_ALLOWED` | 409 | 禁止自我审批 |
| `INV_STOCK_INSUFFICIENT` | 409 | 可用库存不足 |
| `INV_RECEIPT_EXCEEDS_BALANCE` | 409 | 收货超过未收数量 |
| `SEC_PERMISSION_DENIED` | 403 | 用户没有权限 |

不要把数据库异常文本直接返回给用户。

## 7. Requirements Traceability

每个需求应能够追踪：

```text
Business Goal
  → User Story
  → Business Rule
  → API / UI
  → Migration
  → Automated Test
  → Pull Request
  → Release
```
