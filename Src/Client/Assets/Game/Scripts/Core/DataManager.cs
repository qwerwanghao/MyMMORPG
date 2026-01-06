using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Data;
using Newtonsoft.Json;

public class DataManager : Singleton<DataManager>
{
    /// <summary>
    /// DataManagerï¼šå®¢æˆ·ç«¯é…ç½®/ç­–åˆ’æ•°æ®åŠ è½½å™¨ã€?    /// - ä»?`Data/` ç›®å½•è¯»å– JSON æ–‡æœ¬ï¼ˆMapDefine/CharacterDefine/...ï¼‰ã€?    /// - æä¾›å­—å…¸ç´¢å¼•ç»?UIã€åœºæ™¯åˆ‡æ¢ã€è§’è‰²å®šä¹‰ç­‰ä½¿ç”¨ã€?    /// - åŒæ—¶è¯»å– GameServerConfigï¼ˆæœåŠ¡å™¨åœ°å€/ç«¯å£ï¼‰ç”¨äºŽç½‘ç»œè¿žæŽ¥é…ç½®ã€?    /// </summary>
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
