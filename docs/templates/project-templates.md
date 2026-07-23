# 项目模板

## 1. User Story

```markdown
# [ID] 标题

作为：
我希望：
从而：

## Business Rules

- RULE-001：

## Acceptance Criteria

### Scenario：正常

Given
When
Then

### Scenario：失败

Given
When
Then

## Security

- Permission：
- Data Scope：

## Data/API/UI影响

## Audit/Observability

## Test Evidence
```

## 2. ADR

```markdown
# ADR-NNN：决定标题

状态：Proposed / Accepted / Superseded
日期：
Owner：

## Context

为什么必须作出决定？

## Decision

决定是什么？

## Alternatives

考虑过什么其他方案？

## Consequences

好处、成本、风险和后续工作是什么？
```

## 3. Pull Request

```markdown
## Business outcome

## What changed

## Acceptance criteria

## Security and tenant isolation

## Database/Migration impact

## API/UI impact

## Test evidence

## Deployment and rollback

## Screenshots
```

## 4. Release Notes

```markdown
# Release vX.Y.Z

Commit SHA：
Artifact：
Release date：

## New

## Fixed

## Database changes

## Configuration changes

## Known issues

## Deployment

## Rollback/Roll-forward
```

## 5. Incident

```markdown
# Incident INC-NNN

Severity：
Start：
Resolved：
Incident Commander：

## User impact

## Timeline

## Root causes

## Detection

## Recovery

## What went well

## What failed

## Actions

| Action | Owner | Due | Status |
|---|---|---|---|
```

## 6. 每个功能开始前

- [ ] Story有业务价值。
- [ ] 验收条件完整。
- [ ] 业务规则编号存在。
- [ ] 权限和Organisation明确。
- [ ] 数据和Migration影响明确。
- [ ] 错误码和UI提示明确。
- [ ] 测试策略明确。

## 7. 每个功能合并前

- [ ] 本地Build和Test通过。
- [ ] 查看完整Git Diff。
- [ ] 没有Secret。
- [ ] Migration已评审。
- [ ] 权限和跨租户测试通过。
- [ ] 文档更新。
- [ ] CI全部通过。

## 8. 每次生产发布前

- [ ] Staging验证同一Artifact。
- [ ] UAT完成。
- [ ] 数据库备份和Migration演练完成。
- [ ] Release Notes完成。
- [ ] 监控和告警正常。
- [ ] Rollback/Roll-forward明确。
- [ ] 业务和技术Owner批准。
