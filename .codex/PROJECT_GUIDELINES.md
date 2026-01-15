<!--
This file is generated.
Source of truth: inputs listed in .codex/tools/manifest.json
Regenerate: python .codex/tools/build_codex_docs.py
Output: .codex/PROJECT_GUIDELINES.md
-->

# Repository Guidelines

> **最后更新**: 2025-12-08  
> **项目版本**: Unity 6000.0.53f1 / .NET Framework 4.6.2

---

## 目录
- [使用准则概览](#使用准则概览)
- [1) 整体速览](#1-整体速览)
- [2) 环境依赖](#2-环境依赖)
- [3) 项目结构](#3-项目结构)
- [4) 常用路径/动作](#4-常用路径动作)
- [5) 构建与运行](#5-构建与运行)
- [6) 代码风格](#6-代码风格)
- [7) 测试](#7-测试)
- [8) 提交流程](#8-提交流程)
- [9) 安全与配置](#9-安全与配置)
- [10) 常见问题排查](#10-常见问题排查)
- [11) 快速参考](#11-快速参考)
- [12) Unity MCP 使用说明（完整版）](#12-unity-mcp-使用说明完整版)

---

## 使用准则概览

- 回复语言必须使用简体中文。
- 默认遵循 `.codex/AGENTS.md` 的 Plan/Act 协议（先 Plan，待你 `ACT` 后再执行）。
- 不确定点必须提问；不要猜配置/路径/依赖。
- 只改与需求直接相关的最小集合，避免无关重构与 PR 噪音。

## 1) 整体速览

- **双端结构**：Unity 客户端 `Src/Client` + .NET 4.6.2 服务器 `Src/Server/GameServer`，共享库在 `Src/Lib`，数据在 `Src/Data`，工具在 `Tools/`。
- **协议链路**：`Src/Lib/proto/message.proto` → `Tools/genproto.cmd` → 生成 `Src/Lib/Protocol/message.cs`；`Common` 提供日志/网络/单例基座，客户端与服务器共同使用。
- **客户端启动**：`LoadingManager` 初始化日志与数据（`DataManager` 读 `Src/Data/Data/*.txt`），注册服务后进入登录 UI；`UserService` 通过 `NetClient` 连接 `127.0.0.1:8000`，`PackageHandler` + `MessageDistributer` 处理收发。
- **服务器启动**：`Program` 启动 `GameServer`，`NetService` 监听并创建 `NetConnection`，`UserService` 订阅登录/注册，`DBService` 访问 `ExtremeWorldEntities`。
- **推荐调试顺序**：先启动服务器（确认端口/数据库可用），再用 Unity 打开 `Src/Client` 播放场景（如 `Assets/Levels/Test.unity`），验证登录往返。

---

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

## 3) 项目结构

```
MyMMORPG/
├── Src/
│   ├── Client/                    # Unity 客户端
│   │   ├── Assets/
│   │   │   ├── Game/Scripts/      # 玩法/网络脚本
│   │   │   ├── Levels/            # 场景文件
│   │   │   ├── Resources/         # 资源（含 log4net.xml）
│   │   │   └── ThirdParty/        # 第三方库（含 Common.dll）
│   │   ├── Data/                  # 策划数据 (*.txt)
│   │   └── ProjectSettings/       # Unity 项目配置
│   │
│   ├── Server/GameServer/         # .NET 服务器
│   │   └── GameServer/
│   │       ├── Network/           # 网络层
│   │       ├── Services/          # 业务服务
│   │       ├── Entities/          # 实体定义
│   │       └── App.config         # 服务器配置
│   │
│   ├── Lib/                       # 共享库
│   │   ├── proto/                 # Protobuf 定义
│   │   ├── Protocol/              # 生成的协议代码
│   │   └── Common/                # 日志/网络/单例
│   │
│   └── Data/                      # 策划数据源
│       ├── Data/*.txt             # JSON 格式数据
│       └── Excel2Json.cmd         # Excel 转换工具
│
├── Tools/
│   ├── genproto.cmd               # 协议生成脚本
│   └── protoc-3.2.0-win32/        # protoc 编译器
│
└── .codex/
    └── PROJECT_GUIDELINES.md      # 本文档
```

---

## 4) 常用路径/动作

### 公共库 DLL 更新

已在 Common.csproj / Protocol.csproj 的 Post-build 中自动复制 DLL/PDB 到 `Assets\ThirdParty\Common` / `Assets\ThirdParty\Protocol`。如需手工执行，可参考：

```bash
# 1. 编译 Common 项目
msbuild Src\Lib\Common\Common.csproj /p:Configuration=Debug
# 2. 手工复制（如需覆盖）
copy Src\Lib\Common\bin\Debug\Common.dll Src\Client\Assets\ThirdParty\Common\Common.dll
```

同理，Protocol：
```bash
# 1. 编译 Protocol 项目
msbuild Src\Lib\Protocol\Protocol.csproj /p:Configuration=Debug
# 2. 手工复制（如需覆盖）
copy Src\Lib\Protocol\bin\Debug\Protocol.dll Src\Client\Assets\ThirdParty\Protocol\Protocol.dll
```

### 协议生成

```bash
# 在仓库根目录运行
cd Tools
genproto.cmd
```

修改 `Src/Lib/proto/message.proto` 后执行，自动生成 `Src/Lib/Protocol/message.cs`。

### 日志位置

| 端 | 路径 | 说明 |
|----|------|------|
| 客户端 | `Src/Client/Log/client.log` | Unity 播放时生成 |
| 服务器 | `Src/Server/GameServer/GameServer/Log/`（或 log4net.xml 指定路径） | 查看最新 server-detailed.log* |

### Codex 内置 skills

Codex/Claude 提供一组内置 skills，无需额外启动服务，直接在对话里说“调用 <skill-name> 做 X”即可使用。  
这些 skills 由平台固定提供，和仓库里的脚本/目录无关。

可用列表：
- `algorithmic-art`：用 p5.js 做算法/生成艺术。
- `brand-guidelines`：按 Anthropic 品牌规范美化产物。
- `canvas-design`：生成静态视觉设计（png/pdf）。
- `doc-coauthoring`：协作式撰写文档/提案/技术规格。
- `docx`：创建/编辑/分析 .docx 文档（含批注、修订）。
- `frontend-design`：高质量前端界面/组件/页面设计与代码。
- `internal-comms`：内部沟通文档写作模板与格式。
- `mcp-builder`：指导搭建 MCP 服务器（Python/TS）。
- `pdf`：PDF 提取/生成/合并/表单处理。
- `pptx`：PPT 制作/编辑/分析。
- `skill-creator`：创建/更新自定义 skills 的指南。
- `slack-gif-creator`：制作适配 Slack 的动图 GIF。
- `theme-factory`：给文档/网页/幻灯等套主题样式。
- `web-artifacts-builder`：构建复杂 HTML/React artifacts（多组件）。
- `webapp-testing`：用 Playwright 测试本地 Web 应用。
- `xlsx`：表格/Excel 读写分析与可视化。

### 数据更新

```bash
# 从 Excel 更新策划数据（默认 Python）
cd Src\Data
python excel2json.py

# 备用入口（.cmd）
# Excel2Json.cmd
```
- 转表输出在 `Src/Data/Data`，Python 脚本会：
  - 复制指定文件到客户端 `Src/Client/Data`
  - 全量同步到服务器 `Src/Server/GameServer/GameServer/bin/Debug/Data`

---

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

## 6) 代码风格

### EditorConfig 规范

- 编码：UTF-8 BOM
- 换行：CRLF
- 缩进：四空格
- 花括号：独占一行

### 命名约定

| 类型 | 风格 | 示例 |
|------|------|------|
| 类型/公开成员 | PascalCase | `UserService`, `OnLogin` |
| 局部变量/参数 | camelCase | `userName`, `response` |
| 私有字段 | camelCase 或 `[SerializeField]` | `pendingMessage` |

---

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

## 9) 安全与配置

- **敏感配置**：不提交数据库密码等敏感信息；本地维护 `App.config` 变体
- **默认连接**：客户端 `127.0.0.1:8000`（见 `UserService.cs`），改动需同步团队
- **服务器端口**：配置于 `App.config` 的 `userSettings` 节

---

## 10) 常见问题排查

### 连接失败

| 症状 | 可能原因 | 解决方案 |
|------|----------|----------|
| 客户端提示"无法连接服务器" | 服务器未启动 | 先启动 `GameServer.exe` |
| | 端口被占用 | 检查 8000 端口：`netstat -ano | findstr 8000` |
| | 防火墙阻止 | 添加入站规则或临时关闭防火墙 |

### 数据库错误

| 症状 | 可能原因 | 解决方案 |
|------|----------|----------|
| `SqlException: 无法打开数据库` | 数据库不存在 | 创建 `ExtremeWorld` 数据库 |
| `SqlException: 登录失败` | 连接字符串错误 | 检查 `App.config` 中的 `data source` |
| | SQL Server 未启动 | 启动 SQL Server 服务 |

### 协议不匹配

| 症状 | 可能原因 | 解决方案 |
|------|----------|----------|
| 消息解析失败 | 客户端/服务器协议版本不一致 | 重新运行 `genproto.cmd` 并重新编译双端 |

### Unity 编译错误

| 症状 | 可能原因 | 解决方案 |
|------|----------|----------|
| `Common.dll` 相关错误 | DLL 版本过旧 | 重新编译并复制 `Common.dll` |
| 协议类找不到 | `message.cs` 未更新 | 运行 `genproto.cmd` |

---

## 11) 快速参考

### 关键文件速查

| 功能 | 文件路径 |
|------|----------|
| 客户端入口 | `Assets/Game/Scripts/Core/LoadingManager.cs` |
| 客户端网络服务 | `Assets/Game/Scripts/Services/UserService.cs` |
| 服务器入口 | `Src/Server/GameServer/GameServer/Program.cs` |
| 服务器网络服务 | `Src/Server/GameServer/GameServer/Network/NetService.cs` |
| 协议定义 | `Src/Lib/proto/message.proto` |
| 数据库配置 | `Src/Server/GameServer/GameServer/App.config` |

### 常用命令

```bash
# 生成协议
cd Tools && genproto.cmd

# 编译服务器
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug

# 运行服务器
Src\Server\GameServer\GameServer\bin\Debug\GameServer.exe

# 检查端口占用
netstat -ano | findstr 8000
```

---

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

