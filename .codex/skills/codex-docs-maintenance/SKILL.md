# Skill: codex-docs-maintenance（维护/更新 Onboarding & Guidelines）

## 何时使用
- 需要更新 `.codex/ONBOARDING.md` 或 `.codex/PROJECT_GUIDELINES.md`
- 需要新增/拆分 `.codex/docs/**` 分片
- 需要调整 skills 索引或生成规则

## 原则
- `.codex/ONBOARDING.md` / `.codex/PROJECT_GUIDELINES.md` 是生成物：不要直接手改（会漂移/丢失）。
- 只改“真源”：
  - 长文档分片：`.codex/docs/**`
  - 生成清单：`.codex/tools/manifest.json`
  - 生成脚本：`.codex/tools/build_codex_docs.py`

## 标准流程
1. 修改 `.codex/docs/onboarding/*.md` 或 `.codex/docs/project-guidelines/*.md`
2. 生成回 `.codex/*.md`
   ```bash
   python .codex/tools/build_codex_docs.py
   ```
3. 自检：确认生成物可读、目录/链接正确、无乱码（UTF-8 BOM）

## 一次性拆分（仅首次/重建时用）
```bash
python .codex/tools/split_codex_docs.py
```




