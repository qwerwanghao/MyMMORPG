using System;
using Common;
using Common.Data;
using Models;
using Network;
using SkillBridge.Message;
using UnityEngine;

namespace Services
{
    /// <summary>
    /// MapService：客户端地图服务（接收服务端地图相关广播，并驱动客户端切场景/角色列表同步）。
    /// 当前处理的消息：
    /// - MapCharacterEnterResponse：进入地图（全量/增量角色列表）
    /// - MapCharacterLeaveResponse：离开地图（暂未实现）
    /// </summary>
    
    class MapService : Singleton<MapService>, IDisposable
    {
        /// <summary>客户端当前所在地图ID（用于判断是否需要切场景）。</summary>
        public int CurrentMapId = 0;

        private bool initialized = false;

        public MapService()
        {
        }

        public void Run()
        {
            if (this.initialized)
            {
                return;
            }

            MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Subscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            this.initialized = true;
        }

        public void Dispose()
        {
            if (!this.initialized)
            {
                return;
            }

            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Unsubscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);
            this.initialized = false;
        }

        /// <summary>
        /// 收到“进入地图”广播：
        /// - 把服务器回包中的角色列表写入本地 CharacterManager（用于显示/同步）。
        /// - 如果地图ID变化，则切换到对应场景。
        /// </summary>
        private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        {
            if (response == null)
            {
                Log.Warning("OnMapCharacterEnter: response is null");
                return;
            }

            int count = response.Characters != null ? response.Characters.Count : 0;
            Log.InfoFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, count);

            if (response.Characters != null)
            {
                foreach (var cha in response.Characters)
                {
                    if (cha == null)
                    {
                        continue;
                    }

                    // 当前角色可能为空（例如还未完成选角/未初始化 CurrentCharacter），这里要做防御性判空
                    if (User.Instance.CurrentCharacter != null && User.Instance.CurrentCharacter.Id == cha.Id)
                    {
                        // 当前角色切换地图：用服务器回包刷新一次数据（含 mapId/entity 等）
                        User.Instance.CurrentCharacter = cha;
                    }

                    // 服务器返回的进入地图角色列表，交给角色管理器统一维护
                    CharacterManager.Instance.AddCharacter(cha);
                }
            }
            else
            {
                Log.Warning("OnMapCharacterEnter: response.Characters is null");
            }

            if (CurrentMapId != response.mapId)
            {
                this.EnterMap(response.mapId);
                this.CurrentMapId = response.mapId;
            }
        }

        private void OnMapCharacterLeave(object sender, MapCharacterLeaveResponse response)
        {

        }

        private void EnterMap(int mapId)
        {
            if (DataManager.Instance.Maps.ContainsKey(mapId))
            {
                MapDefine map = DataManager.Instance.Maps[mapId];
                if (SceneManager.Instance == null)
                {
                    var go = new GameObject("SceneManager");
                    go.AddComponent<SceneManager>();
                }
                SceneManager.Instance.LoadScene(map.Resource);
            }
            else
                Log.ErrorFormat("EnterMap: Map {0} not existed", mapId);
        }
    }
}
