## 12) Unity MCP 使用说明（完整版）

> 本节以本项目的使用为主，并参考 `CoplayDev/unity-mcp` 的中文 README 进行补全：  
> https://github.com/CoplayDev/unity-mcp/blob/main/README-zh.md

---

### 12.1 适用场景

- 需要“读取 Unity Editor 状态/Console/Selection/场景层级”
- 需要“批量创建/修改 GameObject、组件、资源、脚本”，减少手工重复操作
- 需要把“需求 → 可执行工具调用”标准化（尤其是排查/工具化任务）

---

### 12.2 前置条件（最低要求）

- Unity：本项目客户端为 `Unity 6000.0.53f1`
- Python：`3.10+`
- `uv`（Python 工具链管理器，用于 `uvx` 启动 MCP Server）
- 你的 MCP 客户端（Codex/Claude Desktop/Cursor 等）支持配置 MCP server

Windows 安装 `uv`（推荐）：
```powershell
winget install --id=astral-sh.uv -e
```

检查：
```powershell
uv --version
```

---

### 12.3 本项目集成方式（你需要做什么）

本项目已集成 Unity MCP（Unity 侧提供菜单与窗口）。日常使用通常不需要你手动 clone/安装 `unity-mcp` 仓库。

如果你打开 Unity 后看不到菜单/窗口，才需要回头检查是否遗漏了包或是否被禁用。

#### 12.3.1（可选）安装方式速览（参考官方）

> 如果你的项目里还没集成 Unity MCP，官方常见安装方式包括：

- Unity Asset Store 安装（最省事）
- Unity Package Manager：`Add package from git URL...`
  - `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#<tag>`
- OpenUPM：`openupm add com.coplaydev.unity-mcp`

---

### 12.4 启动 MCP Server（HTTP 推荐）

#### 方式 A：Unity 内启动（推荐）

1. 打开 Unity（项目：`Src/Client`）
2. 菜单：`Window > MCP for Unity`
3. Transport 选择 `HTTP`
4. 端口/URL（示例）：`http://localhost:8080/mcp`
5. 点击 `Start Local HTTP Server`
6. 保持弹出的终端窗口不关闭（关闭即停止服务）

> 注意：URL 必须包含 `/mcp`（很多连接失败都是少了这一段）。

#### 方式 B：命令行启动（可选）

你也可以用 `uvx` 直接拉起 Server（版本与参数以官方 README 为准；以下为常见示例）：
```powershell
uvx --from "git+https://github.com/CoplayDev/unity-mcp@v8.1.0#subdirectory=Server" `
  mcp-for-unity --transport http --http-url http://localhost:8080
