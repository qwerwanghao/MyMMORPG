---
name: debugging-logs
description: "调试与日志：日志位置、级别、常用排错路径"
metadata:
  short-description: "调试与日志"
---

# Skill: debugging-logs（调试与日志）

## 前置条件
- 你能提供客户端/服务器日志片段（或明确“哪一步触发的”）

## 何时使用
- 要抓客户端/服务器日志、定位调用链、确认 log4net 输出位置

## 快速要点
- 服务器日志：`Src/Server/GameServer/GameServer/Log/server-detailed.log*`
- 客户端日志：`Src/Client/Log/client.log`

## 常用定位策略
- 先用日志定位到“最后一次成功/第一次失败”的边界
- 优先关注：网络收发、协议解析、数据库访问、场景加载/资源缺失

## 相关技能（组合）
- 常见排查：`.codex/skills/troubleshooting-guide/SKILL.md`
- 数据库：`.codex/skills/database-extremeworld/SKILL.md`
- 协议：`.codex/skills/protocol-update/SKILL.md`

## 进一步阅读（长文档）
- `.codex/docs/onboarding/07_debugging-and-logs.md`
- `.codex/docs/project-guidelines/04_common-actions.md`



