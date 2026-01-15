# Skills（按需加载）

> 目标：把 `.codex` 的“大而全”文档拆成可按需加载的模块；默认只读 `.codex/AGENTS.md`，其余内容按任务加载对应 skill。

## 使用方式（给 AI）
1. 先读 `.codex/AGENTS.md`（根入口）。
2. 根据任务类型/关键词，打开对应 `SKILL.md`。
3. `SKILL.md` 会指向相关命令/入口文件，并在需要时跳转到 `.codex/docs/**` 的长文档资料或模板分片。

## Skills 列表
- `repo-map`：仓库结构与关键路径（`./repo-map/SKILL.md`）
- `env-setup-windows`：Windows 环境准备（`./env-setup-windows/SKILL.md`）
- `database-extremeworld`：数据库初始化/连接串（`./database-extremeworld/SKILL.md`）
- `build-run`：构建/运行/首次跑通（`./build-run/SKILL.md`）
- `debugging-logs`：调试与日志（`./debugging-logs/SKILL.md`）
- `troubleshooting-guide`：常见问题排查（`./troubleshooting-guide/SKILL.md`）
- `unity-mcp`：Unity MCP 使用（`./unity-mcp/SKILL.md`）
- `pr-process`：提交/PR/噪音控制（`./pr-process/SKILL.md`）
- `quick-reference`：常用命令速查（`./quick-reference/SKILL.md`）
- `session-kickoff`：新会话启动模板（`./session-kickoff/SKILL.md`）
- `issue-postmortem`：问题沉淀模板（`./issue-postmortem/SKILL.md`）
- `data-pipeline`：配表转表与同步（`./data-pipeline/SKILL.md`）
- `protocol-update`：proto 变更与生成（`./protocol-update/SKILL.md`）
- `codex-docs-maintenance`：维护/更新 Onboarding & Guidelines（`./codex-docs-maintenance/SKILL.md`）





