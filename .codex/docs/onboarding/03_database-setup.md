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






