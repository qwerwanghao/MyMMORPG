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




