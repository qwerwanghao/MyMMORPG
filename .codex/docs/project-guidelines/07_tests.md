## 7) 测试

### Unity Play Mode 测试

- 位置：`Assets/Tests/`
- 命名：`FeatureNamePlayModeTests.cs`
> 至少手动验证链路：注册/登录/创角/删角。如暂未添加 PlayMode 测试，PR 中请列出手动用例或补一条 PlayMode 测试。

### 服务器验证

1. 确保 SQL Server `ExtremeWorld` 数据库可用
2. 启动 `GameServer.exe`
3. 覆盖关键流程：登录、注册、创角、删角
4. 无法自动化时在 PR 写明手动验证步骤

---






