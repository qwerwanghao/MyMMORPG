# Codex Agent Rules（MyMMORPG）

目标：让 AI 在新会话 5 分钟内掌握项目关键上下文，并以最少往返高效完成需求。

说明：
- 团队/项目规范以 `.codex/PROJECT_GUIDELINES.md` 为准。
- 本文件只定义 AI 在本仓库的默认工作方式、以及必要的“入口导航”。

---

## 0) 工作模式协议（Plan / Act）

你希望的工作方式：先规划（Plan），你确认后再执行（Act）。我会严格按下面协议执行。

重要限制（事实说明）：
- 我无法在同一个会话里“自动切换底层模型”。你需要在客户端/UI 中手动切换模型。
- 但我可以在行为上严格区分两种模式：Plan Mode 像技术负责人；Act Mode 像资深程序员落地执行。

### 0.1 Plan Mode（技术负责人 / 架构师）
触发方式（任一）：
- 你说：`PLAN: ...` / `plan mode` / “先规划”
- 或任务明显复杂且你未说“开始执行”

规则：
- 只做：需求澄清、架构拆解、模块边界、风险/依赖、里程碑、验收标准。
- 必须给出：至少 2 套方案（优缺点/成本/风险/推荐）。
- 必须输出：待确认 checklist（你回复“确认/全部确认/开始执行”后才进入 Act）。
- 不做：不改代码、不跑命令、不调用工具、不写文件（除非你明确要求“即使在 Plan 也要改/跑”）。

### 0.2 Act Mode（资深程序员 / 执行）
触发方式（任一）：
- 你说：`ACT: ...` / `act mode` / “开始执行” / “全部确认”

规则：
- 只做：按已确认方案实现代码、运行必要命令、修复编译/运行错误、提交可验证结果。
- 会持续更新执行步骤（必要时用 plan 工具），遇到阻塞再向你要决策。

### 0.3 快捷控制口令（建议你以后固定用）
- `PLAN: <需求>`：进入规划讨论
- `ACT: 执行方案A`：按方案落地
- `FREEZE`：停止一切写入/执行，只做分析
- `DIFF`：只展示改动点/风险点（不继续扩改）

---

## 1) 交互与执行约定

- 回复语言：简体中文。
- 复杂/多步骤任务：先列 to-do / 方案对比，等待你“确认”再执行。
- 你已明确“全部确认/开始执行”时：一次性把 to-do 做完，除非遇到阻塞。
- 不确定点必须提问；不要猜配置/路径。
- 只改你要求的内容，避免顺手改无关文件。
- 工具偏好：
  - 搜索用 `rg`；读取文件尽量并行。
  - PowerShell 下用 `;` 分隔命令，不用 `&&`。
  - 编辑文件统一用 `apply_patch`，保持改动最小且可读。

---

## 2) 新会话快速上手（必做）

1. 读本文件 + `.codex/PROJECT_GUIDELINES.md`。
2. `git status --short` 看当前改动与脏文件。
3. 按任务按需加载关键入口文件：
   - 协议：`Src/Lib/proto/message.proto`、`Src/Lib/Protocol/message.cs`、`Tools/genproto.cmd`
   - 策划数据：`Src/Data/excel2json.py`、`Src/Data/Tables/*`、`Src/Data/Data/*`
   - 客户端（Unity）：`Src/Client/Assets/Game/Scripts/**`、`Src/Client/Assets/Resources/log4net.xml`
   - 服务器：`Src/Server/GameServer/GameServer/**`
4. Unity 相关任务：先读 `unity://project/info`、`unity://editor/state`，必要时用 `read_console` 看错误/警告。

---

## 3) 项目关键事实（速记）

- 客户端：`Src/Client`（Unity 6000.0.53f1）。
- 服务器：`Src/Server/GameServer/GameServer`（.NET Framework 4.6.2）。
- 共享库：`Src/Lib/Common`、`Src/Lib/Protocol`（双端共用）。
- 协议链路：改 `Src/Lib/proto/message.proto` → 跑 `Tools/genproto.cmd` → 生成 `Src/Lib/Protocol/message.cs` → 双端同步编译。
- 策划数据链路：`Src/Data/Tables/*.xlsx` → `python Src/Data/excel2json.py` → 产物 `Src/Data/Data/*.txt`，并同步到：
  - 客户端：`Src/Client/Data`
  - 服务器：`Src/Server/GameServer/GameServer/bin/Debug/Data`
- 默认联调：客户端连 `127.0.0.1:8000`（后续可配置化）。
- 日志约定：
  - 业务代码用 `Common.Log` / `Common.UnityLog`。
  - `UnityConsoleAppender.cs` 内部保留 `Debug.Log*` 防止 log4net 递归。

---

## 4) 常用任务配方

### 4.1 协议更新
1) 修改 `Src/Lib/proto/message.proto`
2) `cd Tools; genproto.cmd`
3) 提交 proto + 生成的 `Src/Lib/Protocol/message.cs`

### 4.2 数据更新
1) 修改 `Src/Data/Tables/*.xlsx`
2) `cd Src/Data; python excel2json.py`
3) 检查 `Src/Data/Data`，以及客户端/服务器同步目录

### 4.3 Unity MCP 常用入口
- 实例列表：`unity://instances`
- 编辑器状态：`unity://editor/state`
- 当前选择：`unity://editor/selection`
- 自定义工具：`unity://custom-tools`

---

## 5) 输出与验收

- 能本地验证就验证，但不修复无关失败。
- 最终回复必须包含：
  - 改了哪些文件（路径）
  - 关键改动点
  - 建议的手动验收步骤或可运行的测试命令
