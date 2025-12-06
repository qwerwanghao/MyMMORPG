# Unity MCP驱动的游戏开发工作流

## 概述
本文档定义了一个以Unity MCP为核心的、自动化的游戏开发工作流。所有与Unity编辑器相关的操作都通过Unity MCP进行，确保开发过程的一致性和可重复性。

## 阶段一：环境设置与MCP初始化

### 1.1 项目设置
```bash
# 确保Unity MCP服务器正在运行
# 验证Cline与Unity MCP的连接
```

**使用的MCP工具：**
- `manage_editor` - 检查Unity编辑器状态
- `read_console` - 验证MCP连接状态

### 1.2 项目结构初始化
```markdown
使用manage_asset创建标准项目目录结构：
- Assets/Game/Scripts/UI - UI相关脚本
- Assets/Game/Scripts/Services - 服务层脚本
- Assets/Game/Scripts/Network - 网络通信脚本
- Assets/Game/Scripts/Log - 日志相关脚本
- Assets/Prefabs/UI - UI预制体
- Assets/Scenes - 游戏场景
- Assets/Materials - 材质资源
- Assets/Resources - 资源文件（包括log4net.xml配置）
- Log/ - 客户端日志输出目录
```

**实施步骤：**
1. 使用`manage_asset`创建文件夹结构
2. 使用`manage_asset`导入核心库和依赖项（包括log4net.dll）
3. 配置log4net日志系统
4. 使用`read_console`检查导入过程中的错误

### 1.3 log4net日志系统配置

#### 1.3.1 客户端日志配置
```markdown
1. 创建Assets/Resources/log4net.xml配置文件：
   - 配置RollingFileAppender，按天生成日志文件
   - 设置日志输出路径为相对路径
   - 配置日志格式和级别
   - 启用UnityConsoleAppender实现Unity控制台显示

2. 在 `Assets/Game/Scripts/Core/LoadingManager.cs` 中初始化log4net：
   - 使用 `Application.dataPath` 构建配置文件路径: `Path.Combine(Application.dataPath, "Resources/log4net.xml")`
   - 调用 `XmlConfigurator.ConfigureAndWatch` 加载配置

3. 创建UnityConsoleAppender.cs：
   - 自定义log4net Appender，将日志输出到Unity控制台
   - 根据日志级别选择合适的Unity输出方法
   - 实现防无限循环的标志位机制

4. 创建UnityLogger.cs：
   - 捕获Unity的logMessageReceived事件
   - 解析调用者信息，提供精确的文件名和行号
   - 实现双重防循环机制（内置+外部检测）
   - 支持分级堆栈信息显示策略
```

**客户端log4net配置示例：**
```xml
<log4net>
  <root>
    <level value="DEBUG" />
    <appender-ref ref="LogFileAppender" />
    <appender-ref ref="ColoredConsoleAppender" />
    <appender-ref ref="UnityConsoleAppender" />
  </root>
  <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender">
    <param name="File" value="Logs/client.log" />
    <param name="AppendToFile" value="true" />
    <rollingStyle value="Date" />
    <datePattern value="yyyy-MM-dd'.log'" />
    <staticLogFileName value="true" />
    <maxSizeRollBackups value="30" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level [%C.%M:%L] - %message%newline" />
    </layout>
  </appender>
  <appender name="UnityConsoleAppender" type="UnityConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="[%C.%M:%L] - %message" />
    </layout>
  </appender>
</log4net>
```

#### 1.3.2 服务器端日志配置
```markdown
1. 配置服务器log4net.xml：
   - 使用相同的按天生成策略
   - 设置服务器日志输出路径
   - 配置双重日志输出（主日志+详细日志）

2. 在 `Program.cs` 中初始化log4net：
   - 加载 `log4net.xml` 配置文件: `new System.IO.FileInfo("log4net.xml")`
   - 调用 `XmlConfigurator.ConfigureAndWatch`
   - 初始化 `Log` 系统: `Log.Init("GameServer")`
```

