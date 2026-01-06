using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.Events;

public class SceneManager : MonoSingleton<SceneManager>
{
    private UnityAction<float> onProgress = null;

    protected override void OnStart()
    {
    }

    private void Update()
    {
    }

    public void LoadScene(string name)
    {
        StartCoroutine(LoadLevel(name));
    }

    private IEnumerator LoadLevel(string name)
    {
        Log.InfoFormat("LoadLevel: {0}", name);
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name);
        async.allowSceneActivation = true;
        async.completed += LevelLoadCompleted;
        while (!async.isDone)
        {
            if (onProgress != null)
                onProgress(async.progress);
            yield return null;
        }
    }

    private void LevelLoadCompleted(AsyncOperation obj)
    {
        if (onProgress != null)
            onProgress(1f);
        Log.InfoFormat("LevelLoadCompleted:" + obj.progress);
    }
}
