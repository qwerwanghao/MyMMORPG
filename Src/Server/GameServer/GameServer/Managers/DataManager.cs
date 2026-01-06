using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Common.Data;
using Newtonsoft.Json;

namespace GameServer.Managers
{
    /// <summary>
    /// DataManager：游戏数据管理器，负责从 JSON 文件加载游戏配置数据
    /// </summary>
    public class DataManager : Singleton<DataManager>
    {
        #region 公共字段（数据字典）

        /// <summary>
        /// 数据路径配置
        /// </summary>
        internal string DataPath;

        /// <summary>
        /// 地图定义字典
        /// </summary>
        internal Dictionary<int, MapDefine> Maps = null;

        /// <summary>
        /// 角色定义字典
        /// </summary>
        internal Dictionary<int, CharacterDefine> Characters = null;

        /// <summary>
        /// 传送门定义字典
        /// </summary>
        internal Dictionary<int, TeleporterDefine> Teleporters = null;

        /// <summary>
        /// 刷怪点字典（嵌套：地图ID → 刷怪点ID → 定义）
        /// </summary>
        public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;

        /// <summary>
        /// 刷怪规则字典（嵌套：地图ID → 规则ID → 定义）
        /// </summary>
        public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null;

        #endregion

        #region 私有字段

        private string dataDirectory;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数：初始化数据路径
        /// </summary>
        public DataManager()
        {
            this.DataPath = "Data/";
            this.dataDirectory = null;
            Log.Info("DataManager > DataManager()");
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化：解析数据目录路径并加载数据
        /// </summary>
        public void Init()
        {
            try
            {
                this.dataDirectory = ResolveDataDirectory(this.DataPath);
                Log.InfoFormat("DataManager.Init: DataDirectory={0}", this.dataDirectory);
                Load();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("DataManager.Init failed: {0}", ex);
                EnsureCollectionsNotNull();
            }
        }

        /// <summary>
        /// 启动（IService 接口实现）
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// 停止（IService 接口实现）
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// 更新（IService 接口实现）
        /// </summary>
        public void Update()
        {
        }

        #endregion

        #region 数据加载

        /// <summary>
        /// 加载所有 JSON 配置文件
        /// </summary>
        internal void Load()
        {
            EnsureCollectionsNotNull();

            LoadJsonOrEmpty("MapDefine.txt", out this.Maps);
            LoadJsonOrEmpty("CharacterDefine.txt", out this.Characters);
            LoadJsonOrEmpty("TeleporterDefine.txt", out this.Teleporters);

            Log.InfoFormat("DataManager.Load: Maps={0} Characters={1} Teleporters={2}",
                this.Maps.Count, this.Characters.Count, this.Teleporters.Count);
        }

        /// <summary>
        /// 确保所有集合字典已初始化
        /// </summary>
        private void EnsureCollectionsNotNull()
        {
            if (this.Maps == null) this.Maps = new Dictionary<int, MapDefine>();
            if (this.Characters == null) this.Characters = new Dictionary<int, CharacterDefine>();
            if (this.Teleporters == null) this.Teleporters = new Dictionary<int, TeleporterDefine>();
        }

        /// <summary>
        /// 获取数据文件的完整路径
        /// </summary>
        private string GetDataFilePath(string fileName)
        {
            string dir = this.dataDirectory;
            if (string.IsNullOrEmpty(dir))
            {
                dir = ResolveDataDirectory(this.DataPath);
                this.dataDirectory = dir;
            }
            return Path.Combine(dir, fileName);
        }

        /// <summary>
        /// 加载 JSON 文件到字典，失败时返回空字典
        /// </summary>
        private void LoadJsonOrEmpty<T>(string fileName, out Dictionary<int, T> target)
        {
            target = new Dictionary<int, T>();
            string path = GetDataFilePath(fileName);

            if (!File.Exists(path))
            {
                Log.WarningFormat("DataManager: data file not found: {0}", path);
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<Dictionary<int, T>>(json);
                if (data != null)
                {
                    target = data;
                }
                else
                {
                    Log.WarningFormat("DataManager: data file is empty or invalid JSON: {0}", path);
                }
            }
            catch (Exception e)
            {
                Log.ErrorFormat("DataManager: failed to load {0}: {1}", path, e);
            }
        }

        /// <summary>
        /// 解析数据目录路径（支持多个候选路径）
        /// </summary>
        private static string ResolveDataDirectory(string configuredPath)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                candidates.Add(configuredPath);

                if (!Path.IsPathRooted(configuredPath))
                {
                    candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath));
                    candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), configuredPath));
                }
            }

            candidates.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "Data"));

            foreach (string candidate in candidates.Select(p => Path.GetFullPath(p)).Distinct())
            {
                try
                {
                    if (Directory.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch
                {
                    // ignore invalid candidate
                }
            }

            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));
        }

        #endregion
    }
}
