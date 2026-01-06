using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Resloader
{
    /// <summary>
    /// Resloader：Resources.Load 的薄封装，统一资源加载入口。
    /// 注意：path 是 Resources/ 目录下的相对路径（不带扩展名）。
    /// </summary>
    public static T Load<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }
}
