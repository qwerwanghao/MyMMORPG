# CLAUDE_ZH.md

本文件为Claude Code (claude.ai/code) 在此代码库中工作时提供中文指导。

## 项目概述

这是一个基于Unity 6的MMORPG客户端项目，包含C#服务器组件。项目采用客户端-服务器架构，使用Protobuf进行网络通信。

## 核心架构组件

### 客户端结构 (Unity)
- **Assets/Game/**: 核心游戏逻辑和脚本
  - **Scripts/Core/**: 核心管理器 (DataManager, LoadingManager等)
  - **Scripts/Services/**: 游戏服务 (UserService用于身份验证)
  - **Scripts/Network/**: 网络层 (NetClient用于服务器通信)
  - **Scripts/UI/**: UI组件和控制器
  - **Scripts/Models/**: 数据模型
  - **Scripts/Utilities/**: 工具类和辅助程序 (单例模式)
  - **Scripts/Scene/**: 场景管理
  - **Scripts/Log/**: 自定义日志系统

- **Assets/ThirdParty/**: 外部依赖
  - **Common/**: 与服务器共享的库
  - **Protocol/**: 网络消息的Protobuf定义

### 服务器结构
- 位于 `../Server/GameServer/`
- C# .NET服务器实现
- 包含Entities、Managers和Models目录

### 数据配置
- **Data/**: 游戏配置文件 (JSON格式)
  - `GameConfig.txt`: 客户端配置
  - `GameServerConfig.txt`: 服务器连接设置
  - 各种 `*Define.txt` 文件: 游戏数据定义 (地图、角色、传送器、生成点)

## 网络架构

客户端使用自定义的基于TCP的网络层：
- **NetClient**: 处理连接管理、消息队列和可靠传输
- **MessageDistributer**: 将传入消息路由到相应的处理器
- **Protobuf**: 序列化网络消息以实现高效通信

常见开发工作流程：
1. 客户端通过NetClient连接到服务器
2. 服务层 (如UserService) 处理高级操作
3. 网络消息在Protocol程序集中定义
4. 数据从Data目录的JSON文件加载

## 开发命令

### Unity编辑器
- 在Unity 6中打开项目
- 主场景可能在 `Assets/Levels/` 中
- 构建设置配置为Windows平台

### 测试
- Unity Test Runner可用 (编辑模式和播放模式测试)
- 可通过Unity Test Runner窗口运行测试

### 协议更新
修改网络消息时：
1. 更新Protocol程序集中的协议定义
2. 重新生成Protobuf类
3. 更新客户端和服务器程序集

## 重要模式

### 单例模式
项目广泛使用单例模式管理器：
- `DataManager.Instance`: 游戏数据管理
- `UserService.Instance`: 用户身份验证服务
- `NetClient.Instance`: 网络客户端

### 事件驱动架构
- 服务使用Unity事件进行回调
- 消息分发器处理传入的网络消息
- UI组件订阅服务事件

### 配置加载
- 基于JSON的配置文件
- DataManager处理加载和解析
- 为缺失的配置提供默认值

## 常见文件位置

- 服务器连接: `Assets/Game/Scripts/Services/UserService.cs`
- 网络客户端: `Assets/Game/Scripts/Network/NetClient.cs`
- 数据管理器: `Assets/Game/Scripts/Core/DataManager.cs`
- UI登录: `Assets/Game/Scripts/UI/UILogin.cs`
- 角色选择: `Assets/Game/Scripts/UI/UICharacterSelect.cs`
- 游戏配置: `Data/GameConfig.txt`
- 服务器配置: `Data/GameServerConfig.txt`

## 依赖项

- Unity 6
- Newtonsoft.Json用于JSON序列化
- 与服务器共享的自定义Common和Protocol程序集
- Unity服务 (Analytics, Core)

## 开发注意事项

- 项目使用中文注释和日志
- 默认服务器连接为 127.0.0.1:8000
- 客户端包含带有冷却期的重连逻辑
- 消息队列系统处理网络中断
- 加载管理器处理异步数据加载操作

## 常用开发任务

### 添加新的网络消息
1. 在Protocol程序集中定义消息
2. 在服务器和客户端实现请求/响应处理
3. 在相应的Service中添加发送和接收方法

### 修改游戏配置
1. 编辑Data目录中的相应JSON文件
2. DataManager会在启动时自动加载
3. 使用Unity编辑器模式下的保存方法 (如果有)

### UI开发
1. UI脚本位于Assets/Game/Scripts/UI/
2. 遵循现有的事件订阅模式
3. 使用Unity UI系统和新版UI Toolkit