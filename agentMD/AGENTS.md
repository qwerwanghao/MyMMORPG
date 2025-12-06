# Repository Guidelines

## 项目结构与模块组织
仓库以 `Src/Client` 的 Unity 客户端为核心，玩法脚本集中在 `Assets/Game/Scripts`，项目配置位于 `ProjectSettings`。`.vscode`、`.idea` 等目录仅保存本地编辑器设置，勿随意提交。`.meta` 文件由 Unity 管理，删除或手动修改会导致资源引用失效。服务器端存放在 `Src/Server/GameServer`，通过 `GameServer.sln` 打开解决方案，入口代码在 `GameServer/Program.cs`。公共库统一置于 `Src/Lib`：`proto/` 保存协议定义，`Protocol/` 为生成的 C# 代码，`Common/` 包含日志与网络工具。策划数据、SQL 脚本等放入 `Data/`，工具脚本与 `protoc` 二进制在 `Tools/`。

## 构建、测试与开发命令
Unity 客户端可直接在编辑器中运行，CI 或本地无界面检查可执行 `Unity.exe -projectPath Src\Client -quit -batchmode -playModeTest`。服务器端使用 `msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug` 编译，生成文件位于 `bin/Debug`，通过 `GameServer.exe` 启动。更新 `Src/Lib/proto/message.proto` 后，务必在仓库根目录运行 `Tools\genproto.cmd` 以同步客户端与服务器协议。若修改日志策略，需同时维护 `Src/Client/Assets/Resources/log4net.xml` 与服务器 `App.config`。

## 代码风格与命名约定
仓库使用 `.editorconfig` 统一 UTF-8 BOM、CRLF 结尾以及四空格缩进，请保持 IDE 同步。C# 命名沿用既有规则：对外可见类型与方法使用 PascalCase，本地变量、参数使用 camelCase。私有 Unity 字段通过 `[SerializeField]` 或 `public` 暴露，避免自定义前缀。花括号必须独占一行（`csharp_new_line_before_open_brace = all`），方法内逻辑块使用空行分割，便于阅读。

## 测试指引
Unity Play Mode 测试统一放在 `Assets/Tests` 并命名为 `FeatureNamePlayModeTests.cs`，确保能在 Test Runner 中自动发现。服务器修改后，请在本地 SQL Server `ExtremeWorld` 数据库上启动 `GameServer.exe`，验证登录、注册等关键流程。暂无法自动化的场景，需要在 PR 描述中补充手动验证步骤与观察结果。

## 提交与合并请求规范
提交信息遵循动词开头、范围明确的写法，例如 `修复登录重试逻辑` 或 `Update login retry loop`。涉及协议更新时，同一提交必须包含生成的 `message.cs`。PR 描述需概述玩法或系统影响，列出受影响的 Unity 场景（如 `Assets/Levels/Test.unity`），并附上相关截图或日志。发起评审前确认 Unity 启动无报错、`msbuild` 在 Debug 配置下无警告。

## 安全与配置提示
敏感配置（数据库密码、外网地址等）不得写入仓库，建议在本地维护 `App.config` 的变体或使用用户级配置。客户端默认在 `Assets/Game/Scripts/Services/UserService.cs` 连接 `127.0.0.1:8000`，若调整服务器地址，请及时在 PR 及团队频道同步，避免协作环境失联。
