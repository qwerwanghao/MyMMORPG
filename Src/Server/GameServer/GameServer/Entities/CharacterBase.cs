using Common.Data;
using GameServer.Core;
using SkillBridge.Message;

namespace GameServer.Entities
{
    /// <summary>
    /// 角色基类：继承自 Entity，为 Character 和 Monster 提供公共属性和方法
    /// </summary>
    class CharacterBase : Entity
    {
        #region 公共属性

        /// <summary>
        /// 角色实体ID
        /// </summary>
        public int Id
        {
            get { return this.entityId; }
        }

        /// <summary>
        /// 角色网络信息（用于同步到客户端）
        /// </summary>
        public NCharacterInfo Info;

        /// <summary>
        /// 角色定义数据（来自配置表）
        /// </summary>
        public CharacterDefine Define;

        #endregion

        #region 构造函数

        /// <summary>
        /// 基础构造函数：仅设置位置和方向
        /// </summary>
        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir)
        {
        }

        /// <summary>
        /// 完整构造函数：设置角色类型、等级、位置和方向
        /// </summary>
        public CharacterBase(CharacterType type, int tid, int level, Vector3Int pos, Vector3Int dir) :
            base(pos, dir)
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.Tid = tid;
            this.Info.Entity = this.EntityData;
            this.Info.Name = this.Define.Name;
        }

        #endregion
    }
}
