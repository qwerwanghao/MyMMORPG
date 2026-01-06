using UnityEngine;

public class MainPlayerCamera : MonoSingleton<MainPlayerCamera>
{
    public Camera followCamera;
    public Transform viewPoint;

    /// <summary>本地玩家 GameObject（相机将跟随其 Transform）。</summary>
    public GameObject player;

    private void Start()
    {
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
        // 在 LateUpdate 跟随，避免抖动（先让角色更新完，再更新相机）
        if (player == null)
            return;

        this.transform.position = player.transform.position;
        this.transform.rotation = player.transform.rotation;
    }
}
