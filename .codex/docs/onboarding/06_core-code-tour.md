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






