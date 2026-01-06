using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SkillBridge.Message;

namespace Entities
{
    /// <summary>
    /// Entity：客户端运行时的“可同步实体”基类。
    /// - 维护本地状态：位置/朝向/速度（用于移动、渲染等）。
    /// - 维护协议状态：NEntity（用于网络同步/回包/广播）。
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// 实体唯一 ID（来自协议 NEntity.Id；通常由服务端分配）。
        /// </summary>
        public int entityId;


        /// <summary>当前位置（客户端本地坐标）。</summary>
        public Vector3Int position;
        /// <summary>朝向（客户端本地坐标）。</summary>
        public Vector3Int direction;
        /// <summary>移动速度（协议里的单位；由子类/输入驱动改变）。</summary>
        public int speed;


        /// <summary>
        /// 协议层实体数据（网络同步用）。
        /// 设置时会同步刷新本地 position/direction/speed。
        /// </summary>
        private NEntity entityData;
        public NEntity EntityData
        {
            get {
                return entityData;
            }
            set {
                entityData = value;
                this.SetEntityData(value);
            }
        }

        public Entity(NEntity entity)
        {
            // 初始化：把协议数据映射到本地字段
            this.entityId = entity.Id;
            this.entityData = entity;
            this.SetEntityData(entity);
        }

        /// <summary>
        /// 每帧更新（delta 为秒）：
        /// - 根据速度与朝向推进位置（简单直线移动）。
        /// - 把本地 position/direction/speed 写回 entityData，以便网络同步。
        /// </summary>
        public virtual void OnUpdate(float delta)
        {
            if (this.speed != 0)
            {
                Vector3 dir = this.direction;
                this.position += Vector3Int.RoundToInt(dir * speed * delta / 100f);
            }
            entityData.Position.FromVector3Int(this.position);
            entityData.Direction.FromVector3Int(this.direction);
            entityData.Speed = this.speed;
        }

        /// <summary>
        /// 从协议数据刷新本地字段（位置/朝向/速度）。
        /// </summary>
        public void SetEntityData(NEntity entity)
        {
            this.position = this.position.FromNVector3(entity.Position);
            this.direction = this.direction.FromNVector3(entity.Direction);
            this.speed = entity.Speed;
        }
    }
}
