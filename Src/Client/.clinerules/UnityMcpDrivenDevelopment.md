# Unity MCP驱动的游戏开发工作流（优化版）

## 概述
本文档定义了以Unity MCP为核心的自动化游戏开发工作流。所有与Unity编辑器相关的操作均通过Unity MCP进行，确保开发过程的一致性、可重复性和高效性。

---

## 阶段一：环境设置与MCP初始化

### 1.1 项目设置
- 确保Unity MCP服务器已启动并连接正常。
- 推荐通过`manage_editor`检查Unity编辑器状态，通过`read_console`验证MCP连接。

**常用MCP工具：**
- `manage_editor`：检查/控制编辑器状态
- `read_console`：获取/清理控制台日志

### 1.2 项目结构初始化
- 使用`manage_asset`批量创建标准目录结构：
  - Assets/Game/Scripts/（业务核心脚本）
  - Assets/Game/Scripts/UI（UI相关脚本）
  - Assets/Game/Scripts/Services（服务层脚本）
  - Assets/Game/Scripts/Network（网络通信脚本）
  - Assets/Game/Scripts/Log（日志相关脚本）
  - Assets/Prefabs/UI（UI预制体）
  - Assets/Prefabs/AIPrefabs（AI相关预制体）
  - Assets/Scenes（游戏场景）
  - Assets/Models/Characters、Monsters、NPCs、Objects（美术资源分层）
  - Assets/Resources/Character、UI、Prefabs（运行时加载资源）
  - Log/（客户端日志输出目录）

**实施步骤：**
1. 用`manage_asset`批量创建文件夹结构（支持一次性创建多级目录）。
2. 用`manage_asset`导入核心库和依赖项（如log4net.dll）。
3. 配置log4net日志系统（见下节）。
4. 每步操作后用`read_console`检查错误。

---

## 阶段二：日志系统配置

### 2.1 客户端日志配置
1. 创建`Assets/Resources/log4net.xml`，配置RollingFileAppender（按天生成日志）、绝对路径、日志格式与级别、UnityConsoleAppender。
2. 在LoadingManager.cs中初始化log4net，使用Application.dataPath构建配置路径，调用XmlConfigurator.ConfigureAndWatch加载配置，初始化UnityLogger。
3. 创建UnityConsoleAppender.cs（自定义Appender，日志转发到Unity控制台，防循环机制）。
4. 创建UnityLogger.cs（捕获logMessageReceived事件，精确定位，双重防循环，分级堆栈策略）。

**示例配置见原文。**

### 2.2 日志文件管理与策略
- 日志按天生成，自动保留30天，超期自动清理。
- 客户端日志：Log/client.log、Log/client.log.2025-08-01等
- 服务器日志：server-detailed.log同理
- 日志级别分Info/Warning（简洁）、Error/Fatal（含堆栈），全部精确定位调用者。

---

## 阶段三：MCP驱动的UI与脚本开发

### 3.1 MCP工具用法与典型场景

#### 3.1.1 manage_gameobject
- 创建/查找/修改/删除GameObject，批量操作支持传递数组或多次调用。
- 典型参数：
  - action: "create"/"modify"/"delete"/"find"
  - name/target/parent/primitive_type/components_to_add/component_properties
- 复杂属性赋值示例：
  ```json
  {
    "UILogin": {
      "username": {"find": "UsernameInput", "method": "by_name"},
      "password": {"find": "PasswordInput", "method": "by_name"},
      "buttonLogin": {"find": "LoginButton", "method": "by_name"}
    }
  }
  ```

#### 3.1.2 manage_asset
- 创建/导入/移动/重命名/删除资源，支持批量操作。
- 典型参数：
  - action: "create"/"import"/"move"/"delete"
  - path/asset_type/properties/destination

#### 3.1.3 manage_script
- 创建/读取/更新/删除C#脚本，支持指定namespace、script_type。
- 建议所有引用变量设为public，便于Inspector配置。

#### 3.1.4 manage_scene
- 创建/加载/保存场景，获取场景层级结构。

#### 3.1.5 manage_shader
- 创建/读取/更新/删除Shader脚本。

#### 3.1.6 read_console
- 获取/过滤/清理控制台日志，支持类型筛选、关键字过滤、堆栈输出。

#### 3.1.7 execute_menu_item
- 执行Unity菜单项（如构建、保存等）。

---

### 3.2 UI开发与批量自动化

#### 3.2.1 批量创建UI
- 用manage_gameobject批量创建Canvas、Panel、Button、InputField等。
- 支持一次性配置父子关系、组件属性。

