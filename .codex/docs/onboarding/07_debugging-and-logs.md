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






