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




