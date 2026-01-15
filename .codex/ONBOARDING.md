<!--
This file is generated.
Source of truth: inputs listed in .codex/tools/manifest.json
Regenerate: python .codex/tools/build_codex_docs.py
Output: .codex/ONBOARDING.md
-->

# 新人 Onboarding（MyMMORPG）

> 目标：让新人能**独立跑通客户端 + 服务器联调**、理解核心系统结构，并掌握日常开发工作流。  
> 本文偏“从零到可开发”的细节手册；团队规范与更完整背景仍以 `.codex/PROJECT_GUIDELINES.md` 为准。

---

## 目录
- [1) 仓库结构与关键路径](#1-仓库结构与关键路径)
- [2) 环境准备（Windows）](#2-环境准备windows)
- [3) 数据库初始化（首次必做）](#3-数据库初始化首次必做)
- [4) 首次运行（推荐顺序）](#4-首次运行推荐顺序)
- [5) 常用开发工作流](#5-常用开发工作流)
- [6) 核心系统源码导读](#6-核心系统源码导读)
- [7) 调试与日志](#7-调试与日志)
- [8) 常见问题与排查清单](#8-常见问题与排查清单)
- [9) 新人练习任务（建议一周内完成）](#9-新人练习任务建议一周内完成)
- [10) 进一步阅读](#10-进一步阅读)

---

## 1) 仓库结构与关键路径

**顶层结构**
- `Src/Client`：Unity 客户端项目（Unity 6000.0.53f1）。主要代码在 `Assets/Game/Scripts`。
- `Src/Server/GameServer`：服务器解决方案目录（.NET Framework 4.6.2）。核心项目在 `Src/Server/GameServer/GameServer`。
- `Src/Lib`：双端共享库：
  - `Src/Lib/Common`：日志/网络/单例等公共基础。
  - `Src/Lib/Protocol`：Protobuf 生成的协议代码。
- `Src/Data`：策划 Excel 与转表产物：
  - `Src/Data/Tables/*.xlsx`：源表。
  - `Src/Data/Data/*.txt`：转表产物（JSON 文本）。
  - `Src/Data/excel2json.py`：默认转表脚本。
- `Tools/`：工具链（协议生成等）：
  - `Tools/genproto.cmd`：根据 proto 生成 C# 协议。

**必记关键文件**
- 客户端入口：`Src/Client/Assets/Game/Scripts/Core/LoadingManager.cs`
- 客户端数据加载：`Src/Client/Assets/Game/Scripts/Core/DataManager.cs`
- 客户端网络：`Src/Client/Assets/Game/Scripts/Network/NetClient.cs`
- 客户端用户服务：`Src/Client/Assets/Game/Scripts/Services/UserService.cs`
- 服务器入口：`Src/Server/GameServer/GameServer/Program.cs`
- 服务器主循环：`Src/Server/GameServer/GameServer/GameServer.cs`
- 服务器网络层：`Src/Server/GameServer/GameServer/Network/NetService.cs`
- 服务器用户服务：`Src/Server/GameServer/GameServer/Services/UserSerevice.cs`
- 协议定义：`Src/Lib/proto/message.proto`
- 协议生成代码：`Src/Lib/Protocol/message.cs`

---

## 2) 环境准备（Windows）

### 2.1 必需软件
1. **Unity Hub + Unity Editor 6000.0.53f1+**
   - 通过 Unity Hub 安装对应版本。
   - 首次打开 `Src/Client` 可能需要较长导入时间。
2. **Visual Studio 2019+**
   - 勾选工作负载：“.NET 桌面开发”。
   - 确保安装 **.NET Framework 4.6.2 Targeting Pack**。
3. **SQL Server**
   - 推荐：SQL Server Express / Developer + SSMS。
   - 也可用 LocalDB（见 3.2）。
4. **Python 3.10+**
   - 用于默认转表 `Src/Data/excel2json.py`。
5. （可选）`protoc` 已内置，无需额外安装。

### 2.2 仓库准备
```bash
cd F:\Git
git clone <https://github.com/qwerwanghao/MyMMORPG> MyMMORPG
cd MyMMORPG
git status --short
```
若 `git status` 有大量脏文件，先问负责人是否需要清理或切到干净分支。

---

## 3) 数据库初始化（首次必做）

服务器依赖数据库 `ExtremeWorld`，并通过 EF（Entities.edmx）访问。

### 3.1 使用本机 SQL Server 实例（推荐）
1. 打开 SSMS，确认实例名（示例：`HOMEPC\MMORPG` / `.\SQLEXPRESS`）。
2. 新建数据库：`ExtremeWorld`。
3. 执行初始化脚本：
   - 文件：`Src/Server/GameServer/GameServer/Entities.edmx.sql`
   - 右键“新建查询”→ 选择数据库 ExtremeWorld → 运行脚本。
4. 修改连接串：
   - 文件：`Src/Server/GameServer/GameServer/App.config`
   - 重点替换 `data source=...` 为你的实例名，例如：
     ```xml
     <add name="ExtremeWorldEntities"
          connectionString="metadata=...;provider=System.Data.SqlClient;provider connection string=&quot;data source=.\SQLEXPRESS;initial catalog=ExtremeWorld;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" />
     ```

### 3.2 使用 LocalDB（轻量可选）
1. 确保系统已安装 LocalDB（VS 安装时通常带 `(localdb)\MSSQLLocalDB`）。
2. 创建数据库（PowerShell）：
   ```powershell
   sqllocaldb create MMORPG
   sqllocaldb start MMORPG
   ```
3. 在 SSMS 连接 `(localdb)\MMORPG`，新建 `ExtremeWorld` 并运行 `Entities.edmx.sql`。
4. 修改连接串 `data source=(localdb)\MMORPG`。

### 3.3 验证数据库可用
在 SSMS 中确认存在表（Users/Players/Characters 等），且无脚本报错。

---

## 4) 首次运行（推荐顺序）

**顺序很重要：先服务器、后客户端。**

### 4.1 构建并运行服务器

**方式 A：命令行构建**
```bash
cd F:\Git\MyMMORPG
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug
cd Src\Server\GameServer\GameServer\bin\Debug
GameServer.exe
```

**方式 B：Visual Studio**
1. 打开 `Src/Server/GameServer/GameServer.sln`。
2. 右键解决方案“还原 NuGet 包”（若提示缺失）。
3. 设 `GameServer` 为启动项目。
4. `F5` 启动。

**运行后检查**
- 控制台应输出 “Game Server Running......”。
- 服务器日志位置：`Src/Server/GameServer/GameServer/Log/server-detailed.log*`。
- 如果启动失败：
  - 先看控制台异常；再看 `server-detailed.log`。
  - 常见原因见第 8 节。

### 4.2 打开并运行 Unity 客户端
1. Unity Hub → Open Project → 选择 `Src/Client`。
2. 首次导入完成后，打开测试场景：
   - `Assets/Levels/Test.unity`（或负责人指定的当前测试场景）。
3. 点击 Play。

**运行后检查**
- 客户端日志文件：`Src/Client/Log/client.log`。
- Unity Console 无红色编译错误。
- 在登录 UI 尝试：
  1) 注册新账号  
  2) 登录  
  3) 创角 / 删角  
  若全流程走通，说明联调 OK。

