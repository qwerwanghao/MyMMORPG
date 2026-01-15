## 2) 环境依赖

### 必需工具

| 组件 | 版本要求 | 说明 |
|------|----------|------|
| **Unity** | 6000.0.53f1+ | 客户端开发环境 |
| **.NET Framework** | 4.6.2 | 服务器运行时 |
| **SQL Server** | LocalDB 或完整版 | 数据库（实例名参考 `App.config`） |
| **protoc** | 3.2.0 (已内置) | 协议生成工具，位于 `Tools/protoc-3.2.0-win32/` |
| **Visual Studio** | 2019+ | 服务器项目构建（需安装 .NET 桌面开发工作负载） |

### 数据库配置

服务器默认连接字符串位于 `Src/Server/GameServer/GameServer/App.config`：

```xml
<connectionStrings>
  <add name="ExtremeWorldEntities" 
       connectionString="...data source=HOMEPC\MMORPG;Initial Catalog=ExtremeWorld..." />
</connectionStrings>
```

**首次运行前**：
1. 创建 SQL Server 实例（或使用 LocalDB）
2. 创建数据库 `ExtremeWorld`
3. 执行 `Src/Server/GameServer/GameServer/Entities.edmx.sql` 初始化表结构
4. 修改 `App.config` 中的 `data source` 为本地实例名

---




