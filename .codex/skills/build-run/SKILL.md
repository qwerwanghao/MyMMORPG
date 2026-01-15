---
name: build-run
description: "构建/运行/首次跑通（Windows/.NET/Unity）"
metadata:
  short-description: "构建运行与首次跑通"
---

# Skill: build-run（构建/运行/首次跑通）

## 前置条件
- 你在 Windows 上开发
- 已安装 Visual Studio / Build Tools，并具备 `msbuild`
- 服务器目标框架：`.NET Framework 4.6.2`（需要 Targeting Pack）

## 何时使用
- “怎么跑起来？”、“msbuild 怎么用？”、“服务器/客户端启动顺序”

## 建议顺序（最短路径）
1. 先启动服务器（确认端口/数据库 OK）
2. 再打开 Unity 客户端播放场景进行联调

## 常用命令
```bash
msbuild Src\\Server\\GameServer\\GameServer.sln /p:Configuration=Debug
```

## 常见错误与处理
- `msbuild` 不是内部命令：安装 VS Build Tools，或在 “Developer PowerShell for VS” 里运行
- `MSB3644` / 缺 `.NETFramework,Version=v4.6.2`：安装 `.NET Framework 4.6.2 Targeting Pack`
- 服务器运行时端口占用：用 `netstat -ano | findstr 8000` 定位占用进程
- 服务器报 `SqlException`：优先看数据库与连接串（用 `.codex/skills/database-extremeworld/SKILL.md`）

## 相关技能（组合）
- 项目入口定位：`.codex/skills/repo-map/SKILL.md`
- 数据库：`.codex/skills/database-extremeworld/SKILL.md`
- 常见排查：`.codex/skills/troubleshooting-guide/SKILL.md`

## 进一步阅读（长文档）
- `.codex/docs/onboarding/04_first-run.md`
- `.codex/docs/project-guidelines/05_build-run.md`



