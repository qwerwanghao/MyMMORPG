using GameServer.Core;
using SkillBridge.Message;

namespace GameServer.Entities
{
    /// <summary>
    /// 实体基类：所有游戏对象的基类，提供位置、方向、速度等基础属性
    /// </summary>
    public class Entity
    {
        #region 私有字段

        private Vector3Int position;
        private Vector3Int direction;
        private int speed;
        private NEntity entityData;

        #endregion

        #region 公共属性

        /// <summary>
        /// 实体ID（来自 entityData）
        /// </summary>
        public int entityId
        {
            get { return this.entityData.Id; }
        }

        /// <summary>
        /// 实体位置（同步到 entityData）
        /// </summary>
        public Vector3Int Position
        {
            get { return position; }
            set
            {
                position = value;
                this.entityData.Position = position;
            }
        }

        /// <summary>
        /// 实体朝向（同步到 entityData）
        /// </summary>
        public Vector3Int Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                this.entityData.Direction = direction;
            }
        }

        /// <summary>
        /// 移动速度（同步到 entityData）
        /// </summary>
        public int Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                this.entityData.Speed = speed;
            }
        }

        /// <summary>
        /// 实体网络数据（用于同步到客户端）
        /// </summary>
        public NEntity EntityData
        {
            get
            {
                return entityData;
            }
            set
            {
                entityData = value;
                this.SetEntityData(value);
            }
        }

        #endregion

        #region 构造函数

        /// <summary>
        /// 从位置和方向创建实体
        /// </summary>
        public Entity(Vector3Int pos, Vector3Int dir)
        {
            this.entityData = new NEntity();
            this.entityData.Position = pos;
            this.entityData.Direction = dir;
            this.SetEntityData(this.entityData);
        }

        /// <summary>
        /// 从网络数据创建实体
        /// </summary>
        public Entity(NEntity entity)
        {
            this.entityData = entity;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置实体数据并同步到内部状态
        /// </summary>
        public void SetEntityData(NEntity entity)
        {
            this.Position = entity.Position;
            this.Direction = entity.Direction;
            this.speed = entity.Speed;
        }

        #endregion
    }
}
