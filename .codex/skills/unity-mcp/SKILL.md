# Skill: unity-mcp（Unity MCP 使用）

## 前置条件
- Unity Editor 已打开项目 `Src/Client`
- Unity MCP 服务已启动并可连通（HTTP Server/Bridge 正在运行）
- 客户端已配置 MCP 服务器 URL（例如 `http://localhost:8080/mcp`）

## 何时使用
- 用户说“用 Unity MCP 读取场景/层级/Console/Selection/自定义工具”

## 常用调用顺序（建议）
1. 多实例：先查 `unity_instances`，必要时 `set_active_instance(...)`
2. 状态：`editor_state` / `editor_selection`
3. 排错：`read_console`（先看最新错误/警告）
4. 操作：按需使用 `manage_scene` / `manage_gameobject` / `manage_asset`

## 常用工具名（速查）
- `unity_instances`：列出可用 Unity 实例
- `set_active_instance`：切换当前目标实例
- `editor_state`：编辑器状态（场景、播放态等）
- `editor_selection`：当前选中对象
- `read_console`：读取 Console 日志/错误
- `manage_scene`：场景查询/打开/保存（按工具能力）
- `manage_gameobject`：查找/创建/修改 GameObject
- `manage_asset`：查询/创建/修改资源
- `manage_script` / `script_apply_edits`：脚本编辑（谨慎使用，注意编译）

## 示例（你可以直接对 AI 这么说）
- “列出 Unity 实例”：调用 `unity_instances`
- “切到实例 X”：调用 `set_active_instance(<Name@hash>)`
- “读取 Console 最新 50 行”：调用 `read_console`
- “读取当前场景中 ButtonPlay 的层级路径”：调用 `manage_gameobject` 查找并返回层级

## 常见问题
- 连不上：确认 Unity MCP 终端没关闭、URL 含 `/mcp`、端口一致
- 多实例串了：先 `unity_instances` 再 `set_active_instance`

## 进一步阅读（长文档）
- `.codex/docs/project-guidelines/12_unity-mcp.md`



