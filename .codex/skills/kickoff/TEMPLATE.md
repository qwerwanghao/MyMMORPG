# 新会话启动工作流模板（Plan → Act）

> 用途：当你打开一个新的会话时，用最短输入把上下文“喂给”AI，并强制先规划（Plan），你确认后再执行（Act）。
> 适用：需求不确定 / 任务复杂 / 涉及多模块（Client/Server/Protocol/Data/Unity）。

---

## 元信息
- 标题：
- 日期：
- 目标（可验收）：
- 约束（不能改什么/必须兼容什么/时间预算）：
- 涉及模块/目录：
- 相关文件（路径/入口）：
- 相关日志/截图：

---

## 1) 新会话第一条消息（直接复制粘贴）

```text
PLAN: <用 1-3 句话描述问题 + 验收标准（可验证）>

【硬性流程门禁】
请严格按 `.codex/AGENTS.md` 的 Plan Mode 执行：
- 先 Plan：≥2 套方案 + 待确认 checklist
- 未收到 `ACT: 执行方案X`：禁止改代码
- 若发现需要改变行为/接口/序列化/资源绑定：必须停下重新要确认

【任务分类（必填）】
- 类型：<功能/排查/文档/数据/协议/Code Optimization>
- 是否允许跑命令：<否（默认）/是（ACT 后）>
- 是否允许改资源：<否（默认）/是（列清单）>

【范围与噪音控制（必填）】
- 允许修改的目录（最小集合）：
- 禁止修改的目录：
- 默认禁止引入 PR 噪音（除非我明确要求）：
  - Unity 资源/生成物：`**/*.unity`、`**/*.prefab`、`Assets/**/ThirdParty/**/*.dll`、`Src/Client/Library/`、`Src/Client/Temp/`、`Src/Client/Logs/`、`Src/Client/Log/`、`Src/Client/UserSettings/`、`Src/Client/Packages/packages-lock.json`
  - IDE/工具痕迹：`.idea/`、`.vs/`、`.vscode/`、`.claude/`、`.specify/`
  - 构建产物：`**/bin/`、`**/obj/`、`**/*.csproj.user`

【Code Optimization 特别约束（仅当类型=Code Optimization）】
- 仅顺序/分组/排版；不引入/删除逻辑；不改 API 语义
- 必须最小 diff：不得“删文件再重建”；保留原换行风格与编码
- 默认不改名；不动 Unity 序列化/绑定字段；不改资源引用
- Preserve existing behavior and configuration
- Prefer explicit if/else over nested ternaries
- Avoid one-liners that reduce readability
- Keep functions small and focused
- Do not refactor architecture-level code

【现状补充（按需填写）】
- 相关路径/入口文件：
- 我已尝试与结果：
- 当前报错/日志片段：
- 期望输出（要改哪些文件/生成哪些产物）：
```

---

## 2) AI 在 Plan Mode 必须交付的内容（验收清单）

- [ ] 复述需求与验收标准（避免理解偏差）
- [ ] 列出关键未知点（需要我补充的信息）
- [ ] 提供至少 2 套方案（含：优缺点/成本/风险/推荐）
- [ ] 给出模块拆解与推进步骤（里程碑）
- [ ] 明确哪些步骤会改哪些文件（大致范围）
- [ ] 输出 “待我确认” checklist（我确认后进入 Act）

---

## 3) Act Mode 启动口令（我确认方案后发）

```text
ACT: 执行方案 <A/B/...>
按你在 Plan Mode 给出的步骤推进，遇到阻塞再向我提问。
```

---

## 4) 新会话快速上手信息采集（可选：让 AI 跑命令前你先给它）

> 如果你希望 AI 少跑命令、直接进入分析，你可以把下面信息手动贴给它：

- `git status --short` 输出：
- Unity 相关（如有）：
  - 当前场景名/Prefab Stage：
  - Console 最新错误（前 20 行）：
  - 关键对象层级路径（如 ButtonPlay 的路径）：
- Server 相关（如有）：
  - 最新 server log 错误片段：
  - 端口/配置文件（Settings/Config）：
- Protocol 相关（如有）：
  - 本次改动涉及的 message 名称：
  - proto diff（或相关字段列表）：

---

## 5) 常用对话控制（防跑偏）

- `FREEZE`：停止一切写入/执行，只做分析
- `DIFF`：只展示改动点/风险点（不继续扩改）
- `PLAN:`：只规划不动手
- `ACT:`：按确认方案动手


