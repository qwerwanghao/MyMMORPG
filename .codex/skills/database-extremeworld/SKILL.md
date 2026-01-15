# Skill: database-extremeworld（数据库初始化/连接串）

## 前置条件
- 本机已安装 SQL Server（Express/Developer/LocalDB 任一）
- 你可以修改服务器连接串：`Src/Server/GameServer/GameServer/App.config`

## 何时使用
- 服务器启动报 `SqlException` / “无法打开数据库” / 连接字符串问题
- 新人首次搭建 `ExtremeWorld` 数据库

## 快速要点
- 数据库名：`ExtremeWorld`
- 初始化脚本：`Src/Server/GameServer/GameServer/Entities.edmx.sql`
- 连接串：`Src/Server/GameServer/GameServer/App.config`（`ExtremeWorldEntities`）

## 常见错误与处理
- `SqlException: 无法打开数据库 "ExtremeWorld"`：确认已创建 DB，并已运行 `Entities.edmx.sql`
- `SqlException: 登录失败`：检查 `data source` 实例名/身份验证方式（integrated security / 用户名密码）
- `provider connection string` 写错：优先从可工作的连接串开始替换 `data source`
- 迁移到另一台机器后报错：基本都是连接串实例名不一致导致

## 相关技能（组合）
- 构建/运行：`.codex/skills/build-run/SKILL.md`
- 常见排查：`.codex/skills/troubleshooting-guide/SKILL.md`

## 进一步阅读（长文档）
- `.codex/docs/onboarding/03_database-setup.md`
- `.codex/docs/project-guidelines/02_dependencies.md`
- `.codex/docs/project-guidelines/10_troubleshooting.md`



