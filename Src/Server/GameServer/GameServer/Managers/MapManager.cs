using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Models;

namespace GameServer.Managers
{
    class MapManager : Singleton<MapManager>
    {
        private Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        public void Init()
        {
            foreach (var define in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(define);
                this.Maps[define.ID] = map;
                Log.InfoFormat("MapManager: Loaded Map {0}:{1}", define.ID, define.Name);
            }
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public Map this[int key]
        {
            get
            {
                return Maps.ContainsKey(key) ? Maps[key] : null;
            }
        }

        public void Update()
        {
            foreach (var map in Maps.Values)
            {
                map.Update();
            }
        }
    }
}
