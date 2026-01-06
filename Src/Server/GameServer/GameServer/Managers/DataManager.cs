using System.Collections.Generic;
using System.Text;
using System;
using System.IO;
using System.Linq;

using Common;
using Common.Data;

using Newtonsoft.Json;
namespace GameServer.Managers
{
    public class DataManager : Singleton<DataManager>
    {
        internal string DataPath;
        internal Dictionary<int, MapDefine> Maps = null;
        internal Dictionary<int, CharacterDefine> Characters = null;
        internal Dictionary<int, TeleporterDefine> Teleporters = null;
        public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null;
        public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null;

        private string dataDirectory;

        public DataManager()
        {
            this.DataPath = "Data/";
            this.dataDirectory = null;
            Log.Info("DataManager > DataManager()");
        }

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

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Update()
        {
        }

        internal void Load()
        {
            EnsureCollectionsNotNull();

            LoadJsonOrEmpty("MapDefine.txt", out this.Maps);
            LoadJsonOrEmpty("CharacterDefine.txt", out this.Characters);
            LoadJsonOrEmpty("TeleporterDefine.txt", out this.Teleporters);

            Log.InfoFormat("DataManager.Load: Maps={0} Characters={1} Teleporters={2}",
                this.Maps.Count, this.Characters.Count, this.Teleporters.Count);
        }

        private void EnsureCollectionsNotNull()
        {
            if (this.Maps == null) this.Maps = new Dictionary<int, MapDefine>();
            if (this.Characters == null) this.Characters = new Dictionary<int, CharacterDefine>();
            if (this.Teleporters == null) this.Teleporters = new Dictionary<int, TeleporterDefine>();
        }

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
    }
}
