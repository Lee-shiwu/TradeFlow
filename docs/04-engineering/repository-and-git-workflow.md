# 仓库与Git工作流

## 1. 目标目录

```text
TradeFlow/
├── backend/
│   ├── src/
│   └── tests/
├── frontend/
├── deploy/
├── docs/
├── .github/
│   ├── workflows/
│   └── pull_request_template.md
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── TradeFlow.sln
└── README.md
```

## 2. 分支策略

使用短生命周期GitHub Flow：

```text
main
├── feature/PUR-101-submit-requisition
├── fix/INV-204-concurrent-allocation
├── docs/update-deployment-runbook
└── chore/update-dotnet-patches
```

- `main`始终可发布。
- 不使用长期`develop`分支。
- 每个分支只解决一个明确任务。
- 每天同步`main`，避免大型冲突。
- 通过PR和CI合并。
- 默认Squash Merge。

## 3. Commit规范

格式：

```text
type(scope): imperative summary
```

常用类型：

```text
feat      新业务能力
fix       缺陷修复
test      测试
docs      文档
refactor  不改变行为的重构
perf      性能优化
build     构建和依赖
ci        GitHub Actions
chore     其他维护
```

示例：

```text
feat(purchasing): submit requisition for approval
fix(inventory): prevent duplicate stock reservation
test(sales): cover concurrent order confirmation
docs(api): document conflict error response
```

一个Commit应当：

- 有单一目的。
- 能够编译或只包含明确文档变化。
- 不混入无关格式化。
- 不包含Secret或生成文件。

## 4. Pull Request流程

1. 从最新`main`创建分支。
2. 在Issue中确认验收条件。
3. 小步Commit。
4. 本地运行格式、编译和测试。
5. 阅读完整Diff。
6. Push并创建PR。
7. 填写业务变化、测试证据、数据库和部署影响。
8. 解决评审意见。
9. CI全绿并获得批准。
10. Squash Merge并删除分支。

## 5. Branch Protection

`main`至少配置：

- Require pull request。
- Require status checks。
- Require conversation resolution。
- Block force push。
- Block branch deletion。
- 可选Require signed commits。

## 6. 版本和Release

使用Semantic Versioning：

```text
v1.2.3
│ │ └─ Patch：向后兼容的修复
│ └─── Minor：向后兼容的新功能
└───── Major：不兼容变化
```

每个生产Release包含：

- 业务变化。
- 修复。
- Migration。
- 配置变化。
- 已知问题。
- 部署和回滚说明。
- 对应Commit SHA和Artifact。

## 7. Secrets规则

如果Secret曾经进入Git：

1. 立即撤销并轮换Secret。
2. 不要只删除当前文件。
3. 通知安全或仓库Owner。
4. 根据组织流程清理历史。
5. 增加扫描和预防规则。
