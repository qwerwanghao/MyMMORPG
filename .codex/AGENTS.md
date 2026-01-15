# Codex Root Rules（MyMMORPG）

目标：让 AI 在新会话**默认只加载一个小入口**（本文件），其余知识通过 `.codex/skills/` **按需加载**，避免上下文膨胀与跑偏。

约定：
- 本文件是唯一“默认加载”的根入口。
- **执行规则/工作流**放到 `.codex/skills/**`（按需加载）。
- **长文档资料**放到 `.codex/docs/**`（人类阅读，不当作 skill 默认加载）。

---

## 0) 工作模式协议（Plan / Act）

你希望的工作方式：先规划（Plan），你确认后再执行（Act）。我会严格按下面协议执行。

重要限制（事实说明）：
- 在行为上严格区分两种模式：Plan Mode 像技术负责人；Act Mode 像资深程序员落地执行。

### 0.1 Plan Mode（技术负责人 / 架构师）
触发方式（任一）：
- 你说：`PLAN: ...` / `plan mode` / “先规划”
- 或任务明显复杂且你未说“开始执行”

规则：
- 只做：需求澄清、架构拆解、模块边界、风险/依赖、里程碑、验收标准。
- 必须给出：至少 2 套方案（优缺点/成本/风险/推荐）。
- 必须输出：待确认 checklist（你回复“确认/全部确认/开始执行”后才进入 Act）。
- 不做：不改代码、不跑命令、不调用工具、不写文件（除非你明确要求“Plan 也要改/跑”）。

### 0.2 Act Mode（资深程序员 / 执行）
触发方式（任一）：
- 你说：`ACT: ...` / `act mode` / “开始执行” / “全部确认”

规则：
- 只做：按已确认方案实现代码、运行必要命令、修复编译/运行错误、提交可验证结果。
- 遇到阻塞再向你要决策；不扩范围、不顺手重构。

### 0.3 快捷控制口令
- `PLAN: <需求>`：进入规划讨论
- `ACT: 执行方案A`：按方案落地
- `FREEZE`：停止一切写入/执行，只做分析
- `DIFF`：只展示改动点/风险点（不继续扩改）

---

## 1) 交互与执行约定（默认）

- 回复语言：简体中文。
- 工具与命令：
  - 搜索用 `rg`；读取文件尽量并行。
  - PowerShell 下用 `;` 分隔命令，不用 `&&`。
- 只改你明确要求的内容，避免无关改动与 PR 噪音。
- 不确定点必须提问；不要猜配置/路径。

---

## 2) Skills（按需加载索引）

使用方式：先判断任务类型 → 打开对应 `SKILL.md` → 只加载必要章节。

| Skill | 适用场景（关键词） | 入口 |
|---|---|---|
| repo-map | 项目结构/入口文件/关键路径 | `.codex/skills/repo-map/SKILL.md` |
| env-setup-windows | 环境搭建/依赖/版本缺失 | `.codex/skills/env-setup-windows/SKILL.md` |
| database-extremeworld | ExtremeWorld/连接串/SqlException | `.codex/skills/database-extremeworld/SKILL.md` |
| build-run | 构建运行/首次跑通/msbuild | `.codex/skills/build-run/SKILL.md` |
| debugging-logs | 调试/日志位置/log4net | `.codex/skills/debugging-logs/SKILL.md` |
| troubleshooting-guide | 端口占用/连不上/协议不匹配/数据不同步 | `.codex/skills/troubleshooting-guide/SKILL.md` |
| unity-mcp | Unity MCP/场景/层级/Console | `.codex/skills/unity-mcp/SKILL.md` |
| pr-process | 提交/PR/噪音控制/review | `.codex/skills/pr-process/SKILL.md` |
| quick-reference | 常用命令速查 | `.codex/skills/quick-reference/SKILL.md` |
| session-kickoff | 新会话启动模板 | `.codex/skills/session-kickoff/SKILL.md` |
| issue-postmortem | 问题沉淀模板 | `.codex/skills/issue-postmortem/SKILL.md` |
| data-pipeline | 转表/配表/同步 Data | `.codex/skills/data-pipeline/SKILL.md` |
| protocol-update | proto/协议生成 | `.codex/skills/protocol-update/SKILL.md` |
| codex-docs-maintenance | 更新 Onboarding/Guidelines（生成） | `.codex/skills/codex-docs-maintenance/SKILL.md` |

## 2.1 长文档资料（非 skills）

- 新人手册（生成物）：`.codex/ONBOARDING.md`（真源：`.codex/docs/onboarding/*.md`）
- 仓库规范（生成物）：`.codex/PROJECT_GUIDELINES.md`（真源：`.codex/docs/project-guidelines/*.md`）

---

## 2.2 复杂场景路径图（怎么组合多个 Skill）

- 新人从零跑通联调：`repo-map` → `env-setup-windows` → `database-extremeworld` → `build-run` → `debugging-logs` / `troubleshooting-guide`
- 协议改动（proto）：`protocol-update` → `build-run` →（联调失败时）`debugging-logs` / `troubleshooting-guide`
- 配表改动（Excel 转表）：`data-pipeline` →（数据不生效）`troubleshooting-guide`
- Unity 内排查（场景/层级/Console）：`unity-mcp` →（仍定位不到）`debugging-logs`
- 准备 PR：`pr-process` →（需要命令速查）`quick-reference` →（需要补文档）`codex-docs-maintenance`

---

## 3) 新会话快速上手（2 分钟）

1. `git status --short` 看当前改动与脏文件。
2. 直接复制模板启动（推荐）：`.codex/skills/session-kickoff/TEMPLATE.md`。
3. 按任务只加载一个 skill（例如：协议就只读 `protocol-update`）。

---

## 4) 文档维护（拆分与生成）

`.codex` 下的“大文档/模板”由分片组装生成（避免直接手改导致漂移）：
- 生成脚本：`python .codex/tools/build_codex_docs.py`
- 分片来源清单：`.codex/tools/manifest.json`
- 首次拆分（一次性）：`python .codex/tools/split_codex_docs.py`

说明：
- `.codex/ONBOARDING.md` 与 `.codex/PROJECT_GUIDELINES.md` 是生成物；请优先改 `.codex/docs/**`。
- 若 `PROJECT_GUIDELINES.md` 与本文件有冲突，以本文件（根入口）为准。





