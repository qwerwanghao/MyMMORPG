#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// 忽略证书错误
/// </summary>
internal class CustomCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true; // 允许所有证书
    }
}

/// <summary>
/// AI图片直接生成Prefab工具
/// 跳过JSON文件保存步骤，直接从图片生成prefab
/// </summary>
public class AIDirectPrefabGenerator : MonoBehaviour
{
    #region 序列化字段 - API配置

    [Header("=== API配置 ===")]
    [SerializeField] private string apiKey = "";
    [SerializeField] private string apiBase = "https://openrouter.ai/";
    [SerializeField] private string modelName = "google/gemini-2.5-pro";
    [SerializeField] private int maxCompletionTokens = 12000;
    [SerializeField] private bool ignoreCertificateErrors = false;

    #endregion

    #region 序列化字段 - 输入设置

    [Header("=== 输入设置 ===")]
    [SerializeField] private string imagePath;

    #endregion

    #region 序列化字段 - 运行状态

    [Header("=== 运行状态 ===")]
    [SerializeField] private bool isProcessing = false;
    [SerializeField] private string lastGeneratedInstructions;

    #endregion

    #region 常量与静态字段

    // AI图片分析提示 - 优化版，强调局部坐标
    private const string AI_PROMPT = @"你是Unity UI开发专家。请分析这个游戏界面截图，并根据UI元素的层级关系和相对位置，生成创建指令。

**核心原则：**
- **层级结构**：使用缩进（2个空格）来表示父子关系。
- **坐标系统**：
  - **全局坐标**：顶层元素（Canvas的直接子节点）使用相对于屏幕中心(0,0)的全局坐标。
  - **局部坐标**：所有子节点（如Panel下的元素）必须使用相对于其直接父节点中心(0,0)的**局部坐标**。

**分析步骤：**
1. **识别根元素**：将Canvas下的主要功能块（如登录面板、角色信息面板）作为顶层Panel，并为其指定**全局坐标**。
2. **分析子元素**：对于每个Panel内的子元素（按钮、文本等），估算它们相对于其**父Panel中心**的位置，并指定**局部坐标**。
3. **保持相对位置**：确保同一Panel内的元素相对位置与截图中一致。

**输出格式示例：**
```
CANVAS|0,0|1920,1080|UIRoot
  PANEL|-450,0|450,700|LoginPanel|#颜色  // LoginPanel在屏幕左侧，使用全局坐标
    TEXT|0,260|380,280|LogoAndCharacter|32|#6a4a35 // Logo相对于LoginPanel顶部，使用局部坐标
    BUTTON|0,-280|140,70|登录|36|#FFFFFF // 登录按钮相对于LoginPanel底部，使用局部坐标
```

**组件类型：**
- CANVAS：根Canvas，固定为 `CANVAS|0,0|1920,1080|UIRoot`
- PANEL：容器面板，可作为其他元素的父节点。
- TEXT：文字。
- BUTTON：按钮。
- IMAGE：图标或装饰。

**重要提醒：**
- **子节点的坐标必须是相对于父节点的局部坐标！** 这是确保布局正确的关键。
- 准确重现截图中看到的所有文字内容。
- 根据视觉位置关系来设定坐标，不要使用固定模板。

请严格按照以上规则，分析截图并输出UI创建指令：";

    private const string PrefabRootFolder = "Assets/Prefabs";
    private const string PrefabFolder = "Assets/Prefabs/AIPrefabs";
    private static Font builtinFont;

    #endregion

    #region 生命周期

    /// <summary>
    /// Unity Editor脚本启动方法
    /// </summary>
    void Start()
    {
        // Unity Editor组件初始化
        Log.InfoFormat("🚀 AI直接Prefab生成器已加载");
    }

    #endregion

    #region Unity菜单项

    /// <summary>
    /// Unity菜单栏入口 - 图片直接生成Prefab
    /// </summary>
    [MenuItem("AI工具/图片直接生成Prefab")]
    public static void StartDirectGeneration()
    {
        AIDirectPrefabGenerator instance = FindFirstObjectByType<AIDirectPrefabGenerator>();
        if (instance == null)
        {
            GameObject go = new GameObject("AIDirectPrefabGenerator");
            instance = go.AddComponent<AIDirectPrefabGenerator>();
        }

        instance.SelectImageAndStart();
    }

