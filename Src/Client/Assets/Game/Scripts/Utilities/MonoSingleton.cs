using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public bool global = true;

    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
            }
            return instance;
        }
    }

    private void Start()
    {
        if (global) DontDestroyOnLoad(this.gameObject);
        this.OnStart();
    }

    protected virtual void OnStart()
    {
    }
}
