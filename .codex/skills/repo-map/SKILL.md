# Skill: repo-map（仓库结构与关键路径）

## 何时使用
- 用户问“项目结构/目录在哪/入口文件在哪/Client-Server-Lib-Data-Tools 关系”
- 新会话需要快速定位核心入口

## 快速要点
- 客户端：`Src/Client`（Unity）
- 服务器：`Src/Server/GameServer/GameServer`（.NET Framework 4.6.2）
- 共享库：`Src/Lib/Common`、`Src/Lib/Protocol`
- 数据：`Src/Data`（Tables→Data），同步到 `Src/Client/Data` 与 `Src/Server/.../bin/Debug/Data`

## 进一步阅读（长文档）
- Onboarding：`.codex/docs/onboarding/01_repo-structure.md`
- Guidelines：`.codex/docs/project-guidelines/03_structure.md`




