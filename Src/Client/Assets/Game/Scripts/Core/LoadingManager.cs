using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    /// <summary>
    /// LoadingManager：客户端启动流程入口（启动画面 → 加载数据 → 初始化服务 → 显示登录 UI）。
    /// - 初始化 log4net/Log 系统（便于联调定位）。
    /// - 调用 DataManager.LoadData 读取 `Data/` 下策划配置。
    /// - 调用 ClientInitPipeline.Run 初始化基础设施（日志、单例、服务/管理器订阅等）。
    /// </summary>

    [Header("UI References")]
    public GameObject UITips;
    public GameObject UILoading;
    public GameObject UILogin;
    public Slider progressBar;
    public Text progressText;
    public Text progressNumber;

    private void Awake()
    {
        // 尽早初始化基础设施，避免其它脚本在 Start 里访问到未就绪的单例（如 NetClient/SceneManager）。
        ClientInitPipeline.Run();
    }

    private IEnumerator Start()
    {
        UITips.SetActive(true);
        UILoading.SetActive(false);
        UILogin.SetActive(false);

        yield return new WaitForSeconds(2f);
        UILoading.SetActive(true);
        yield return new WaitForSeconds(1f);
        UITips.SetActive(false);

        yield return DataManager.Instance.LoadData();

        // Fake Loading Simulate
        for (float i = 50; i < 100;)
        {
            i += Random.Range(0.1f, 1.5f);
            progressBar.value = i;
            yield return new WaitForEndOfFrame();
        }

        UILoading.SetActive(false);
        UILogin.SetActive(true);
        yield return null;
    }

    private void Update()
    {
    }
}
