## 5) 常用开发工作流

### 5.1 协议更新（客户端/服务器都要同步）
1. 修改 `Src/Lib/proto/message.proto`。
2. 生成协议：
   ```bash
   cd Tools
   genproto.cmd
   ```
3. 检查 `Src/Lib/Protocol/message.cs` 变化。
4. 双端重新编译并提交 proto + message.cs。

### 5.2 表格数据更新（默认 Python）
1. 修改 `Src/Data/Tables/*.xlsx`。
2. 转表并同步：
   ```bash
   cd Src\Data
   python excel2json.py
   ```
3. 产物在 `Src/Data/Data/*.txt`，脚本会自动同步到：
   - 客户端：`Src/Client/Data`
   - 服务器：`Src/Server/GameServer/GameServer/bin/Debug/Data`

### 5.3 共享库 DLL 更新（客户端依赖）
当你改了 `Src/Lib/Common` 或 `Src/Lib/Protocol`：
```bash
msbuild Src/Lib/Common/Common.csproj /p:Configuration=Debug
msbuild Src/Lib/Protocol/Protocol.csproj /p:Configuration=Debug
```
Post-build 会复制 DLL/PDB 到 `Src/Client/Assets/ThirdParty/*`。若 Unity 仍报旧版本问题，可手动覆盖。

---