    [MenuItem("AI工具/停止协程")]
    public static void StopAllCoroutine()
    {
        AIDirectPrefabGenerator instance = FindFirstObjectByType<AIDirectPrefabGenerator>();
        if (instance == null)
        {
            EditorUtility.DisplayDialog("错误", "请先在场景中添加AIDirectPrefabGenerator组件", "确定");
            return;
        }

        instance.StopAllCoroutines();
        instance.isProcessing = false;
    }

    [MenuItem("AI工具/重新生成当前图片")]
    public static void RegenerateCurrentImage()
    {
        AIDirectPrefabGenerator instance = FindFirstObjectByType<AIDirectPrefabGenerator>();
        if (instance != null)
        {
            if (instance.isProcessing)
            {
                EditorUtility.DisplayDialog("提示", "正在处理中，请稍等或先停止当前任务", "确定");
                return;
            }

            if (string.IsNullOrEmpty(instance.imagePath) || !File.Exists(instance.imagePath))
            {
                EditorUtility.DisplayDialog("错误", "没有可重新生成的图片，请先选择图片", "确定");
                return;
            }

            Log.InfoFormat("🔄 开始重新生成当前图片...");
            instance.StartCoroutine(instance.ProcessImageToPrefabDirect());
        }
        else
        {
            EditorUtility.DisplayDialog("错误", "请先在场景中添加AIDirectPrefabGenerator组件", "确定");
        }
    }

    /// <summary>
    /// 快速切换Scene视图到2D模式
    /// </summary>
    [MenuItem("AI工具/切换Scene视图到2D模式")]
    public static void SwitchSceneViewTo2D()
    {
        try
        {
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.in2DMode = true;
                sceneView.pivot = Vector3.zero;
                // sceneView.rotation = Quaternion.identity; // 在2D模式下，Unity会自动处理，无需手动设置
                sceneView.size = 10f;
                sceneView.Repaint();

                Log.InfoFormat("✅ Scene视图已切换到2D模式");
                EditorUtility.DisplayDialog("切换完成", "Scene视图已切换到2D模式", "确定");
            }
            else
            {
                Log.WarningFormat("⚠️ 未找到活动的Scene视图");
            }
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 切换失败: {e.Message}");
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 选择图片并开始处理
    /// </summary>
    public void SelectImageAndStart()
    {
        string selectedPath = EditorUtility.OpenFilePanel("选择图片文件", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(selectedPath))
        {
            imagePath = selectedPath;
            StartCoroutine(ProcessImageToPrefabDirect());
        }
        else
        {
            Log.InfoFormat("❌ 用户取消了文件选择");
        }
    }

    #endregion

    #region 私有方法 - 主流程

    /// <summary>
    /// 直接图片转Prefab流程 - 完全绕过JSON格式
    /// </summary>
    private IEnumerator ProcessImageToPrefabDirect()
    {
        if (isProcessing)
        {
            Log.WarningFormat("⚠️ 正在处理中，请等待完成后再试");
            yield break;
        }

        isProcessing = true;
        Log.InfoFormat("🚀 开始AI图片直接生成Prefab流程...");

        // 步骤1: 分析图片生成UI指令
        Log.InfoFormat("🔍 步骤1: 分析图片生成UI创建指令...");
        yield return StartCoroutine(AnalyzeImageWithAPI());

        if (string.IsNullOrEmpty(lastGeneratedInstructions))
        {
            Log.ErrorFormat("❌ 图片分析失败");
            isProcessing = false;
            yield break;
        }

        // 步骤2: 直接从指令创建Prefab
        Log.InfoFormat("🎮 步骤2: 直接从指令创建Prefab...");
        GameObject generatedPrefab = CreatePrefabFromInstructions();

        if (generatedPrefab == null)
        {
            Log.ErrorFormat("❌ Prefab创建失败");
            EditorUtility.DisplayDialog("失败", "Prefab创建失败，请检查控制台错误信息", "确定");
            isProcessing = false;
            yield break;
        }

        Log.InfoFormat("✅ 流程完成！");
        EditorUtility.DisplayDialog("完成", "AI图片直接生成Prefab完成！", "确定");

        isProcessing = false;
    }

    /// <summary>
    /// 调用API分析图片生成UI指令 - 简化版
    /// </summary>
    private IEnumerator AnalyzeImageWithAPI()
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Log.ErrorFormat("API Key为空，请先配置apiKey");
            EditorUtility.DisplayDialog("错误", "API Key为空，请先在场景对象的AIDirectPrefabGenerator组件中配置 apiKey", "确定");
            yield break;
        }

