## 1) 整体速览

- **双端结构**：Unity 客户端 `Src/Client` + .NET 4.6.2 服务器 `Src/Server/GameServer`，共享库在 `Src/Lib`，数据在 `Src/Data`，工具在 `Tools/`。
- **协议链路**：`Src/Lib/proto/message.proto` → `Tools/genproto.cmd` → 生成 `Src/Lib/Protocol/message.cs`；`Common` 提供日志/网络/单例基座，客户端与服务器共同使用。
- **客户端启动**：`LoadingManager` 初始化日志与数据（`DataManager` 读 `Src/Data/Data/*.txt`），注册服务后进入登录 UI；`UserService` 通过 `NetClient` 连接 `127.0.0.1:8000`，`PackageHandler` + `MessageDistributer` 处理收发。
- **服务器启动**：`Program` 启动 `GameServer`，`NetService` 监听并创建 `NetConnection`，`UserService` 订阅登录/注册，`DBService` 访问 `ExtremeWorldEntities`。
- **推荐调试顺序**：先启动服务器（确认端口/数据库可用），再用 Unity 打开 `Src/Client` 播放场景（如 `Assets/Levels/Test.unity`），验证登录往返。

---






