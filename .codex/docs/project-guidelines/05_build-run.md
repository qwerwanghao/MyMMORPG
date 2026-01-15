## 5) 构建与运行

### Unity 客户端

```bash
# 编辑器直接播放
# 或无界面测试
Unity.exe -projectPath Src\Client -quit -batchmode -playModeTest
```

### 服务器

```bash
# Debug 构建
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug

# Release 构建
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Release

# 运行
cd Src\Server\GameServer\GameServer\bin\Debug
GameServer.exe
```

### 日志配置

| 端 | 配置文件 |
|----|----------|
| 客户端 | `Assets/Resources/log4net.xml` |
| 服务器 | `Src/Server/GameServer/GameServer/log4net.xml` |

---






