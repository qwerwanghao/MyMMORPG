# MyMMORPG 项目上下文指南

## 项目概览

MyMMORPG 是一个双端（客户端+服务器）的 MMORPG 教学/练手示例工程。它采用了 Unity 作为客户端开发引擎，.NET 作为服务器开发框架，并使用 Protobuf 进行网络通信协议的定义。

### 核心技术栈
- **客户端**：Unity 6000.0.53f1（位于 `Src/Client`）。负责 UI 交互、场景表现及网络收发。
- **服务器**：.NET Framework 4.6.2（位于 `Src/Server/GameServer`）。负责 TCP 监听、业务逻辑分发、数据库读写。
- **共享库**：`Src/Lib/Common`（基础功能）和 `Src/Lib/Protocol`（Protobuf 生成代码）。
- **数据驱动**：策划数据通过 Excel 维护，使用 Python 脚本导出为 JSON/Text 格式供双端使用。

## 构建与运行

### 环境要求
- **Unity**: 6000.0.53f1+
- **.NET Framework**: 4.6.2
- **数据库**: SQL Server (LocalDB 或 完整实例)
- **IDE**: Visual Studio 2019+ (安装“.NET 桌面开发”工作负载)
- **Python**: 3.10+ (用于转表工具)

### 1. 数据库初始化（首次运行必做）
1. 确保 SQL Server 已启动。
2. 创建名为 `ExtremeWorld` 的数据库。
3. 执行初始化脚本：`Src/Server/GameServer/GameServer/Entities.edmx.sql` 以创建表结构。
4. 修改 `Src/Server/GameServer/GameServer/App.config` 中的连接字符串，指向您的本地 SQL 实例。

### 2. 生成策划数据
修改 Excel 后或首次运行前需同步数据：
```bash
cd Src\Data
python excel2json.py
```

### 3. 启动服务器
**构建：**
```bash
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug
```
**运行：**
直接在 VS 中运行或执行：
```bash
cd Src\Server\GameServer\GameServer\bin\Debug
GameServer.exe
```
*   默认监听地址：`127.0.0.1:8000`。
*   日志路径：`Src/Server/GameServer/GameServer/Log/`。

### 4. 启动客户端
1. 在 Unity Hub 中打开 `Src/Client` 项目。
2. 打开测试场景（如 `Assets/Levels/Test.unity`）。
3. 点击 Play 进入游戏。
4. 客户端日志路径：`Src/Client/Log/client.log`。

### 5. 协议更新
若修改了 `Src/Lib/proto/message.proto`：
1. 运行生成脚本：
    ```bash
    cd Tools
    genproto.cmd
    ```
2. 重新编译双端项目，DLL 会通过编译后事件自动复制到客户端目录。

## 开发规范

- **代码风格**：
    - **类型/公开成员**：使用 PascalCase（大驼峰，如 `UserService`）。
    - **局部变量/参数**：使用 camelCase（小驼峰，如 `userName`）。
    - **编码格式**：UTF-8 BOM，换行符使用 CRLF。
- **提交流程**：
    - 提交信息以动词开头，范围清晰（如：`修复登录重试逻辑`）。
    - 修改协议时，必须同时提交 `message.proto` 和生成的 `message.cs`。
- **目录结构说明**：
    - `Src/Client`：Unity 客户端源码与资源。
    - `Src/Server`：服务器解决方案。
    - `Src/Lib`：双端共享的公共代码与协议。
    - `Src/Data`：Excel 源表与转表工具。
    - `Tools`：Protobuf 编译器等辅助工具。
    - `.codex`：存储详细的项目文档与 AI 助手规则。
