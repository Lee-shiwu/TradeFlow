# 部署与数据迁移运行手册

每次正式发布复制本文件的检查清单，填写版本、负责人和时间。

## 1. Release信息

```text
Release：
Commit SHA：
Artifact：
Migration：
Change Owner：
Technical Owner：
Planned Window：
Rollback Decision Owner：
```

## 2. 上线前检查

- [ ] PR和CI全部通过。
- [ ] Staging使用同一Artifact验证。
- [ ] UAT签字完成。
- [ ] Release Notes完成。
- [ ] 配置变化已评审。
- [ ] Migration已在Staging演练。
- [ ] 备份和恢复点确认。
- [ ] 上一版本Artifact可用。
- [ ] Smoke Test准备完成。
- [ ] Dashboard和告警可访问。
- [ ] 支持人员和业务联系人在线。
- [ ] 用户维护通知已发送。

## 3. 数据迁移流程

### Discover

- 识别所有旧文件、表和Owner。
- 统计记录数、空值、重复和非法值。
- 明确数据保留范围。

### Map

建立字段映射：

| Source | Target | Transform | Validation | Owner |
|---|---|---|---|---|
| Old SKU | Product.Sku | Trim + Uppercase | Organisation内唯一 | Catalog Owner |

### Clean

- 删除或合并重复数据。
- 修复非法日期和金额。
- 补充必填值。
- 不明确的数据交给业务Owner决定。

### Trial Migration

至少执行两次：

1. 导入到空测试数据库。
2. 生成拒绝记录报告。
3. 核对总数、金额、库存和未结单据。
4. 修复规则并重复。

### Final Migration

1. 冻结旧系统写入。
2. 生成最终备份和导出。
3. 运行经过版本控制的迁移程序。
4. 保存运行日志和Hash。
5. 执行业务核对。
6. 业务Owner签字。

## 4. 部署步骤

- [ ] 宣布部署开始。
- [ ] 记录当前版本和健康状态。
- [ ] 确认数据库恢复点。
- [ ] 运行Migration Job。
- [ ] 验证Schema版本。
- [ ] 部署Backend/Worker。
- [ ] 部署Frontend。
- [ ] 检查Health和Readiness。
- [ ] 执行Smoke Test。
- [ ] 检查错误率、延迟和数据库连接。
- [ ] 检查Outbox积压。
- [ ] 业务Owner验证关键流程。

## 5. Smoke Test

- [ ] 登录成功。
- [ ] 选择Organisation成功。
- [ ] 产品列表可打开。
- [ ] 创建和读取一条安全测试数据。
- [ ] 采购申请可创建。
- [ ] 库存查询可用。
- [ ] 权限拒绝行为正常。
- [ ] 日志中可以找到测试请求Trace ID。

测试数据必须有明确标识并按规则清理。

## 6. 回滚判定

考虑回滚：

- 核心流程不可用。
- 出现数据破坏。
- 出现跨租户或高危安全问题。
- 错误率显著超过阈值。
- Migration未完成且可以安全回退。

如果Migration已写入合法新业务数据，由Technical Owner决定Roll-forward、停止写入或补偿，不能盲目恢复旧数据库。

## 7. 上线后

- [ ] 发布完成通知。
- [ ] 创建GitHub Release和Tag。
- [ ] 保存部署记录。
- [ ] 观察至少一个约定监控窗口。
- [ ] 记录问题和人工操作。
- [ ] 安排上线后回顾。
