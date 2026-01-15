## 11) 快速参考

### 关键文件速查

| 功能 | 文件路径 |
|------|----------|
| 客户端入口 | `Assets/Game/Scripts/Core/LoadingManager.cs` |
| 客户端网络服务 | `Assets/Game/Scripts/Services/UserService.cs` |
| 服务器入口 | `Src/Server/GameServer/GameServer/Program.cs` |
| 服务器网络服务 | `Src/Server/GameServer/GameServer/Network/NetService.cs` |
| 协议定义 | `Src/Lib/proto/message.proto` |
| 数据库配置 | `Src/Server/GameServer/GameServer/App.config` |

### 常用命令

```bash
# 生成协议
cd Tools && genproto.cmd

# 编译服务器
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug

# 运行服务器
Src\Server\GameServer\GameServer\bin\Debug\GameServer.exe

# 检查端口占用
netstat -ano | findstr 8000
```

---




