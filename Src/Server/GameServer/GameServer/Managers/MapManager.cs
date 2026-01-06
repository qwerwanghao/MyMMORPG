using Common;
using GameServer.Models;
using System.Collections.Generic;

namespace GameServer.Managers
{
    /// <summary>
    /// MapManager：地图管理器，负责管理所有地图实例
    /// </summary>
    class MapManager : Singleton<MapManager>
    {
        #region 私有字段

        private Dictionary<int, Map> Maps = new Dictionary<int, Map>();

        #endregion

        #region 索引器

        /// <summary>
        /// 通过地图ID获取地图实例
        /// </summary>
        public Map this[int key]
        {
            get
            {
                return Maps.ContainsKey(key) ? Maps[key] : null;
            }
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化：从 DataManager 加载所有地图定义并创建地图实例
        /// </summary>
        public void Init()
        {
            foreach (var define in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(define);
                this.Maps[define.ID] = map;
                Log.InfoFormat("MapManager: Loaded Map {0}:{1}", define.ID, define.Name);
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
        /// 更新：更新所有地图实例
        /// </summary>
        public void Update()
        {
            foreach (var map in Maps.Values)
            {
                map.Update();
            }
        }

        #endregion
    }
}