**服务器端log4net配置示例：**
```xml
<log4net>
  <root>
    <level value="DEBUG" />
    <appender-ref ref="RollingLogFileAppender" />
    <appender-ref ref="ColoredConsoleAppender" />
  </root>
  <appender name="RollingLogFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Log/server-detailed.log" />
    <appendToFile value="true" />
    <rollingStyle value="Date" />
    <datePattern value="yyyy-MM-dd'.log'" />
    <staticLogFileName value="true" />
    <param name="MaxSizeRollBackups" value="30" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%newline%date [%thread] [%-5level] [%C.%M:%L] - %message" />
    </layout>
  </appender>
</log4net>
```

#### 1.3.3 分级日志策略
```markdown
系统实现了智能的分级日志策略，平衡可读性和诊断能力：

**简洁级别（Info/Warning）**：
- 不包含堆栈信息，保持日志简洁易读
- 适用于常规业务流程记录和轻微警告

**详细级别（Error/Fatal）**：
- 包含完整堆栈信息，便于问题诊断
- 适用于错误处理和严重异常情况

**精确定位**：
- 所有日志都显示准确的调用者信息：[类名.方法名:行号]
- 自动过滤日志框架内部调用，只显示业务代码位置
```

#### 1.3.4 日志使用方法
```csharp
// 客户端日志使用（由 LoadingManager 初始化）
Log.Info("用户登录请求");
Log.Warning("连接超时警告");
Log.Error("网络连接失败");

// 服务器端日志使用（由 Program.cs 初始化）
Log.Info("服务器启动完成");
Log.Debug("调试信息");
Log.Error("数据库连接失败");
```

#### 1.3.5 日志文件管理
```markdown
**按天生成策略**：
- 客户端当天日志: `Logs/client.log` (相对于项目根目录)
- 服务器当天日志: `Log/server-detailed.log` (相对于服务器可执行文件目录)
- 历史日志会自动按日期备份，例如 `client.log.2025-08-01`

**自动清理**：
- 自动保留最近30天的日志文件
- 超过30天的旧日志自动删除，防止磁盘空间无限增长

**存储路径**：
- 客户端: `项目根目录/Logs/`
- 服务器: `可执行文件目录/Log/`

**优势**：
- 文件数量可控，每天最多新增几个文件
- 查找特定日期的日志非常方便
- 总磁盘占用可预测和控制
```

#### 1.3.6 Unity控制台集成
```markdown
**UnityConsoleAppender功能**：
- 将log4net日志自动转发到Unity Editor控制台
- 根据日志级别选择合适的Unity输出方法：
  * Error → Debug.LogError()（红色显示）
  * Warning → Debug.LogWarning()（黄色显示）
  * Info → Debug.Log()（白色显示）

**防无限循环机制**：
- 双重防护：内置标志位 + 外部类检测
- 确保在任何编译状态下都不会产生循环
- 异常安全：即使处理出错也不影响后续日志

**开发体验**：
- 实时在Unity控制台查看所有日志
- 保持与Unity原生日志的一致体验
- 支持控制台的过滤和搜索功能
```

## 阶段二：MCP驱动的UI与脚本开发

### 2.1 UI开发流程

#### 2.1.1 加载登录场景与UI
```markdown
1. 使用`manage_scene`加载包含UI Canvas的场景（如`Loading.unity`）。
2. 使用`manage_asset`在场景中实例化`UILogin`预制体。
```

**示例：实例化登录界面**
```csharp
// 1. 加载主场景
action: "load"
name: "Loading"
path: "Assets/Levels/"

// 2. 实例化UILogin预制体
// 假设已有一个名为 "UI/Prefabs/UILogin.prefab" 的预制体
action: "instantiate" 
asset_type: "Prefab"
path: "Assets/UI/Prefabs/UILogin.prefab"
parent: "Canvas" // 指定Canvas为父对象
```

### 2.2 脚本开发流程

#### 2.2.1 创建脚本
```markdown
// 更新现有脚本以匹配UI交互
action: "update"
name: "UILogin"
path: "Assets/Game/Scripts/UI/UILogin.cs"
contents: |
  // 确保脚本包含了对 username, password, buttonLogin 等UI元素的引用
  // 以及一个 OnClickLogin 方法来调用 UserService.Instance.SendLogin(...)
```

