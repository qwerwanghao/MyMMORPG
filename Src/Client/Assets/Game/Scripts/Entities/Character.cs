using Common;
using SkillBridge.Message;
using UnityEngine;

namespace Entities
{
    /// <summary>
    /// Character：客户端侧的角色实体（玩家/怪物/NPC 的统一表示）。
    /// - Info：来自协议的 NCharacterInfo（包含角色基础信息与 NEntity）。
    /// - Define：来自配置表 CharacterDefine（用于速度、显示名等本地配置）。
    /// </summary>
    public class Character : Entity
    {
        /// <summary>协议层角色信息（由服务端下发/同步）。</summary>
        public NCharacterInfo Info;

        /// <summary>配置表定义（DataManager.Instance.Characters[tid]）。</summary>
        public Common.Data.CharacterDefine Define;

        /// <summary>
        /// 角色显示名：玩家用协议里的名字；非玩家用配置表名字。
        /// </summary>
        public string Name
        {
            get
            {
                if (this.Info.Type == CharacterType.Player)
                    return this.Info.Name;
                else
                    return this.Define.Name;
            }
        }

        /// <summary>
        /// 是否为“本地玩家自己”。
        /// 用于区分输入控制/相机跟随/显示高亮等。
        /// </summary>
        public bool IsPlayer
        {
            get { return this.Info.Id == Models.User.Instance.CurrentCharacter.Id; }
        }

        /// <summary>
        /// 由协议角色信息构建角色对象：
        /// - 基类 Entity 使用 info.Entity 进行初始化。
        /// - Define 从本地配置表中按 Tid 查到。
        /// </summary>
        public Character(NCharacterInfo info) : base(info.Entity)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Characters[info.Tid];
        }

        /// <summary>向前移动（设置速度为正）。</summary>
        public void MoveForward()
        {
            Log.InfoFormat("MoveForward");
            this.speed = this.Define.Speed;
        }

        /// <summary>向后移动（设置速度为负）。</summary>
        public void MoveBack()
        {
            Log.InfoFormat("MoveBack");
            this.speed = -this.Define.Speed;
        }

        /// <summary>停止移动（速度归零）。</summary>
        public void Stop()
        {
            Log.InfoFormat("Stop");
            this.speed = 0;
        }

        /// <summary>设置朝向（通常由输入或同步驱动）。</summary>
        public void SetDirection(Vector3Int direction)
        {
            Log.InfoFormat("SetDirection:{0}", direction);
            this.direction = direction;
        }

        /// <summary>设置位置（通常由同步/传送/校正驱动）。</summary>
        public void SetPosition(Vector3Int position)
        {
            Log.InfoFormat("SetPosition:{0}", position);
            this.position = position;
        }
    }
}