```

#### 方式 C：stdio（备选）

一般只在某些客户端/环境不方便走 HTTP 时使用。具体启动与客户端配置参数以官方 README 为准。

---

### 12.5 MCP 客户端配置示例（HTTP）

典型配置（不同客户端字段名可能略有差异，以各客户端文档为准）：
```json
{
  "mcpServers": {
    "UnityMCP": { "url": "http://localhost:8080/mcp" }
  }
}
```

多 Unity 实例时建议流程：
1) 先列实例：`unity_instances`  
2) 再切换：`set_active_instance(<Name@hash>)`

---

### 12.6 常用 Tool（工具）速查

> 下面是常用工具名（具体参数/返回结构请以你当前 MCP 客户端的 schema 为准；不同版本会增删工具）。

**Editor / 状态**
- `unity_instances`：列出所有 Unity Editor 实例
- `set_active_instance`：将后续调用路由到指定实例
- `editor_state`：编辑器状态（场景/播放态等）
- `editor_selection`：当前选中对象
- `read_console`：读取/清理 Console 日志
- `execute_menu_item`：执行 Unity 菜单项（例如保存、刷新等）
- `refresh_unity`：刷新资产数据库（可选触发编译）
- `manage_editor`：控制编辑器状态（播放模式、活动工具、标签、层等）
- `debug_request_context`：返回当前请求上下文（client/session/meta），用于排查“串实例/串会话”

**Scene / GameObject**
- `manage_scene`：加载/保存/创建场景、获取层级、截图等
- `manage_gameobject`：创建/修改/删除/移动/复制 GameObject
- `find_gameobjects`：按 name/tag/layer/component/path 等搜索（适合大场景）

**Components / Materials / Prefabs / Assets**
- `manage_components`：添加/移除组件、设置属性
- `manage_material`：创建/设置材质属性、分配给渲染器
- `manage_prefabs`：Prefab Stage 打开/关闭/保存、从 GameObject 创建 Prefab
- `manage_asset`：导入/创建/删除/搜索资源
- `manage_scriptable_object`：创建/修改 ScriptableObject 资产
- `manage_shader`：Shader CRUD（创建/读取/更新/删除）
- `manage_vfx`：VFX 相关操作（粒子/线条/Trail/VFX Graph 等，视版本支持）

**Scripts（脚本相关）**
- `manage_script`：创建/读取/删除脚本（传统 CRUD）
- `apply_text_edits`：基于行/列的精确文本编辑（适合小改动）
- `script_apply_edits`：结构化编辑（insert/replace/delete），更安全
- `validate_script`：脚本快速验证（basic/standard）
- `find_in_file`：在脚本中正则搜索并返回片段/行号
- `create_script`：创建新的 C# 脚本
- `delete_script`：删除脚本
- `get_sha`：获取脚本 SHA 与元数据（不返回内容），用于防止并发编辑漂移

**批处理 / 性能**
- `batch_execute`：批量执行多条命令（大量创建/修改时建议用，减少往返）

**Tests（如启用）**
- `run_tests`：异步启动测试，返回 `job_id`
- `get_test_job`：轮询测试任务结果

**自定义工具**
- `execute_custom_tool`：执行 Unity 侧注册的项目自定义工具

---

### 12.7 Resource（资源）速查

- `mcpforunity://instances`：Unity 实例列表（对应 `unity_instances`）
- `mcpforunity://custom-tools`：当前项目可用的自定义工具列表

---

### 12.8 推荐工作流（结合本项目）

- 排查编译/运行问题：先 `read_console` → 再定位相关脚本/资源
- 读取某 UI 节点层级：优先 `find_gameobjects`（name=ButtonPlay）→ 再 `manage_gameobject` 获取完整路径
- 批量创建/修复对象：用 `batch_execute` 减少往返
- 改脚本：优先 `script_apply_edits`（避免一次性整段覆盖导致冲突/丢格式），改完后 `refresh_unity` 触发编译并用 `read_console` 看结果

---

### 12.9 示例：你可以这样让 AI 调用 Unity MCP

- “列出 Unity 实例”：调用 `unity_instances`
- “切到实例 X”：调用 `set_active_instance(<Name@hash>)`
- “读取 Console 最新错误”：调用 `read_console`
- “我现在选中的对象是什么”：调用 `editor_selection`
- “找到场景里名为 ButtonPlay 的对象并输出层级路径”：调用 `find_gameobjects` / `manage_gameobject`
- “打开场景 Assets/Levels/MainCity.unity”：调用 `manage_scene`
- “给某个 GameObject 添加组件并设置字段”：调用 `manage_components`
- “批量创建 20 个对象并挂同一套组件”：调用 `batch_execute`

---

### 12.10 常见问题排查（Checklist）

- **连不上 / 404**
  - URL 是否包含 `/mcp`
  - Unity MCP Server 是否仍在运行（Unity 内启动的终端是否被关）
  - 端口是否被占用（必要时换端口）
- **多实例串了**
  - 先 `unity_instances`，再 `set_active_instance`
- **改脚本后 Unity 报错**
  - `refresh_unity` 触发重新编译
  - `read_console` 获取首个编译错误（通常是根因）
- **工具名不一致**
  - 以你 MCP 客户端返回的 tool list/schema 为准（不同版本会有差异）

---

### 12.11（可选）脚本验证更严格：Roslyn

官方文档提到可以用 Roslyn 做更严格的 C# 验证。本项目一般不强制启用；如果你经常需要 AI 大批量改脚本，再考虑开启。


