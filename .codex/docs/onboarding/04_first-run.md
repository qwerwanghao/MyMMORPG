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




