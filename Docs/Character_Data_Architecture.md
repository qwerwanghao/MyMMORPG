# MMORPG 核心架构：Character 实体的全链路关系

本档将以 `Character`（角色）为例，解剖 MMORPG 中数据如何在 **硬盘、数据库、服务器内存、网络协议、客户端内存** 之间流动。这是理解大型联网游戏开发的基础。

## 1. 核心思想：静态与动态的分离

在游戏开发中，我们将数据分为两类：
- **静态配置 (Template/Static Data)**：所有玩家共享的策划数据（如：战士的初始生命值、怪物的模型路径）。存在 **硬盘**。
- **动态实例 (Instance/Dynamic Data)**：每个玩家独有的存档数据（如：小明现在的等级、当前位置）。存在 **数据库**。

---

## 2. 数据在五大维度的表现形式

### (1) 硬盘中的静态数据 (Configuration)
**位置**：`Src/Client/Data/GameConfig.txt`（包含 `CharacterDefine`）
**作用**：定义角色的“模版”。
- 比如：TID 为 1 的角色是“战士”，使用 `Male_Warrior` 预制体，基础移速是 5。
- **核心思想**：两端（Server & Client）都会在启动时读取这些文件，存入内存中的 `DataManager`。

### (2) 数据库中的持久数据 (Persistence - DB)
**位置**：数据库表 `TCharacter`
**技术：ORM (Entity Framework)**
- **什么是 ORM？**
  - 想象数据库是一个 Excel 表格，而代码里是 C# 对象。手动写 SQL 语句查表很麻烦。
  - **ORM (Object-Relational Mapping)** 是一座桥梁。它让你像操作 C# 列表一样操作数据库（例如：`db.TCharacters.Add(newChar)`）。
  - **项目中**：`TCharacter.cs` 就是由 EF 自动生成的数据库表映射类。

### (3) 服务器运行时的逻辑实体 (Server Memory - Logic)
**位置**：`Src/Server/GameServer/GameServer/Entities/Character.cs`
**作用**：服务器内存中活生生的“角色”。
- 它包含一个 `TCharacter Data`（来自数据库）和一个 `NCharacterInfo Info`（准备发给网络）。
- **管理者**：`CharacterManager.cs` 使用 `Dictionary` 在内存中维护所有当前在线的角色。

### (4) 网络流转的协议对象 (Network - Protocol)
**位置**：`Src/Lib/proto/message.proto` 中的 `NCharacterInfo`
**技术：Protobuf**
- **什么是 Protobuf？**
  - 在网络上传输 C# 对象非常占空间且效率低。
  - **Protobuf (Protocol Buffers)** 是 Google 开发的一种二进制序列化格式。它将对象压缩成极小的二进制流，速度极快。
  - **核心思想**：它是 Client 和 Server 沟通的“共有语言”。

### (5) 客户端运行时的表现实体 (Client Memory - View)
**位置**：`Src/Client/Assets/Game/Scripts/Services/UserService.cs` 以及各种 `Models`
**作用**：客户端解析并显示角色。
- 客户端收到 `NCharacterInfo` 后，会根据其中的 `TID` 去 **(1)** 提到的 `DataManager` 里查表，找到对应的 3D 模型渲染出来。

---

## 3. 角色流转全链路图示 (Character Data Flow)

```text
[ 硬盘 (Static Data) ]
      |
      | (Load on Start)
      V
[ 后台管理/DataManager ] <-------+
                                 | (Lookup Template by TID)
[ 数据库 (DB: TCharacter) ]      |
      |                          |
      | (EntityFramework ORM)    |
      V                          |
[ 物理机内存 (Server: Character) ]--+
      |
      | (Protobuf Serialization)
      V
[ 网络传输 (NCharacterInfo) ]
      |
      | (Protobuf Serialization)
      V
[ 玩家内存 (Client: Models.User) ]
      |
      | (Unity Engine Rendering)
      V
[ 玩家屏幕 (View) ]
```

---

## 4. 关键点总结

1. **唯一标识符**：
   - `ID`：数据库给的唯一身份证号（每个角色一个）。
   - `TID`：策划给的模版编号（所有战士都共用同一个 TID）。
2. **为什么不直接传数据库对象？**
   - 安全性：数据库对象包含敏感信息（如密码或内部逻辑状态）。
   - 性能：数据库对象太重，Protobuf 生成的协议对象（NInfo）仅包含客户端渲染必需的最简信息。
3. **数据同步的时机**：
   - 登录时：拉取整个列表。
   - 移动时：仅同步坐标（NEntity）。
   - 升级时：同步等级。

🤖 Generated with [Claude Code](https://claude.com/claude-code)
