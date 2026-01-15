---
name: codex-docs-maintenance
description: "维护/更新 Codex Onboarding & Project Guidelines（生成与拆分）"
metadata:
  short-description: "Codex 文档维护"
---

# Skill: codex-docs-maintenance（维护/更新 Onboarding & Guidelines）

## 前置条件
- 本机可运行 Python（用于生成脚本）
- 你接受：`.codex/ONBOARDING.md` / `.codex/PROJECT_GUIDELINES.md` 是生成物，不直接手改

## 何时使用
- 需要更新 `.codex/ONBOARDING.md` 或 `.codex/PROJECT_GUIDELINES.md`
- 需要新增/拆分 `.codex/docs/**` 分片
- 需要调整 skills 索引或生成规则

## 原则
- `.codex/ONBOARDING.md` / `.codex/PROJECT_GUIDELINES.md`：生成物（入口）
- `.codex/docs/**`：长文档真源分片（维护点）
- `.codex/skills/**`：按需加载的执行规则/速查（维护点）

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