---

## 5) 常用开发工作流

### 5.1 协议更新（客户端/服务器都要同步）
1. 修改 `Src/Lib/proto/message.proto`。
2. 生成协议：
   ```bash
   cd Tools
   genproto.cmd
   ```
3. 检查 `Src/Lib/Protocol/message.cs` 变化。
4. 双端重新编译并提交 proto + message.cs。

### 5.2 表格数据更新（默认 Python）
1. 修改 `Src/Data/Tables/*.xlsx`。
2. 转表并同步：
   ```bash
   cd Src\Data
   python excel2json.py
   ```
3. 产物在 `Src/Data/Data/*.txt`，脚本会自动同步到：
   - 客户端：`Src/Client/Data`
   - 服务器：`Src/Server/GameServer/GameServer/bin/Debug/Data`

### 5.3 共享库 DLL 更新（客户端依赖）
当你改了 `Src/Lib/Common` 或 `Src/Lib/Protocol`：
```bash
msbuild Src/Lib/Common/Common.csproj /p:Configuration=Debug
msbuild Src/Lib/Protocol/Protocol.csproj /p:Configuration=Debug
```
Post-build 会复制 DLL/PDB 到 `Src/Client/Assets/ThirdParty/*`。若 Unity 仍报旧版本问题，可手动覆盖。

