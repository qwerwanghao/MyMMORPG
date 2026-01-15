# Skill: troubleshooting（常见问题排查）

## 何时使用
- “连不上服务器 / 端口占用 / 数据不同步 / 协议不匹配 / Unity 编译红”

## 快速入口
- 端口检查：`netstat -ano | findstr 8000`
- 协议：确认 `message.proto` 与生成的 `message.cs` 是否同步
- 配表：确认 `Src/Data/Data` 与同步目录是否更新

## 进一步阅读（长文档）
- `.codex/docs/onboarding/08_troubleshooting.md`
- `.codex/docs/project-guidelines/10_troubleshooting.md`




