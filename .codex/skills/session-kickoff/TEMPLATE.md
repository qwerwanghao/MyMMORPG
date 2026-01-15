# 新会话启动模板（快速版）

> 用途：把关键信息一次性给全，强制 **先 Plan 后 Act**，并明确本次要按需加载哪些 skills。  
> 规则以 `.codex/AGENTS.md` 为准（本模板不重复写规则，只负责采集信息）。

```text
PLAN: <用 1-3 句话描述：你要解决什么问题 + 验收标准（可验证）>

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

【范围（必填，最少写 1 行）】
- 允许修改：<目录/文件列表，越小越好>
- 禁止修改：<目录/文件列表；不填表示无额外限制>

【是否允许执行（必填）】
- 跑命令：<否/是（仅 ACT 后）>
- 改资源（Unity 场景/Prefab 等）：<否/是（列清单）>

【你已经试过什么（选填，但强烈建议）】
- 我已尝试：
- 结果：

【关键上下文（选填，但能显著提速）】
- git：`git status --short` 输出：
- Unity（如有）：当前场景/Console 前 20 行/目标对象层级路径：
- Server（如有）：server log 错误片段/端口/配置：
- Protocol（如有）：涉及的 message 名称/proto diff：
- Data（如有）：涉及的表/生成物路径：

请严格按 `.codex/AGENTS.md` 的 Plan Mode：
1) 给 ≥2 套方案（优缺点/成本/风险/推荐）
2) 输出待确认 checklist
3) 我回 `ACT: 执行方案X` 后再改代码/跑命令
```


