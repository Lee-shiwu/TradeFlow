# TradeFlow 前端（React + JavaScript + Vite）

前端统一使用 JavaScript。已有系统状态页、商品列表和商品详情页的行为保持不变；
后端仍使用 C#，数据库与 HTTP 接口契约不变。

## 文件约定

- API、Hook、工具函数和无 JSX 的测试使用 `.js`。
- React 页面、Provider 和包含 JSX 的测试使用 `.jsx`。
- Vite/Vitest 配置使用 `vite.config.js`，测试初始化使用 `src/test/setup.js`。
- 原有接口类型保存在 JSDoc 注释中，例如 `productDetailsTypes.js` 中的 `@typedef`。
- 后续开发不再添加 `.ts`、`.tsx`、`interface`、`as Type` 或 TypeScript 泛型语法。
- JSDoc 提供字段说明和编辑器补全，不执行运行时验证。保持现有输入验证和 API 错误处理。
- `jsconfig.json` 为 VS Code 提供项目配置；项目不再依赖 `tsc` 构建或类型检查。

## 安装与启动

在本目录运行：

```powershell
pnpm install --frozen-lockfile
pnpm dev
```

访问 http://localhost:6173 。先启动本地 SQL Server 和后端 API（HTTP 6280），
再启动前端；`pnpm dev` 不会启动 API 或数据库。
Vite 将 `/api` 和 `/health` 代理到 `VITE_DEV_API_TARGET`，默认 http://localhost:6280 。
保留本地 `.env.local` 中的临时组织、用户及代理配置，不将这些本地配置提交。

## 验证

```powershell
pnpm lint
pnpm test:run
pnpm build
```

不再使用 `pnpm typecheck`。CI 同样执行 Lint、Vitest 和 Vite 构建。

单独运行商品详情页面测试：

```powershell
pnpm test -- src/features/products/pages/ProductDetailsPage.test.jsx --run
```

## 当前页面

- `/`：API 和数据库连通状态。
- `/products`：商品搜索、状态筛选、分页及 SKU 详情入口。
- `/products/:productId`：商品详情、审计信息、错误提示、重试和返回列表。

查询仍由 TanStack Query 管理；公共 API 客户端仍添加临时身份请求头并转换 API 错误。
查询缓存、状态筛选、分页、Base64 RowVersion 和后端组织隔离规则不因语言迁移改变。
