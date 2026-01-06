using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Common;

using SkillBridge.Message;
using ProtoBuf;
using Services;

public class LoadingManager : MonoBehaviour
{
    /// <summary>
    /// LoadingManager：客户端启动流程入口（启动画面 → 加载数据 → 初始化服务 → 显示登录 UI）。
    /// - 初始化 log4net/Log 系统（便于联调定位）。
    /// - 调用 DataManager.LoadData 读取 `Data/` 下策划配置。
    /// - 初始化基础服务（MapService/UserService 等）。
    /// </summary>

    public GameObject UITips;
    public GameObject UILoading;
    public GameObject UILogin;

    public Slider progressBar;
    public Text progressText;
    public Text progressNumber;

    // Use this for initialization
    IEnumerator Start()
    {
        // 使用绝对路径配置log4net
        var configPath = Path.Combine(Application.dataPath, "Resources/log4net.xml");

        // 初始化log4net配置
        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(configPath));

        // 初始化旧的Log系统以保持兼容性
        Log.Init("Unity");
        UnityLogger.Init();

        // 使用改进的日志系统记录日志
        Log.Info("LoadingManager start");

        UITips.SetActive(true);
        UILoading.SetActive(false);
        UILogin.SetActive(false);
        yield return new WaitForSeconds(2f);
        UILoading.SetActive(true);
        yield return new WaitForSeconds(1f);
        UITips.SetActive(false);

        yield return DataManager.Instance.LoadData();

        //Init basic services
        MapService.Instance.Init();
        UserService.Instance.Init();


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


    // Update is called once per frame
    void Update()
    {

    }
}