---

## 6) 核心系统源码导读

### 6.1 客户端（Unity）

**启动链路**
1. `LoadingManager.Start()`（`Assets/Game/Scripts/Core/LoadingManager.cs`）
   - 初始化 log4net（读取 `Assets/Resources/log4net.xml`）。
   - `Log.Init("Unity")` + `UnityLogger.Init()`：统一日志出口。
   - 触发 `DataManager.Instance.LoadData()` 加载策划数据。
   - 初始化 `UserService`（网络/登录相关）。
   - UI 从 Tips/Loading 切到 Login。

**数据系统**
- `DataManager`（`Core/DataManager.cs`）
  - `DataPath="Data/"`，从 `Src/Client/Data/*.txt` 读取 JSON 文本。
  - 解析并缓存 `Maps/Characters/Teleporters/SpawnPoints`。
  - Editor 下提供 Save 方法写回 txt（仅策划/工具用）。

**网络系统**
- `NetClient`（`Network/NetClient.cs`）
  - 维护 TCP Socket、发送队列与接收缓冲。
  - `PackageHandler`：4 字节长度前缀 + Protobuf `NetMessage` 打包/解包。
  - `MessageDistributer.Instance`：将解出的消息分发给订阅者（Services/UI）。
  - `Update()` 每帧调用：保持连接→收包→发包→分发。

**业务服务**
- `Services/UserService.cs`
  - 对外提供 `SendRegister/SendLogin/SendCreateCharacter/SendDeleteCharacter`。
  - 维护 `IsBusy` 防止重复提交。
  - 通过 `MessageDistributer` 订阅对应 Response，然后回调 UI。

**UI 层**
- 登录/注册：`UI/UILogin.cs`、`UI/UIRegister.cs`
- 选角：`UI/UICharacterSelect.cs`、`UI/UICharInfo.cs`
- 通用弹窗：`UI/MessageBox.cs` / `UI/UIMessageBox.cs`
  - UI 不直接处理网络包，只监听 `UserService` 回调。

**日志系统**
- `Common.Log` / `Common.UnityLog`：统一业务日志 API。
- `UnityLogger`：把 Unity Console 输出也写回 log4net（带位置信息）。
- `UnityConsoleAppender`：把 log4net 日志转发到 Unity Console（内部必须用 Debug 防递归）。

### 6.2 服务器（.NET）

**启动链路**
1. `Program.Main()`（`Program.cs`）
   - `XmlConfigurator.ConfigureAndWatch("log4net.xml")`
   - `Log.Init("GameServer")`
   - 创建 `GameServer` 并 `Init/Start`。
2. `GameServer.Init()`（`GameServer.cs`）
   - `NetService.Init(ServerPort)`：监听 TCP。
   - `DBService.Instance.Init()`：初始化 EF 上下文。
   - `UserService.Instance.Init()`：注册消息处理。
   - 启动后台 Update 线程（tick）。

**网络层**
- `Network/TcpSocketListener.cs`：底层监听 socket。
- `Network/NetService.cs`
  - `OnSocketConnected` 创建 `NetConnection<NetSession>`。
  - `DataReceived` 把字节交给 `packageHandler.ReceiveData()`。
- `Network/NetConnection.cs` / `NetSession.cs`
  - `NetSession` 保存登录态（`Session.User`）。
  - `Verified` 标记连接是否通过登录校验。
- `MessageDistributer<NetConnection<NetSession>>`
  - 按消息类型路由到 Services 的订阅回调。

**账号/角色网关**
- `Services/UserSerevice.cs`
  - 处理注册/登录/创角/删角四类请求。
  - 全部走 EF：`DBService.Instance.Entities`。
  - 登录成功后把 `TUser` 绑定到 `sender.Session.User`。