#### 3.2.2 批量Prefab生成
- 用manage_asset批量保存GameObject为Prefab，支持指定路径和命名规范。

#### 3.2.3 批量脚本挂载与配置
- 用manage_gameobject批量挂载脚本，批量配置公共字段和事件绑定。

#### 3.2.4 资源批量导入与整理
- 用manage_asset批量导入美术资源，自动归类到指定目录。

---

## 阶段四：MCP驱动的游戏逻辑与AI实现

### 4.1 场景与对象自动化管理
- 用manage_scene创建/切换场景。
- 用manage_gameobject批量创建主摄像机、光源、UI、角色等。
- 用manage_asset保存场景对象为Prefab。

### 4.2 角色与AI系统
- 用manage_gameobject创建角色对象，批量添加Animator、Rigidbody、Collider等组件。
- 用manage_script批量生成角色/AI控制脚本，挂载到对象并配置参数。

---

## 阶段五：自动化测试与持续集成

### 5.1 UI与功能自动化测试
- 用manage_editor进入播放模式，manage_gameobject模拟用户输入（查找UI、输入、点击），read_console验证响应。
- 支持自动化测试脚本（如AutomatedTest），集成到开发流程。

### 5.2 自动化构建与产物校验
- 用execute_menu_item自动打开构建设置、执行构建。
- 用read_console监控构建日志，自动检测错误并输出报告。

---

## 阶段六：常见问题与排查

### 6.1 常见MCP错误及排查
- 连接异常：用manage_editor检查编辑器状态，read_console查日志。
- 资源导入失败：用read_console过滤"import"/"error"关键字，定位具体资源。
- 脚本编译错误：用read_console获取详细堆栈，结合manage_script定位脚本。

### 6.2 自动恢复与回滚
- 建议关键操作前用manage_asset/scene保存快照，出错可快速回滚。

---

## 阶段七：最佳实践

1. 所有Unity操作优先用MCP工具，避免手工重复劳动。
2. 推荐每次操作后立即用read_console检查结果，发现异常及时修正。
3. 复杂功能拆分为小模块，逐步集成，便于测试和维护。
4. 自动化测试脚本应覆盖核心流程，持续集成时自动执行。
5. 批量操作优先，能合并的操作尽量合并为一次MCP调用。
6. 充分利用component_properties实现复杂引用和属性赋值。
7. 资源、Prefab、脚本等命名规范统一，便于批量管理和查找。

---

## 阶段八：可选扩展与集成

### 8.1 自定义MCP扩展开发
- 参考现有API风格，扩展如动画控制、性能监控等新功能。
- 扩展后及时更新本规则文档和工具列表。

### 8.2 与外部工具集成
- 推荐与版本控制（如git）、CI/CD系统集成，自动触发MCP批量操作和测试。

---

## 附录：MCP工具参数与典型用法速查

### manage_gameobject
| 参数              | 说明                         | 示例值                  |
|-------------------|------------------------------|-------------------------|
| action            | 操作类型                     | "create"/"modify"       |
| name/target       | 对象名/目标                  | "LoginPanel"            |
| parent            | 父对象名                     | "Canvas"                |
| primitive_type    | 基础类型                     | "Button"/"InputField"   |
| components_to_add | 添加组件列表                 | ["UILogin"]             |
| component_properties | 组件属性配置              | 见上文复杂赋值示例      |

### manage_asset
| 参数         | 说明           | 示例值                        |
|--------------|----------------|-------------------------------|
| action       | 操作类型       | "create"/"import"/"move"      |
| path         | 路径           | "Assets/Prefabs/UI/"          |
| asset_type   | 资源类型       | "Prefab"/"Material"           |
| properties   | 资源属性       | {"color": [1,0,0,1]}          |
| destination  | 目标路径       | "Assets/Prefabs/AIPrefabs/"   |

### manage_script
| 参数         | 说明           | 示例值                        |
|--------------|----------------|-------------------------------|
| action       | 操作类型       | "create"/"update"             |
| name         | 脚本名         | "UILogin"                     |
| path         | 路径           | "Assets/Scripts/UI/"          |
| script_type  | 脚本类型       | "MonoBehaviour"               |
| namespace    | 命名空间       | "UI"                          |
| contents     | 脚本内容       | 见模板                        |

---

本工作流以Unity MCP为核心，强调自动化、批量化、标准化和可扩展性。所有操作建议结合实际工程结构和MCP工具能力灵活调整。