        if (!File.Exists(imagePath))
        {
            Log.ErrorFormat("❌ 图片文件不存在: " + imagePath);
            yield break;
        }

        byte[] imageBytes = File.ReadAllBytes(imagePath);
        string base64Image = Convert.ToBase64String(imageBytes);
        string imageExtension = Path.GetExtension(imagePath).ToLower();
        string mimeType = imageExtension == ".png" ? "image/png" : "image/jpeg";

        Log.InfoFormat("🔍 开始分析图片生成UI指令...");

        var requestData = new
        {
            model = modelName,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = AI_PROMPT },
                        new {
                            type = "image_url",
                            image_url = new { url = $"data:{mimeType};base64,{base64Image}" }
                        }
                    }
                }
            },
            max_completion_tokens = maxCompletionTokens
        };

        yield return StartCoroutine(SendAPIRequest(requestData, (response) =>
        {
            string extractedInstructions = ExtractInstructionsFromResponse(response);
            if (!string.IsNullOrEmpty(extractedInstructions))
            {
                lastGeneratedInstructions = extractedInstructions;
                Log.InfoFormat("✅ UI指令生成完成！");
                Log.InfoFormat($"📝 生成的指令:\n{extractedInstructions}");
            }
            else
            {
                Log.ErrorFormat("❌ AI返回的指令为空或格式不正确");
                Log.InfoFormat($"🔍 AI原始响应:\n{response}");
            }
        }));
    }

    /// <summary>
    /// 统一的API请求方法
    /// </summary>
    private IEnumerator SendAPIRequest(object requestData, System.Action<string> onSuccess)
    {
        string requestJson = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);

        using (UnityWebRequest request = new UnityWebRequest(GetCompletionsUrl(), "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            if (ignoreCertificateErrors)
                request.certificateHandler = new CustomCertificateHandler();

            yield return request.SendWebRequest();

            // 检查错误
            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Log.ErrorFormat($"❌ API请求失败: {request.error}");
                Log.ErrorFormat($"❌ HTTP状态码: {request.responseCode}");
                Log.ErrorFormat($"❌ 响应内容: {request.downloadHandler.text}");
                yield break;
            }

            try
            {
                JObject responseJson = JObject.Parse(request.downloadHandler.text);
                string aiResponse = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(aiResponse))
                {
                    onSuccess?.Invoke(aiResponse);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat($"❌ 响应解析失败: {e.Message}");
                Log.ErrorFormat($"❌ 原始响应: {request.downloadHandler.text}");
            }
        }
    }

    /// <summary>
    /// 从AI响应中提取UI创建指令
    /// </summary>
    private string ExtractInstructionsFromResponse(string response)
    {
        try
        {
            // 尝试提取代码块中的指令
            var codeMatch = System.Text.RegularExpressions.Regex.Match(response,
                @"```\s*(.*?)\s*```",
                System.Text.RegularExpressions.RegexOptions.Singleline);

            if (codeMatch.Success)
            {
                string codeContent = codeMatch.Groups[1].Value.Trim();
                Log.InfoFormat($"✅ 从代码块中提取到指令");
                return codeContent;
            }

            // 如果没有代码块，尝试直接查找指令行
            var lines = response.Split('\n');
            var instructions = new System.Collections.Generic.List<string>();

            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();

                // 跳过注释行和空行
                if (string.IsNullOrEmpty(trimmedLine) ||
                    trimmedLine.StartsWith("//") ||
                    trimmedLine.StartsWith("/*") ||
                    trimmedLine.StartsWith("*"))
                {
                    continue;
                }

                if (trimmedLine.StartsWith("CANVAS") || trimmedLine.StartsWith("TEXT") ||
                    trimmedLine.StartsWith("BUTTON") || trimmedLine.StartsWith("IMAGE") ||
                    trimmedLine.StartsWith("PANEL"))
                {
                    instructions.Add(line.TrimEnd()); // 保留缩进
                }
            }

            if (instructions.Count > 0)
            {
                Log.InfoFormat($"✅ 从响应中提取到 {instructions.Count} 行指令");
                return string.Join("\n", instructions);
            }

            Log.WarningFormat("⚠️ 未找到有效的UI指令");
            return "";
        }
        catch (Exception e)
        {
            Log.ErrorFormat($"❌ 指令提取失败: {e.Message}");
            return "";
        }
    }

    /// <summary>
    /// 从指令创建Prefab
    /// </summary>
    private GameObject CreatePrefabFromInstructions()
    {
        if (string.IsNullOrEmpty(lastGeneratedInstructions))
        {
            Log.ErrorFormat("❌ 没有可用的UI指令");
            return null;
        }

        GameObject uiRoot = ParseInstructionsAndCreateUI(lastGeneratedInstructions);

        if (uiRoot == null)
        {
            Log.ErrorFormat("❌ UI解析创建失败");
            return null;
        }

        // 创建Prefab
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string prefabName = $"AI_Direct_Generated_{timestamp}";

        EnsurePrefabFolders();
        string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";

        // 保存为Prefab
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(uiRoot, prefabPath);

        if (prefabAsset != null)
        {
            Log.InfoFormat($"✅ Prefab创建成功: {prefabPath}");

            // 选中新创建的Prefab
            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);

            // 设置Scene视图为2D模式以便查看
            SetSceneViewTo2D();

            return prefabAsset;
        }
        else
        {
            Log.ErrorFormat("❌ Prefab保存失败");
            if (uiRoot != null)
            {
                DestroyImmediate(uiRoot);
            }
            return null;
        }
    }

    /// <summary>
    /// 解析指令并创建UI元素（支持层级结构）
    /// </summary>
    private GameObject ParseInstructionsAndCreateUI(string instructions)
    {
        if (string.IsNullOrEmpty(instructions))
        {
            Log.ErrorFormat("❌ 指令为空，无法创建UI");
            return null;
        }

        Log.InfoFormat($"📝 开始解析带层级的指令:\n{instructions}");

        try
        {
            string[] lines = instructions.Split('\n');
            GameObject rootObject = null;
            var parentStack = new System.Collections.Generic.Stack<GameObject>(); // 父对象栈
            int successCount = 0;
            int failCount = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || line.Trim().StartsWith("//"))
                    continue;

                // 计算缩进层级
                int indentLevel = GetIndentLevel(line);
                string trimmedLine = line.Trim();

                try
                {
                    string[] parts = trimmedLine.Split('|');
                    if (parts.Length < 2)
                    {
                        Log.WarningFormat($"⚠️ 指令格式错误，跳过: '{trimmedLine}' (参数不足，需要至少2个参数)");
                        failCount++;
                        continue;
                    }

                    string command = parts[0].Trim().ToUpper();

                    // 根据缩进层级调整父对象栈
                    while (parentStack.Count > indentLevel)
                    {
                        parentStack.Pop();
                    }

                    // 确定当前父对象
                    GameObject parentObject = parentStack.Count > 0 ? parentStack.Peek() : null;
                    GameObject createdObject = null;

                    switch (command)
                    {
                        case "CANVAS":
                            createdObject = CreateCanvas(parts);
                            if (createdObject != null)
                            {
                                rootObject = createdObject;
                                successCount++;
                            }
                            else failCount++;
                            break;
                        case "TEXT":
                            if (parentObject != null)
                            {
                                CreateText(parentObject, parts);
                                successCount++;
                            }
                            else
                            {
                                Log.WarningFormat("⚠️ 找不到父对象，跳过Text创建");
                                failCount++;
                            }
                            break;
                        case "BUTTON":
                            if (parentObject != null)
                            {
                                CreateButton(parentObject, parts);
                                successCount++;
                            }
                            else
                            {
                                Log.WarningFormat("⚠️ 找不到父对象，跳过Button创建");
                                failCount++;
                            }
                            break;
                        case "IMAGE":
                            if (parentObject != null)
                            {
                                CreateImage(parentObject, parts);
                                successCount++;
                            }
                            else
                            {
                                Log.WarningFormat("⚠️ 找不到父对象，跳过Image创建");
                                failCount++;
                            }
                            break;
                        case "PANEL":
                            createdObject = CreateContainerWithParent(parentObject, parts);
                            if (createdObject != null) successCount++;
                            else failCount++;
                            break;
                        default:
                            Log.WarningFormat($"⚠️ 未知指令类型: '{command}'，跳过。完整指令: '{trimmedLine}'");
                            failCount++;
                            break;
                    }

                    // 如果创建了容器对象，将其加入父对象栈
                    if (createdObject != null && (command == "CANVAS" || command == "PANEL"))
                    {
                        parentStack.Push(createdObject);
                    }
                }
                catch (System.Exception e)
                {
                    Log.ErrorFormat($"❌ 处理指令时出错: '{trimmedLine}' - {e.Message}");
                    failCount++;
                }
            }

            Log.InfoFormat($"📊 层级指令解析完成: 成功{successCount}个，失败{failCount}个");

            // 即使有部分失败，只要有Canvas创建成功就返回
            if (rootObject != null && successCount > 0)
            {
                Log.InfoFormat($"✅ 层级UI创建成功，根对象: {rootObject.name}");
                return rootObject;
            }
            else
            {
                Log.ErrorFormat($"❌ 层级UI创建失败，成功创建{successCount}个元素");
                return null;
            }
        }
        catch (Exception e)
        {
            Log.ErrorFormat($"❌ 层级指令解析发生严重错误: {e.Message}");
            return null;
        }
    }

    #endregion

    #region 私有方法 - UI组件创建

    /// <summary>
    /// 创建Canvas组件
    /// </summary>
    private GameObject CreateCanvas(string[] parts)
    {
        // 支持新格式：CANVAS|x,y|width,height|根Canvas
        string name = parts.Length > 3 ? parts[3] : "GeneratedCanvas";
        GameObject canvas = new GameObject(name);

        // 确保Canvas始终为2D UI模式
        Canvas canvasComponent = canvas.AddComponent<Canvas>();
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComponent.sortingOrder = 0;

        // 配置CanvasScaler，确保UI适配不同分辨率
        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f; // 平衡宽高适配

        // 添加事件处理
        canvas.AddComponent<GraphicRaycaster>();

        Log.InfoFormat($"✅ 创建2D UI Canvas: {name}");
        return canvas;
    }

    /// <summary>
    /// 创建Panel组件并指定父对象
    /// </summary>
    private GameObject CreateContainerWithParent(GameObject parent, string[] parts)
    {
        try
        {
            // PANEL|x,y|width,height|面板名称|颜色(可选)
            if (parts.Length < 3)
            {
                Log.WarningFormat($"⚠️ Panel指令参数不足: {string.Join("|", parts)}，跳过创建");
                return null;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string panelName = parts.Length > 3 ? parts[3] : "Panel";
            Color backgroundColor = parts.Length > 4 ? ParseColor(parts[4]) : new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject panelObj = new GameObject(panelName);

            // 如果有父对象，设置父子关系
            if (parent != null)
            {
                panelObj.transform.SetParent(parent.transform, false);
            }

            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            // 设置锚点为中心，使用中心坐标系
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            // 添加Image组件作为背景
            Image image = panelObj.AddComponent<Image>();
            image.color = backgroundColor;

            Log.InfoFormat($"✅ 创建Panel: {panelName} at {position} size {size} (父对象: {(parent != null ? parent.name : "无")})");
            return panelObj;
        }
        catch (System.Exception ex)
        {
            Log.ErrorFormat($"❌ Panel创建失败: {ex.Message}，指令: {string.Join("|", parts)}");
            return null;
        }
    }

    /// <summary>
    /// 创建Text组件
    /// </summary>
    private void CreateText(GameObject parent, string[] parts)
    {
        try
        {
            // TEXT|x,y|width,height|文本内容|字体大小|颜色
            if (parts.Length < 4)
            {
                Log.WarningFormat($"⚠️ Text指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string textContent = parts.Length > 3 ? parts[3] : "Text";

            // 安全解析字体大小
            int fontSize = 24; // 默认值
            if (parts.Length > 4 && int.TryParse(parts[4], out int parsedSize))
            {
                fontSize = Mathf.Clamp(parsedSize, 8, 100); // 限制字体大小范围
            }

            // 解析颜色
            Color textColor = parts.Length > 5 ? ParseColor(parts[5]) : Color.black;

            string textName = $"Text_{textContent.Replace(" ", "_").Replace("/", "_")}";
            GameObject textObj = new GameObject(textName);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            // 设置锚点为中心，使用中心坐标系
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = textContent;
            textComponent.font = GetBuiltinFont();
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = TextAnchor.MiddleCenter;

            Log.InfoFormat($"✅ 创建Text: '{textContent}' at {position} size {size} fontSize {fontSize} color {textColor}");
            Log.InfoFormat($"📍 Text坐标详情: anchoredPosition={rect.anchoredPosition}, sizeDelta={rect.sizeDelta}");
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 创建Text失败: {e.Message}，指令: {string.Join("|", parts)}");
        }
    }

    /// <summary>
    /// 创建Button组件
    /// </summary>
    private void CreateButton(GameObject parent, string[] parts)
    {
        try
        {
            // BUTTON|x,y|width,height|按钮文本|字体大小|颜色
            if (parts.Length < 4)
            {
                Log.WarningFormat($"⚠️ Button指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string buttonText = parts.Length > 3 ? parts[3] : "Button";

            // 安全解析字体大小
            int fontSize = 24; // 默认值
            if (parts.Length > 4 && int.TryParse(parts[4], out int parsedSize))
            {
                fontSize = Mathf.Clamp(parsedSize, 8, 100); // 限制字体大小范围
            }

            // 解析按钮颜色
            Color buttonColor = parts.Length > 5 ? ParseColor(parts[5]) : Color.white;

            string buttonName = $"Button_{buttonText.Replace(" ", "_").Replace("/", "_")}";
            GameObject buttonObj = new GameObject(buttonName);
            buttonObj.transform.SetParent(parent.transform, false);

            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            // 设置锚点为中心，使用中心坐标系
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = buttonObj.AddComponent<Image>();
            image.color = buttonColor;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            // 添加文本子对象
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = buttonText;
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;

            Log.InfoFormat($"✅ 创建Button: '{buttonText}' at {position} size {size} fontSize {fontSize} color {buttonColor}");
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 创建Button失败: {e.Message}，指令: {string.Join("|", parts)}");
        }
    }

    /// <summary>
    /// 创建Image组件
    /// </summary>
    private void CreateImage(GameObject parent, string[] parts)
    {
        try
        {
            // IMAGE|x,y|width,height|图片名称|颜色(可选)
            if (parts.Length < 3)
            {
                Log.WarningFormat($"⚠️ Image指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string imageName = parts.Length > 3 ? parts[3] : "Image";
            Color color = parts.Length > 4 ? ParseColor(parts[4]) : Color.white;

            GameObject imageObj = new GameObject(imageName);
            imageObj.transform.SetParent(parent.transform, false);

            RectTransform rect = imageObj.AddComponent<RectTransform>();
            // 设置锚点为中心，使用中心坐标系
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = imageObj.AddComponent<Image>();
            image.color = color;

            Log.InfoFormat($"✅ 创建Image: {imageName} at {position} size {size} color {color}");
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 创建Image失败: {e.Message}，指令: {string.Join("|", parts)}");
        }
    }

    /// <summary>
    /// 创建Toggle组件
    /// </summary>
    private void CreateToggle(GameObject parent, string[] parts)
    {
        try
        {
            if (parts.Length < 3)
            {
                Log.WarningFormat($"⚠️ Toggle指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string toggleName = parts.Length > 3 ? parts[3] : "Tgl_Toggle";
            string labelText = parts.Length > 4 ? parts[4] : "Toggle";
            bool isOn = parts.Length > 5 && bool.TryParse(parts[5], out bool result) ? result : false;

            GameObject toggleObj = new GameObject(toggleName);
            toggleObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = toggleObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            Toggle toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = isOn;

            // 创建Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(toggleObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchoredPosition = new Vector2(-size.x / 2 + 15, 0);
            bgRect.sizeDelta = new Vector2(20, 20);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.white;

            // 创建Checkmark
            GameObject checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(bgObj.transform, false);
            RectTransform checkRect = checkmarkObj.AddComponent<RectTransform>();
            checkRect.anchoredPosition = Vector2.zero;
            checkRect.sizeDelta = new Vector2(16, 16);
            Image checkImage = checkmarkObj.AddComponent<Image>();
            checkImage.color = Color.green;

            // 创建Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchoredPosition = new Vector2(15, 0);
            labelRect.sizeDelta = new Vector2(size.x - 30, size.y);
            Text labelTextComp = labelObj.AddComponent<Text>();
            labelTextComp.text = labelText;
            labelTextComp.font = GetBuiltinFont();
            labelTextComp.fontSize = 14;
            labelTextComp.color = Color.black;

            // 设置Toggle引用
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;

            Log.InfoFormat($"✅ 创建Toggle: {toggleName} at {position} size {size}");
        }
        catch (System.Exception ex)
        {
            Log.ErrorFormat($"❌ Toggle创建失败: {ex.Message}，指令: {string.Join("|", parts)}");
        }
    }

    /// <summary>
    /// 创建Slider组件
    /// </summary>
    private void CreateSlider(GameObject parent, string[] parts)
    {
        try
        {
            if (parts.Length < 3)
            {
                Log.WarningFormat($"⚠️ Slider指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string sliderName = parts.Length > 3 ? parts[3] : "Sld_Slider";
            float value = parts.Length > 4 && float.TryParse(parts[4], out float v) ? v : 0.5f;
            float minValue = parts.Length > 5 && float.TryParse(parts[5], out float min) ? min : 0f;
            float maxValue = parts.Length > 6 && float.TryParse(parts[6], out float max) ? max : 1f;

            GameObject sliderObj = new GameObject(sliderName);
            sliderObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = sliderObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.value = value;
            slider.minValue = minValue;
            slider.maxValue = maxValue;

            // 创建Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = size;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = Color.gray;

            // 创建Fill Area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = size;

            // 创建Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = size;
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.blue;

            // 创建Handle Slide Area
            GameObject handleAreaObj = new GameObject("Handle Slide Area");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchoredPosition = Vector2.zero;
            handleAreaRect.sizeDelta = size;

            // 创建Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = new Vector2(20, 20);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;

            // 设置Slider引用
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            Log.InfoFormat($"✅ 创建Slider: {sliderName} at {position} size {size}");
        }
        catch (System.Exception ex)
        {
            Log.ErrorFormat($"❌ Slider创建失败: {ex.Message}，指令: {string.Join("|", parts)}");
        }
    }

    /// <summary>
    /// 创建ScrollView组件
    /// </summary>
    private void CreateScrollView(GameObject parent, string[] parts)
    {
        try
        {
            if (parts.Length < 3)
            {
                Log.WarningFormat($"⚠️ ScrollView指令参数不足: {string.Join("|", parts)}，跳过创建");
                return;
            }

            Vector2 position = ParsePosition(parts[1]);
            Vector2 size = ParseSize(parts[2]);
            string scrollViewName = parts.Length > 3 ? parts[3] : "Scr_ScrollView";

            GameObject scrollViewObj = new GameObject(scrollViewName);
            scrollViewObj.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = scrollViewObj.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
            rectTransform.sizeDelta = size;

            // 添加Image作为背景
            Image bgImage = scrollViewObj.AddComponent<Image>();
            bgImage.color = new Color(1f, 1f, 1f, 0.1f); // 半透明白色

            // 添加ScrollRect组件
            ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();

            // 创建Viewport
            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            Image viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewportObj.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // 创建Content
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, size.y);
            contentRect.anchoredPosition = Vector2.zero;

            // 设置ScrollRect引用
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            Log.InfoFormat($"✅ 创建ScrollView: {scrollViewName} at {position} size {size}");
        }
        catch (System.Exception ex)
        {
            Log.ErrorFormat($"❌ ScrollView创建失败: {ex.Message}，指令: {string.Join("|", parts)}");
        }
    }

    #endregion

    #region 私有方法 - 辅助解析

    /// <summary>
    /// 计算行的缩进层级
    /// </summary>
    private int GetIndentLevel(string line)
    {
        int spaces = 0;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == ' ')
                spaces++;
            else if (line[i] == '\t')
                spaces += 4; // Tab算作4个空格
            else
                break;
        }
        return spaces / 2; // 每2个空格算一级缩进
    }

    /// <summary>
    /// 解析位置字符串
    /// </summary>
    private Vector2 ParsePosition(string posStr)
    {
        try
        {
            if (string.IsNullOrEmpty(posStr))
            {
                Log.WarningFormat("⚠️ 位置字符串为空，使用默认位置(0,0)");
                return Vector2.zero;
            }

            string[] coords = posStr.Split(',');
            if (coords.Length < 2)
            {
                Log.WarningFormat($"⚠️ 位置格式错误: '{posStr}'，应为'x,y'格式，使用默认位置(0,0)");
                return Vector2.zero;
            }

            // 清理空格和其他字符
            string xStr = coords[0].Trim();
            string yStr = coords[1].Trim();

            if (!float.TryParse(xStr, out float x))
            {
                Log.WarningFormat($"⚠️ X坐标解析失败: '{xStr}'，使用0");
                x = 0f;
            }

            if (!float.TryParse(yStr, out float y))
            {
                Log.WarningFormat($"⚠️ Y坐标解析失败: '{yStr}'，使用0");
                y = 0f;
            }

            return new Vector2(x, y);
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 位置解析异常: '{posStr}' - {e.Message}，使用默认位置(0,0)");
            return Vector2.zero;
        }
    }

    /// <summary>
    /// 解析尺寸字符串
    /// </summary>
    private Vector2 ParseSize(string sizeStr)
    {
        try
        {
            if (string.IsNullOrEmpty(sizeStr))
            {
                Log.WarningFormat("⚠️ 尺寸字符串为空，使用默认尺寸(100,50)");
                return new Vector2(100, 50);
            }

            string[] dims = sizeStr.Split(',');
            if (dims.Length < 2)
            {
                Log.WarningFormat($"⚠️ 尺寸格式错误: '{sizeStr}'，应为'width,height'格式，使用默认尺寸(100,50)");
                return new Vector2(100, 50);
            }

            // 清理空格和其他字符
            string widthStr = dims[0].Trim();
            string heightStr = dims[1].Trim();

            if (!float.TryParse(widthStr, out float width))
            {
                Log.WarningFormat($"⚠️ 宽度解析失败: '{widthStr}'，使用100");
                width = 100f;
            }

            if (!float.TryParse(heightStr, out float height))
            {
                Log.WarningFormat($"⚠️ 高度解析失败: '{heightStr}'，使用50");
                height = 50f;
            }

            // 确保尺寸为正数
            width = Mathf.Max(1f, width);
            height = Mathf.Max(1f, height);

            return new Vector2(width, height);
        }
        catch (System.Exception e)
        {
            Log.ErrorFormat($"❌ 尺寸解析异常: '{sizeStr}' - {e.Message}，使用默认尺寸(100,50)");
            return new Vector2(100, 50);
        }
    }

    /// <summary>
    /// 解析颜色字符串
    /// </summary>
    private Color ParseColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr)) return Color.white;

        if (colorStr.StartsWith("#"))
        {
            ColorUtility.TryParseHtmlString(colorStr, out Color color);
            return color;
        }

        return Color.white;
    }

    /// <summary>
    /// 获取内置字体
    /// </summary>
    private static Font GetBuiltinFont()
    {
        if (builtinFont == null)
            builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return builtinFont;
    }

    /// <summary>
    /// 获取API完成URL
    /// </summary>
    private string GetCompletionsUrl()
    {
        string baseUrl = string.IsNullOrWhiteSpace(apiBase) ? "https://openrouter.ai/" : apiBase;
        baseUrl = baseUrl.TrimEnd('/') + "/";
        return $"{baseUrl}api/v1/chat/completions";
    }

    /// <summary>
    /// 确保Prefab文件夹存在
    /// </summary>
    private static void EnsurePrefabFolders()
    {
        if (!AssetDatabase.IsValidFolder(PrefabRootFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder(PrefabRootFolder, "AIPrefabs");
    }

    /// <summary>
    /// 设置Scene视图为2D模式，确保UI正确显示
    /// </summary>
    private void SetSceneViewTo2D()
    {
        try
        {
            // 获取当前的Scene视图
            var sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                // 设置为2D模式
                sceneView.in2DMode = true;

                // 设置合适的视角
                sceneView.pivot = Vector3.zero;
                //sceneView.rotation = Quaternion.identity;
                sceneView.size = 10f;

                // 刷新视图
                sceneView.Repaint();

                Log.InfoFormat("✅ Scene视图已设置为2D模式");
            }
        }
        catch (System.Exception e)
        {
            Log.WarningFormat($"⚠️ 设置Scene视图失败: {e.Message}");
        }
    }

    #endregion
}
#endif
