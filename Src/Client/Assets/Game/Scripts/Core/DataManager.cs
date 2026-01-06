using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System;
using System.IO;

using Common.Data;

using Newtonsoft.Json;
using Common;

public class DataManager : Singleton<DataManager>
{
    /// <summary>
    /// DataManager：客户端配置/策划数据加载器。
    /// - 从 `Data/` 目录读取 JSON 文本（MapDefine/CharacterDefine/...）。
    /// - 提供字典索引给 UI、场景切换、角色定义等使用。
    /// - 同时读取 GameServerConfig（服务器地址/端口）用于网络连接配置。
    /// </summary>
    public string DataPath;
    public Dictionary<int, MapDefine> Maps = null;
    public Dictionary<int, CharacterDefine> Characters = null;
    public Dictionary<int, TeleporterDefine> Teleporters = null;
    public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;
    public GameServerConfig Config = null;


    public DataManager()
    {
        this.DataPath = "Data/";
        Log.InfoFormat("DataManager > DataManager()");
    }

    public void Load()
    {
        try
        {
            string jsonConfig = File.ReadAllText(this.DataPath + "GameServerConfig.txt");
            this.Config = JsonConvert.DeserializeObject<GameServerConfig>(jsonConfig);
        }
        catch (Exception e)
        {
            Log.WarningFormat("Load GameServerConfig failed: {0}", e.Message);
            this.Config = new GameServerConfig()
            {
                ServerHost = "127.0.0.1",
                ServerPort = 8000
            };
        }

        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

        json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

        json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
        this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
        this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);
    }


    public IEnumerator LoadData()
    {
        try
        {
            string jsonConfig = File.ReadAllText(this.DataPath + "GameServerConfig.txt");
            this.Config = JsonConvert.DeserializeObject<GameServerConfig>(jsonConfig);
        }
        catch (Exception e)
        {
            Log.WarningFormat("LoadData GameServerConfig failed: {0}", e.Message);
            this.Config = new GameServerConfig()
            {
                ServerHost = "127.0.0.1",
                ServerPort = 8000
            };
        }

        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
        this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
        this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);

        yield return null;
    }

#if UNITY_EDITOR
    public void SaveTeleporters()
    {
        string json = JsonConvert.SerializeObject(this.Teleporters, Formatting.Indented);
        File.WriteAllText(this.DataPath + "TeleporterDefine.txt", json);
    }

    public void SaveSpawnPoints()
    {
        string json = JsonConvert.SerializeObject(this.SpawnPoints, Formatting.Indented);
        File.WriteAllText(this.DataPath + "SpawnPointDefine.txt", json);
    }

#endif
}
