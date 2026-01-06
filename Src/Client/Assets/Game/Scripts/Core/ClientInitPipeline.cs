using System.IO;
using Common;
using Network;
using Services;
using UnityEngine;

/// <summary>
/// ClientInitPipeline：客户端初始化入口（同步）。
/// </summary>
public static class ClientInitPipeline
{
    private static bool ran = false;

    public static void Run()
    {
        if (ran)
        {
            return;
        }

        InitLogging();
        EnsureUnitySingletons();

        MapService.Instance.Run();
        UserService.Instance.Run();
        CharacterManager.Instance.Run();

        ran = true;
    }

    private static void EnsureUnitySingletons()
    {
        EnsureMonoSingleton<NetClient>("NetClient");
        EnsureMonoSingleton<global::SceneManager>("SceneManager");
    }

    private static void EnsureMonoSingleton<T>(string gameObjectName) where T : MonoBehaviour
    {
        if (Object.FindFirstObjectByType<T>() != null)
        {
            return;
        }

        var go = new GameObject(gameObjectName);
        go.AddComponent<T>();
    }

    private static void InitLogging()
    {
        // 使用绝对路径配置 log4net
        var configPath = Path.Combine(Application.dataPath, "Resources/log4net.xml");
        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(configPath));

        // 初始化旧的 Log 系统以保持兼容性
        Log.Init("Unity");
        UnityLogger.Init();

        Log.Info("ClientInitPipeline.Run()");
    }
}