#### 2.2.2 脚本内容模板
```csharp
// (示例内容已在 manage_script 的 contents 参数中提供)
// UILogin.cs 的核心逻辑是：
// 1. 获取对InputField和Button的引用。
// 2. 在Start()方法中，通过 UserService.Instance.OnLogin 订阅登录结果的回调。
// 3. 实现 OnClickLogin() 方法，该方法从InputField获取用户输入，
//    然后调用 UserService.Instance.SendLogin(user, pass) 将登录请求发送到服务器。
```

#### 2.2.3 脚本挂载和配置
```markdown
1. 使用manage_gameobject将脚本挂载到GameObject：
   action: "modify"
   target: "UILogin(Clone)" // 实例化后的预制体名称
   components_to_add: ["UILogin"] // 脚本名

2. 使用manage_gameobject配置脚本的公共字段：
   action: "modify"
   target: "UILogin(Clone)"
   component_properties: {
     "UILogin": {
       "username": {"find": "UsernameInput", "method": "by_path", "path": "UILogin(Clone)/UsernameInput"},
       "password": {"find": "PasswordInput", "method": "by_path", "path": "UILogin(Clone)/PasswordInput"},
       "buttonLogin": {"find": "LoginButton", "method": "by_path", "path": "UILogin(Clone)/LoginButton"}
     }
   }
```

### 2.3 事件绑定
```markdown
使用manage_gameobject为按钮添加事件监听器：
action: "modify"
target: "UILogin(Clone)/LoginButton"
component_properties: {
  "Button": {
    "onClick": {
      "target": {"find": "UILogin(Clone)", "component": "UILogin"},
      "method": "OnClickLogin"
    }
  }
}
```

## 阶段三：MCP驱动的游戏逻辑实现

### 3.1 场景管理

#### 3.1.1 创建场景
```markdown
使用manage_scene创建新场景：
action: "create"
name: "LoginScene"
path: "Assets/Scenes/"
```

#### 3.1.2 场景对象管理
```markdown
使用manage_gameobject在场景中创建和管理对象：
1. 创建主摄像机和光源
2. 创建UI Canvas
3. 实例化UI Prefab
4. 配置场景特定的设置
```

### 3.2 角色与AI系统

#### 3.2.1 角色Prefab创建
```markdown
1. 使用manage_gameobject创建角色GameObject
2. 添加必要组件（Animator, Rigidbody, Collider等）
3. 使用manage_script创建角色控制脚本
4. 使用manage_asset保存为Prefab
```

#### 3.2.2 AI行为脚本
```csharp
// 使用manage_script创建AI脚本模板
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("AI Settings")]
    public float moveSpeed = 5f;
    public float detectionRange = 10f;
    
    private Transform target;
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        // AI逻辑实现
        DetectPlayer();
        MoveToTarget();
    }
    
    void DetectPlayer()
    {
        // 检测玩家逻辑
    }
    
    void MoveToTarget()
    {
        // 移动到目标逻辑
    }
}
```

## 阶段四：MCP驱动的自动化测试与构建

### 4.1 自动化测试流程

#### 4.1.1 播放模式测试
```markdown
1. 使用manage_editor进入播放模式：
   action: "play"

2. 使用read_console读取控制台日志：
   action: "get"
   types: ["error", "warning"]
   count: 50

3. 使用manage_gameobject模拟用户输入：
   - 查找UI元素
   - 模拟点击和输入操作
   - 验证响应结果

4. 使用manage_editor退出播放模式：
   action: "pause"
```

