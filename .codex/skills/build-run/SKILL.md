# Skill: build-run（构建/运行/首次跑通）

## 何时使用
- “怎么跑起来？”、“msbuild 怎么用？”、“服务器/客户端启动顺序”

## 建议顺序（最短路径）
1. 先启动服务器（确认端口/数据库 OK）
2. 再打开 Unity 客户端播放场景进行联调

## 常用命令
```bash
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug
```

## 进一步阅读（长文档）
- `.codex/docs/onboarding/04_first-run.md`
- `.codex/docs/project-guidelines/05_build-run.md`




