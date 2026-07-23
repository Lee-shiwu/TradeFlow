# 业务蓝图

状态：Draft。阶段1需要由业务负责人确认。

## 1. 企业结构

```text
Organisation
├── Branch
│   ├── Department
│   └── Warehouse
├── Users
│   ├── Membership
│   ├── Roles
│   └── Data Scopes
├── Suppliers
├── Customers
└── Products
```

### 初始决定

- 所有业务数据属于一个Organisation。
- SKU在Organisation内唯一。
- Supplier和Customer在Organisation内共享。
- Warehouse属于Organisation，可与Branch关联。
- 用户可以加入多个Organisation，但每次请求只能在一个当前Organisation上下文中执行。

## 2. 角色和权限

| 角色 | 主要职责 |
|---|---|
| Organisation Administrator | 用户、角色和企业设置 |
| Purchasing Officer | 采购申请和采购订单 |
| Purchasing Manager | 审批和采购监管 |
| Warehouse Operator | 收货、调拨、盘点和发货 |
| Sales Representative | 报价和销售订单 |
| Sales Manager | 折扣和特殊订单审批 |
| Finance Officer | 发票、Credit Note和GST |
| Auditor | 只读访问审计和业务历史 |

权限采用`模块.资源.动作`：

```text
Purchasing.Requisition.Create
Purchasing.Requisition.Submit
Purchasing.Requisition.Approve
Inventory.GoodsReceipt.Create
Inventory.StockAdjustment.Approve
Sales.Order.Confirm
Invoicing.CreditNote.Create
Auditing.Entry.View
```

## 3. 采购到收货

```text
创建采购申请
  → 提交
  → 根据金额和部门确定审批人
  → 批准或拒绝
  → 创建采购订单
  → 发给供应商
  → 仓库分批收货
  → 创建库存流水
  → 全部收货后关闭
```

### 采购申请状态

```text
Draft → Submitted → Approved
                  ↘ Rejected → Draft
Approved → Cancelled
```

### 采购订单状态

```text
Draft → Issued → PartiallyReceived → FullyReceived → Closed
             ↘ Cancelled
```

## 4. 销售到发票

```text
创建报价
  → 客户接受
  → 转销售订单
  → 确认客户和价格
  → 预留库存
  → 拣货
  → 发货并扣减库存
  → 生成发票
  → 记录付款状态
```

### 销售订单状态

```text
Draft → Confirmed → Allocated → PartiallyShipped → Shipped → Closed
    ↘ Cancelled          ↘ Cancelled并释放未发库存
```

## 5. 库存模型

```text
Available = OnHand - Reserved
```

每次库存变化创建不可随意修改的Stock Transaction：

- GoodsReceipt
- SalesShipment
- TransferOut
- TransferIn
- AdjustmentIncrease
- AdjustmentDecrease
- StocktakeVariance

错误流水通过反向更正交易处理，不直接修改历史。

## 6. 金额和GST

- 默认货币为NZD。
- 金额使用`decimal`。
- 税率是带生效日期的主数据，不散落硬编码。
- 订单和发票保存计算时使用的税率快照。
- 已正式发布的发票通过Credit Note修正。
- 舍入规则在发票级和行级之间必须作出唯一决定并测试。

## 7. 审计要求

以下事件至少记录用户、时间、Organisation、实体、操作、结果和Trace ID：

- 登录和权限失败。
- 用户、角色和权限修改。
- 主数据创建、修改、停用。
- 审批、拒绝和取消。
- 库存调整和盘点。
- 发票与Credit Note。
- 批量导入和数据迁移。

## 8. 待业务确认

- 采购审批金额层级。
- 是否允许代理审批。
- 客户信用额度处理方式。
- 销售折扣审批阈值。
- 是否允许负库存。
- 超收容差比例。
- GST舍入规则。
- 发票编号格式。
- 数据保留年限。