#### 4.1.2 功能测试脚本
```csharp
// 自动化测试脚本示例
public class AutomatedTest : MonoBehaviour
{
    public void TestLoginFlow()
    {
        // 1. 查找登录界面元素
        var usernameField = GameObject.Find("UsernameInput").GetComponent<InputField>();
        var passwordField = GameObject.Find("PasswordInput").GetComponent<InputField>();
        var loginButton = GameObject.Find("LoginButton").GetComponent<Button>();
        
        // 2. 模拟用户输入
        usernameField.text = "testuser";
        passwordField.text = "testpass";
        
        // 3. 模拟按钮点击
        loginButton.onClick.Invoke();
        
        // 4. 验证结果
        StartCoroutine(VerifyLoginResult());
    }
    
    IEnumerator VerifyLoginResult()
    {
        yield return new WaitForSeconds(2f);
        // 验证登录结果的逻辑
    }
}
```

### 4.2 构建流程

#### 4.2.1 自动化构建
```markdown
1. 使用execute_menu_item打开构建设置：
   menu_path: "File/Build Settings..."

2. 配置构建参数（平台、场景列表等）

3. 使用execute_menu_item开始构建：
   menu_path: "File/Build And Run"

4. 使用read_console监控构建过程：
   action: "get"
   types: ["error", "warning"]
   filter_text: "build"
```

## 阶段五：MCP功能扩展

### 5.1 需求分析

当现有Unity MCP功能无法满足开发需求时，需要进行功能扩展：

#### 5.1.1 常见扩展需求
- 高级UI操作（如拖拽、多选）
- 复杂动画控制
- 自定义编辑器工具
- 性能分析和优化工具
- 版本控制集成

### 5.2 功能设计

#### 5.2.1 新功能API设计示例
```python
# 示例：添加动画控制功能
def manage_animation(action, target, animation_name=None, parameters=None):
    """
    管理GameObject的动画
    
    Args:
        action: 操作类型 ('play', 'stop', 'pause', 'set_parameter')
        target: 目标GameObject名称或路径
        animation_name: 动画名称
        parameters: 动画参数字典
    
    Returns:
        操作结果字典
    """
    pass
```

#### 5.2.2 实现步骤
```markdown
1. 在Unity MCP服务器中实现新功能
2. 更新MCP工具列表和文档
3. 在Cline中测试新功能
4. 更新开发工作流文档
```

### 5.3 扩展示例：性能监控工具

#### 5.3.1 功能定义
```python
def monitor_performance(action, duration=10, metrics=None):
    """
    监控Unity性能指标
    
    Args:
        action: 'start', 'stop', 'get_report'
        duration: 监控持续时间（秒）
        metrics: 要监控的指标列表 ['fps', 'memory', 'draw_calls']
    
    Returns:
        性能报告数据
    """
    pass
```

#### 5.3.2 使用示例
```markdown
1. 开始性能监控：
   action: "start"
   duration: 30
   metrics: ["fps", "memory", "draw_calls"]

2. 获取性能报告：
   action: "get_report"

3. 停止监控：
   action: "stop"
```

## 最佳实践

### 6.1 开发流程最佳实践

1. **始终使用MCP工具**：所有Unity操作都通过MCP进行，确保可重复性
2. **分步骤验证**：每个操作后使用`read_console`检查错误
3. **模块化开发**：将复杂功能拆分为小的、可测试的模块
4. **自动化测试**：为每个功能编写自动化测试脚本

### 6.2 错误处理

1. **日志监控**：定期使用`read_console`检查错误和警告
2. **回滚机制**：保存关键状态，出错时能够快速回滚
3. **渐进式开发**：小步快跑，每次只修改一个小功能

### 6.3 性能优化

1. **批量操作**：尽可能将多个相关操作合并为一次MCP调用
2. **缓存结果**：避免重复查询相同的GameObject或组件
3. **异步处理**：对于耗时操作，使用异步方式处理

## 总结

本工作流以Unity MCP为核心，提供了一个完整的、自动化的游戏开发流程。通过标准化的MCP操作，确保了开发过程的一致性、可重复性和可维护性。当现有功能不足时，可以通过扩展MCP功能来满足新的需求。

这种方法的优势：
- **一致性**：所有操作都通过标准化的MCP接口
- **可重复性**：开发流程可以完全自动化和重现
- **可扩展性**：可以根据需要添加新的MCP功能
- **可维护性**：清晰的流程和标准化的操作方式
