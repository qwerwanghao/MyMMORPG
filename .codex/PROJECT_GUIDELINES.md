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
- [12) Unity MCP 使用说明（精简）](#12-unity-mcp-使用说明精简)

---

## 使用准则概览

- 回复语言必须使用简体中文。
- 接到需求后先整理详细的 to-do 列表，发送用户确认；若用户提出修改意见，需重新整理并确认。
- 执行 to-do 时，每完成一项都要暂停并请用户确认后再继续下一项。
- 涉及代码或文档改动时，使用 Approve 流程，请用户确认后再提交最终变更。
- 开发过程中若有任何不确定之处，必须主动向用户提问。

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
