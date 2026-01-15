## 12) Unity MCP 使用说明（精简）

- 环境：Python 3.10+、`uv`，Unity 6000.0.53f1；本项目已集成 Unity MCP（Bridge + Server）。
- 启动本地 HTTP 服务（推荐）：Unity 菜单 `Window > MCP for Unity`，保持 `Transport=HTTP`（默认 `http://localhost:8080/mcp`），点击“Start Local HTTP Server”，保持弹出的终端窗口不关闭。
- 手动启动（可选，HTTP）：在 `UnityMcpServer/src` 目录执行  
  `uvx --from "git+https://github.com/CoplayDev/unity-mcp@v8.1.0#subdirectory=Server" mcp-for-unity --transport http --http-url http://localhost:8080`
- 客户端配置示例（Claude/Cursor 等）：在对应配置中加入  
  ```json
  {
    "mcpServers": {
      "UnityMCP": { "url": "http://localhost:8080/mcp" }
    }
  }
  ```
- 常用工具/资源（通过 MCP 直接调用）：  
  `manage_scene`、`manage_gameobject`、`manage_asset`、`manage_script`/`script_apply_edits`、`read_console`、`editor_state`、`editor_selection`、`unity_instances`、`set_active_instance`。
- 多实例：先查 `unity_instances`，再用 `set_active_instance(<Name@hash>)` 绑定目标实例。
- 故障排查：确认 Unity 已打开、HTTP 服务在跑（终端未关闭）、`uv --version` 正常；如连接失败，核对配置里的 URL 与实际端口路径完全一致（含 `/mcp`）。




