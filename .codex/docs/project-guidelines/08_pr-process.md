## 8) 提交流程

### 提交信息格式

- 动词开头、范围清晰
- 示例：`修复登录重试逻辑` / `Update login retry loop`

### 协议更新

修改 `message.proto` 后，**必须同时提交**生成的 `message.cs`。

### PR 要求

- [ ] 说明影响范围
- [ ] 列出受影响场景（如 `Assets/Levels/Test.unity`）
- [ ] 附相关截图/日志
- [ ] Unity 无报错
- [ ] `msbuild` Debug 通过

### PR 噪音控制（强烈建议）

为降低 review 成本、减少冲突，请尽量让 PR 只包含“与本需求直接相关”的文件：

- **通常不要提交**：`Src/Client/Log/**`、`Src/Client/Logs/**`、`Src/Client/UserSettings/**`、`Src/Client/Packages/packages-lock.json`、`**/*.csproj.user`、临时目录（如 `.idea/`、`.claude/`、`.specify/`）。
- **二进制与资源谨慎提交**：`Assets/ThirdParty/**/*.dll`、大体积 `*.unity` / `*.prefab` 只有在确实与需求相关（如新增场景、修改 UI 绑定）时才提交。
- **必须提交资源改动时**：建议单独一个 commit，并在 PR 描述里说明“为何需要这些资源变更”。

---




