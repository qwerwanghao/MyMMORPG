# MyMMORPG（教学/练手 MMO 项目）

一个双端的 MMORPG 示例工程：Unity 客户端 + .NET 服务器，采用 Protobuf 协议、TCP 长连接、EF 数据库访问与 log4net 日志。适合用于网络游戏架构学习与二次扩展。

---

## 项目组成

- **客户端**：`Src/Client`（Unity 6000.0.53f1）  
  负责 UI、角色选择/创建、场景表现、网络收发与数据加载。
- **服务器**：`Src/Server/GameServer/GameServer`（.NET Framework 4.6.2）  
  负责 TCP 监听、消息分发、账号/角色网关、数据库读写与主循环 Tick。
- **共享库**：`Src/Lib/Common`、`Src/Lib/Protocol`  
  双端共用的网络、日志与协议代码。
- **策划数据**：`Src/Data`  
  Excel 源表与转表产物；转表脚本默认使用 Python。
- **工具链**：`Tools/`  
  协议生成等辅助脚本。

更细的 Src 子模块说明与关键时序图见：`Src/README.md`。

---

## 主要特性（当前实现）

- 账号注册 / 登录
- 角色创建 / 删除 / 选择
- TCP 消息收发 + Protobuf 序列化
- 数据驱动的地图/角色/传送/出生点配置
- 统一日志系统（log4net + Unity Console 转发）

---

## 开发环境

| 组件 | 版本/要求 |
|------|-----------|
| Unity | 6000.0.53f1+ |
| .NET Framework | 4.6.2 |
| Visual Studio | 2019+（安装“.NET 桌面开发”） |
| SQL Server | LocalDB 或完整实例 |
| Python | 3.10+（用于转表） |

---

## 快速开始（联调）

1) **初始化数据库（首次必做）**  
   - 在 SQL Server 中创建数据库 `ExtremeWorld`。  
   - 运行 `Src/Server/GameServer/GameServer/Entities.edmx.sql` 初始化表结构。  
   - 修改 `Src/Server/GameServer/GameServer/App.config` 的连接字符串指向本地实例。

2) **生成策划数据（可选，但建议先跑一次）**
```bash
cd Src\Data
python excel2json.py
```
产物在 `Src/Data/Data`，并会同步到客户端/服务器的 Data 目录。

3) **构建并启动服务器**
```bash
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug
cd Src\Server\GameServer\GameServer\bin\Debug
GameServer.exe
```
默认监听 `127.0.0.1:8000`。

4) **打开 Unity 客户端并运行**
1. Unity 打开项目 `Src/Client`。  
2. 进入测试场景（如 `Assets/Levels/Test.unity`）并 Play。  
3. 在登录/选角 UI 完成注册 → 登录 → 创角/删角链路。

---

## 文档与规范

项目的开发/贡献/排查细节集中在 `.codex`：

- `.codex/ONBOARDING.md`：新人从零跑通环境与核心系统导读。  
- `.codex/PROJECT_GUIDELINES.md`：项目与团队规范（提交、风格、链路等）。  
- `.codex/AGENTS.md`：Codex/AI 在本仓库的工作规则。  
- `.codex/WORKFLOWS.template.md`：问题沉淀模板。

---

## 贡献方式

- 修改协议后需同步提交生成的 `message.cs`（见 `.codex/PROJECT_GUIDELINES.md`）。  
- PR 请说明影响范围、受影响场景与验证步骤。  
- 提交信息动词开头、范围清晰。

