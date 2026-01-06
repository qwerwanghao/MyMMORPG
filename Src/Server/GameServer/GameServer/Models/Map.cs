using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;

using Common;
using Common.Data;

using Network;
using GameServer.Managers;
using GameServer.Entities;

namespace GameServer.Models
{
    /// <summary>
    /// Map：服务端地图实例（对应一张逻辑地图/场景）。
    /// - 维护地图内在线角色列表（线程安全：ConcurrentDictionary）。
    /// - 负责进入/离开地图时的广播（MapCharacterEnter/Leave）。
    /// - 具体刷怪/同步/逻辑 Tick 可放在 Update()。
    /// </summary>
    class Map
    {
        internal class MapCharacter
        {
            // 该角色所在的网络连接（用于下发广播）
            public NetConnection<NetSession> connection;
            // 服务端运行时角色对象（GameServer.Entities.Character）
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }

        public int ID
        {
            // 地图ID（来自配置表 MapDefine.ID）
            get { return this.Define.ID; }
        }
        internal MapDefine Define;

        // 地图内的角色列表：key=CharacterId，value=连接+角色对象。
        // 注意：网络消息处理是多线程（MessageDistributer.Start(8)），进入/离开可能并发发生，因此用并发集合。
        private readonly ConcurrentDictionary<int, MapCharacter> mapCharacters = new ConcurrentDictionary<int, MapCharacter>();


        internal Map(MapDefine define)
        {
            this.Define = define;
        }

        internal void Update()
        {
        }

        /// <summary>
        /// 角色进入地图
        /// </summary>
        /// <param name="conn">进入者的连接</param>
        /// <param name="character">进入者的服务端运行时角色</param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}", this.Define.ID, character.Id);

            // 更新进入者的协议数据：告诉客户端“当前在哪张地图”
            character.Info.mapId = this.ID;

            // 发送给“进入者”的全量进入消息：包含自己 + 当前地图已有的所有角色信息
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character.Info);

            foreach (var kv in this.mapCharacters)
            {
                // 给进入者：把已有角色列表一并带回，用于一次性生成当前地图内的其他角色
                message.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);
                // 给已在地图里的每个人：广播“进入者来了”（增量进入）
                this.SendCharacterEnterMap(kv.Value.connection, character.Info);
            }

            // 把进入者加入地图（后续广播/离开会用到）
            this.mapCharacters[character.Id] = new MapCharacter(conn, character);

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }

        /// <summary>
        /// 向“已在地图里的玩家”发送“某角色进入地图”的增量消息。
        /// </summary>
        void SendCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            message.Response.mapCharacterEnter.mapId = this.Define.ID;
            message.Response.mapCharacterEnter.Characters.Add(character);

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }

        /// <summary>
        /// 角色离开地图
        /// </summary>
        /// <param name="character">要离开的角色</param>
        internal void CharacterLeave(Character character)
        {
            // 1. 空检查
            if (character == null)
            {
                Log.Warning("CharacterLeave: character is null");
                return;
            }

            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}", this.Define.ID, character.Id);

            // 2. 从地图角色列表中移除该角色
            MapCharacter removedCharacter;
            if (!this.mapCharacters.TryRemove(character.Id, out removedCharacter))
            {
                Log.WarningFormat("CharacterLeave: character {0} not found in map {1}", character.Id, this.Define.ID);
                return;
            }

            // 3. 通知地图上其他玩家该角色已离开（增量离开广播）
            foreach (var kv in this.mapCharacters)
            {
                this.SendCharacterLeaveMap(kv.Value.connection, character.Id);
            }

            Log.InfoFormat("CharacterLeave: character {0} left map {1}, remaining characters: {2}",
                character.Id, this.Define.ID, this.mapCharacters.Count);
        }

        /// <summary>
        /// 向指定连接发送角色离开地图消息
        /// </summary>
        /// <param name="conn">目标连接</param>
        /// <param name="characterId">离开的角色ID</param>
        void SendCharacterLeaveMap(NetConnection<NetSession> conn, int characterId)
        {
            if (conn == null)
            {
                Log.Warning("SendCharacterLeaveMap: connection is null");
                return;
            }

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();

            message.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            message.Response.mapCharacterLeave.characterId = characterId;

            byte[] data = PackageHandler.PackMessage(message);
            conn.SendData(data, 0, data.Length);
        }
    }
}
