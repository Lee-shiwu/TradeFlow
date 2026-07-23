# CI/CD与环境策略

## 1. 环境

| 环境 | 用途 | 数据 |
|---|---|---|
| Local | 单人开发 | 合成测试数据 |
| Development | 团队集成 | 合成数据 |
| Test | 自动化和手工测试 | 合成数据 |
| Staging | 生产前演练和UAT | 脱敏或合成数据 |
| Production | 真实业务 | 受保护生产数据 |

禁止直接把生产数据库复制到低级环境。

## 2. Build Once, Deploy Many

同一个Commit只构建一次Artifact：

```text
Commit SHA
  → Backend Artifact / Container Image
  → Frontend Artifact
  → SBOM和测试报告
  → Development
  → Test
  → Staging
  → Production
```

不同环境只改变配置，不重新编译源代码。

## 3. Pull Request Pipeline

```text
Checkout
  → Restore locked dependencies
  → Format/Lint
  → Backend Build
  → Unit Tests
  → SQL Server Integration Tests
  → Architecture Tests
  → Frontend Type Check
  → Frontend Tests
  → Production Build
  → Security Scans
```

任何一步失败都不能合并。

## 4. Main Pipeline

合并`main`后：

1. 使用Commit SHA构建版本。
2. 生成不可变Artifact。
3. 生成版本和依赖清单。
4. 部署到Development。
5. 运行Smoke Test。
6. 将同一Artifact提升到Test。

## 5. Production Pipeline

1. 选择已在Staging验证的Artifact。
2. 要求GitHub Environment审批。
3. 检查维护窗口和变更记录。
4. 创建或确认恢复点。
5. 执行向后兼容Migration。
6. 部署应用。
7. 执行Smoke Test。
8. 观察关键Metrics。
9. 完成发布记录。

## 6. 数据库部署

- Migration不由Web应用启动自动执行。
- Migration作为独立Job运行。
- Job记录Migration名称、开始时间、结束时间和结果。
- 大表变更必须在Staging使用接近生产的数据规模演练。
- 破坏性Contract变化延迟到旧代码完全退出后执行。

Expand-Migrate-Contract示例：

```text
Release A：增加新列，旧列仍可用
Release B：后台迁移数据，应用双读或切换读取
Release C：所有代码只使用新列
Release D：确认无旧版本后删除旧列
```

## 7. 配置与Secrets

- 非敏感配置使用环境变量或配置服务。
- Secret使用Key Vault。
- GitHub到Azure优先使用OIDC，不保存长期Azure密码。
- 每个环境使用独立身份和最小权限。
- 修改生产配置必须有审计记录。

## 8. 回滚策略

应用失败优先部署上一个已知正常Artifact。数据库变化必须提前保证旧应用仍能运行。

如果数据已经发生不可逆业务变化，通常使用Roll-forward修复，不能简单恢复旧数据库覆盖合法新交易。

## 9. 环境验收

- 环境可以通过IaC重复创建。
- 同一Artifact经过所有环境。
- Production需要审批。
- Migration和应用部署结果可追踪。
- 上一版本Artifact仍可获得。
- Smoke Test自动执行。
