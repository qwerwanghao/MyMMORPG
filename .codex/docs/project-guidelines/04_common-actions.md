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




