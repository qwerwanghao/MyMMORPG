---
name: pr-process
description: "提交/PR/噪音控制：最小改动、可审查、可回滚"
metadata:
  short-description: "提交与 PR 流程"
---

# Skill: pr-process（提交/PR/噪音控制）

## 前置条件
- 你能提供：本次改动目标、影响范围、手动验证步骤（或可运行的测试命令）

## 何时使用
- 用户说“准备提 PR / review / 噪音控制 / 提交流程 / 需要更新哪些文档”

## 快速清单（提交前）
- PR 只包含与需求相关的最小改动
- 写清楚：设计意图、关键变更点、验证方式、回滚方式（如必要）
- 协议改动：必须提交 `message.proto` + 生成的 `message.cs`

## 进一步阅读（长文档）
- `.codex/docs/project-guidelines/08_pr-process.md`



