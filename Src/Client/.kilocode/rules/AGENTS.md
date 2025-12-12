# Codex Agent Rules（MyMMORPG）

> 目标：让 AI 在新会话 5 分钟内掌握项目关键上下文，并以最少往返高效完成需求。  
> 说明：团队与项目规范以 `.codex/PROJECT_GUIDELINES.md` 为准；本文件只描述 AI 在本仓库的默认工作方式与最小必备上下文。

---

## 1) 交互与执行约定

- 回复语言：简体中文。
- 需求处理：
  - 复杂/多步骤任务（涉及多文件、工具调用或需要你选择方案）：先列 to-do 并等待确认。
  - 你已明确“全部确认/开始执行”时，可一次性连续完成全部 to-do；除非遇到阻塞，不再中途反复确认。
  - 纯问答、简单澄清、不改代码的短任务可直接回答。
- 不确定点必须提问；不要猜配置/路径。
- 只改你要求的内容，避免顺手改无关文件。
- 工具偏好：
  - 搜索用 `rg`，读文件尽量并行。
  - PowerShell 下用 `;` 分隔命令，不使用 `&&`。
  - 编辑文件统一用 `apply_patch`，保持改动最小可读。
- 内置 skills：平台提供 `xlsx/pdf/pptx/docx/...` 等内置 skills（列表见 `.codex/PROJECT_GUIDELINES.md`），需要时直接“调用 <skill-name> 做 X”。

---

## 2) 新会话快速上手（必做）

1. 读本文件。
2. 读 `.codex/PROJECT_GUIDELINES.md`（项目完整手册、团队流程、代码风格）。
3. 运行 `git status --short` 了解当前脏文件和进行中的改动。
4. 按任务按需加载关键文件：
   - 协议：`Src/Lib/proto/message.proto`、`Src/Lib/Protocol/message.cs`、`Tools/genproto.cmd`
   - 策划数据：`Src/Data/excel2json.py`、`Src/Data/Tables/*`、`Src/Data/Data/*`
   - 客户端 Unity：`Src/Client/Assets/Game/Scripts/**`、`Src/Client/Assets/Resources/log4net.xml`
   - 服务器：`Src/Server/GameServer/GameServer/**`
5. Unity 相关任务：先读 `unity://project/info`、`unity://editor/state`，必要时用 `read_console` 看错误/告警。

---

## 3) 项目关键事实（速记）

- 客户端：`Src/Client`（Unity 6000.0.53f1）。
- 服务器：`Src/Server/GameServer/GameServer`（.NET Framework 4.6.2）。
- 共享库：`Src/Lib/Common`、`Src/Lib/Protocol`（双端共用）。
- 协议链路：改 `Src/Lib/proto/message.proto` → 运行 `Tools/genproto.cmd` → 生成 `Src/Lib/Protocol/message.cs`；双端必须同步提交/编译。
- 策划数据链路：`Src/Data/Tables/*.xlsx` → `python Src/Data/excel2json.py` → 产物 `Src/Data/Data/*.txt`；脚本会同步到：
  - 客户端 `Src/Client/Data`
  - 服务器 `Src/Server/GameServer/GameServer/bin/Debug/Data`
- 默认联调：客户端连 `127.0.0.1:8000`。
- 日志：
  - 业务代码用 `Common.Log` / `Common.UnityLog`；不要用 `Debug.Log*`。
  - `UnityConsoleAppender.cs` 内部保留 `Debug.Log*` 防止 log4net 递归。

---

## 4) 常用任务配方

- 协议更新：
  1) 修改 `Src/Lib/proto/message.proto`
  2) `cd Tools; genproto.cmd`
  3) 提交 proto + 生成的 `Src/Lib/Protocol/message.cs`
- 数据更新：
  1) 修改 `Src/Data/Tables/*.xlsx`
  2) `cd Src/Data; python excel2json.py`
  3) 检查 `Src/Data/Data`、客户端/服务器同步目录
- Common/Protocol DLL 更新（客户端依赖）：
  1) `msbuild Src/Lib/Common/Common.csproj /p:Configuration=Debug`
  2) `msbuild Src/Lib/Protocol/Protocol.csproj /p:Configuration=Debug`
  3) 确认 Post-build 已复制到 `Src/Client/Assets/ThirdParty/*`

---

## 5) 输出与验证

- 能本地验证就验证，但不修复无关失败。
- 最终回复需包含：
  - 修改/新增/删除的文件列表
  - 关键改动点
  - 建议的手动验证步骤或可跑的测试命令
