# Docs（长文档资料）

定位：
- 这里放“人类阅读的长文档/叙事说明”（Onboarding、Guidelines 等）。
- 这些内容不是 skill（不应该默认加载、也不应该被当成执行规则），但可以被 skills 引用。

目录：
- Onboarding 分片索引：`.codex/docs/onboarding/README.md`
- Project Guidelines 分片索引：`.codex/docs/project-guidelines/README.md`

生成物：
- `.codex/ONBOARDING.md`、`.codex/PROJECT_GUIDELINES.md` 等由 `.codex/tools/manifest.json` 指定的 sources 组装生成。

维护：
- 修改 `docs/**` 分片 → `python .codex/tools/build_codex_docs.py` 生成回 `.codex/*.md`




