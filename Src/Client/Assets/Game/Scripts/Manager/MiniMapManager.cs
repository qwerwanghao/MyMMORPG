using System;
using Models;
using UnityEngine;

public class MiniMapManager : Singleton<MiniMapManager>
{
    public Sprite LoadSprite()
    {
        var miniMap = User.Instance.CurrentMapData.MiniMap;
        if (string.IsNullOrEmpty(miniMap))
            return null;

        string spritePath = "UI/MiniMap/" + miniMap;
        return Resloader.Load<Sprite>(spritePath);
    }
}
    
