using Common.Data;
using GameServer.Core;
using SkillBridge.Message;

namespace GameServer.Entities
{
    /// <summary>
    /// 玩家角色类：继承自 CharacterBase，表示游戏中的玩家角色实例
    /// </summary>
    public class Character : CharacterBase
    {
        #region 公共字段

        /// <summary>
        /// 角色数据库记录
        /// </summary>
        public TCharacter Data;

        #endregion

        #region 构造函数

        /// <summary>
        /// 创建玩家角色实例
        /// </summary>
        /// <param name="type">角色类型</param>
        /// <param name="cha">角色数据库数据</param>
        public Character(CharacterType type, TCharacter cha) :
            base(new Core.Vector3Int(cha.MapPosX, cha.MapPosY, cha.MapPosZ), new Core.Vector3Int(100, 0, 0))
        {
            this.Data = cha;
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Id = cha.ID;
            this.Info.Name = cha.Name;
            this.Info.Level = 1; // cha.Level;
            this.Info.Tid = cha.TID;
            this.Info.Class = (CharacterClass)cha.Class;
            this.Info.mapId = cha.MapID;
            this.Info.Entity = this.EntityData;
        }

        #endregion
    }
}
