---
name: troubleshooting-guide
description: "常见问题排查：端口占用、连不上、版本/协议不匹配、数据不同步"
metadata:
  short-description: "常见问题排查"
---

# Skill: troubleshooting-guide（常见问题排查）

## 前置条件
- 你能提供：错误日志片段（客户端/服务器）、复现步骤、涉及的模块（Client/Server/Protocol/Data/Unity）

## 何时使用
- “连不上服务器 / 端口占用 / 数据不同步 / 协议不匹配 / Unity 编译红”

## 快速入口
- 端口检查：`netstat -ano | findstr 8000`
- 协议：确认 `Src/Lib/proto/message.proto` 与生成的 `Src/Lib/Protocol/message.cs` 是否同步
- 配表：确认 `Src/Data/Data` 与同步目录（Client/Server Data）是否更新

## 常见问题 → 快速处理
- 端口占用：查占用进程 → 结束或改端口
- `SqlException`：优先检查连接串与 `ExtremeWorld` 是否已初始化
- 消息解析失败：优先检查是否跑过 `Tools/genproto.cmd` 并双端重新编译
- 配表不生效：确认转表产物与同步目录时间一致；必要时重启客户端/服务器

## 相关技能（组合）
- 先定位入口：`.codex/skills/repo-map/SKILL.md`
- 协议相关：`.codex/skills/protocol-update/SKILL.md`
- 配表相关：`.codex/skills/data-pipeline/SKILL.md`
- 构建/运行：`.codex/skills/build-run/SKILL.md`
- 日志/断点：`.codex/skills/debugging-logs/SKILL.md`

## 进一步阅读（长文档）
- `.codex/docs/onboarding/08_troubleshooting.md`
- `.codex/docs/project-guidelines/10_troubleshooting.md`



