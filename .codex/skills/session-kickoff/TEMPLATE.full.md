# 新会话启动模板（完整版 / 高风险任务）

> 适用：跨模块、改动可能影响行为/接口/序列化/资源绑定、或你希望强“噪音控制”。  
> 规则以 `.codex/AGENTS.md` 为准（本模板强调“填空采集”）。

```text
PLAN: <问题 + 验收标准（可验证）>

【任务类型（必填）】
- 类型：<功能/排查/文档/数据/协议/Code Optimization>

【本次要按需加载的 skills（必填，勾选即可）】
- [ ] repo-map（入口/路径）
- [ ] build-run（构建运行）
- [ ] database-extremeworld（数据库）
- [ ] protocol-update（协议）
- [ ] data-pipeline（转表）
- [ ] unity-mcp（Unity MCP）
- [ ] debugging-logs（日志/断点）
- [ ] troubleshooting-guide（排查）
- [ ] pr-process（PR/噪音控制）
- [ ] quick-reference（命令速查）
- [ ] codex-docs-maintenance（维护 .codex 文档）

【范围与噪音控制（必填）】
- 允许修改（最小集合）：<目录/文件列表>
- 禁止修改：<目录/文件列表>
- 默认不提交/不改动（除非我明确要求）：
  - Unity 生成物：`Src/Client/Library/`、`Src/Client/Temp/`、`Src/Client/Logs/`、`Src/Client/Log/`、`Src/Client/UserSettings/`
  - 大资源：`**/*.unity`、`**/*.prefab`、`Assets/**/ThirdParty/**/*.dll`
  - 构建产物：`**/bin/`、`**/obj/`、`**/*.csproj.user`
  - IDE 痕迹：`.idea/`、`.vs/`、`.vscode/`

【是否允许执行（必填）】
- 跑命令：<否/是（仅 ACT 后）>
- 改资源：<否/是（列清单：哪些场景/Prefab/资源）>

【验收标准（必填）】
- 功能验收：
- 稳定性验收（无异常/无阻断日志/无 NRE）：
- 回归点（不该被影响的路径）：

【现状补充（按需填写）】
- 相关路径/入口文件：
- 当前报错/日志片段：
- 我已尝试与结果：
- 期望输出（要改哪些文件/生成哪些产物）：

请严格按 `.codex/AGENTS.md` 的 Plan Mode：
1) 给 ≥2 套方案（优缺点/成本/风险/推荐）
2) 输出待确认 checklist
3) 我回 `ACT: 执行方案X` 后再改代码/跑命令

（可选）如果方案涉及“行为/接口/序列化/资源绑定”变化：必须先停下重新要我确认。
```