**数据库与实体**
- `Services/DBService.cs`：持有 EF 上下文 `Entities`。
- `Entities/*` + `TUser/TPlayer/TCharacter.cs`：EF 实体与表结构对应。

**服务器主循环**
- `Utils/Time.cs`：提供 tick/time 统计。
- `GameServer.Update()`：每 100ms Tick 一次（目前只提供时钟驱动，后续玩法可挂在这里或消息回调里）。

---

## 7) 调试与日志

### 7.1 服务器调试
- Visual Studio 下 `F5` 启动即可断点调试。
- 关键断点位：
  - `Network/NetService.DataReceived`
  - `Services/UserSerevice.OnRegister/OnLogin/OnCreateCharacter/OnDeleteCharacter`
  - `Services/DBService`（查询/SaveChanges）
- 日志：
  - `Src/Server/GameServer/GameServer/Log/server-detailed.log*`
  - 若日志不输出，先确认 `bin/Debug/log4net.xml` 存在（csproj 已设置 Always Copy）。

### 7.2 客户端调试
- Unity Console 先看编译报错。
- 运行时断点：
  - 在 Rider/VS 里 Attach 到 Unity Editor。
  - 关键断点位：
    - `LoadingManager.Start`
    - `NetClient.Update/ProcessRecv/ProcessSend`
    - `Services/UserService.OnUserLogin/OnUserRegister/...`
- 日志：
  - Unity Console（实时）
  - `Src/Client/Log/client.log`

---

## 8) 常见问题与排查清单

### 8.1 服务器启动失败
- **报 SqlException / 无法打开数据库**
  - 检查 `ExtremeWorld` 是否存在、表是否初始化。
  - 检查 `App.config` 连接串的 `data source` 是否正确。
- **端口 8000 被占用**
  ```bash
  netstat -ano | findstr 8000
  ```
  结束占用进程或改 `GameServer.Properties.Settings.Default.ServerPort`。

### 8.2 客户端连不上服务器
- 确认服务器已启动并监听 8000。
- 检查防火墙/杀毒是否阻止本地端口。
- 看 `client.log` 是否有 “Connect Server before Send Message!”。

### 8.3 协议不匹配 / 消息解析失败
- 先确认 proto 变更后是否运行 `genproto.cmd`。
- `message.proto` 与 `message.cs` 必须同时提交、双端重新编译。

### 8.4 DLL 版本问题（Unity 编译红）
- `Common.dll` / `Protocol.dll` 版本不一致或未复制。
- 重新编译共享库并确认 `Assets/ThirdParty` 内 DLL 更新时间。

### 8.5 数据不同步 / 配表不生效
- 运行 `cd Src/Data; python excel2json.py`。
- 确认：
  - `Src/Data/Data` 有最新 txt。
  - `Src/Client/Data` 被同步。
  - `Src/Server/GameServer/GameServer/bin/Debug/Data` 被同步。

---

## 9) 新人练习任务（建议一周内完成）

1. **跑通全链路**
   - 用新账号完成注册→登录→创角→删角。
2. **配表改动**
   - 改一个 `Tables/CharacterDefine.xlsx` 字段值，
   - 运行 `excel2json.py`，
   - 在客户端 UI 中观察变化。
3. **协议小改**
   - 在 `message.proto` 给 `NCharacterInfo` 加一个可选字段（如 `Title`），
   - 运行 `genproto.cmd`，
   - 客户端/服务器各打印该字段（默认空即可）。
4. **新增一个 UI 交互**
   - 在 `UICharacterSelect` 里加一个按钮（如“随机名字”），
   - 只改 UI 和本地逻辑，不碰网络。
5. **阅读并记录**
   - 按第 6 节源码导读顺序逐个打开文件，
   - 画出你理解的“客户端请求→服务器响应”流程图，
   - 提交给负责人 review。

---

## 10) 进一步阅读
- 团队/提交流程/完整排查：`.codex/PROJECT_GUIDELINES.md`
- Unity MCP 使用与工具：`.codex/PROJECT_GUIDELINES.md` 的 Unity MCP 小节
- 任何不明确的地方，先问负责人或在群里同步。

