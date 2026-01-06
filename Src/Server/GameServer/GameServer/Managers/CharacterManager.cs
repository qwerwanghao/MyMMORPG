using Common;
using GameServer.Entities;
using Network;
using SkillBridge.Message;
using System.Collections.Concurrent;

namespace GameServer.Managers
{
    /// <summary>
    /// CharacterManager 负责管理游戏中的所有角色实例
    /// 主要职责：角色生命周期管理（创建、移除、存储）
    /// </summary>
    class CharacterManager : Singleton<CharacterManager>
    {
        #region 私有字段

        /// <summary>
        /// 角色字典：使用 ConcurrentDictionary 确保线程安全
        /// </summary>
        private readonly ConcurrentDictionary<int, Character> characters;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数：初始化角色字典
        /// </summary>
        public CharacterManager()
        {
            this.characters = new ConcurrentDictionary<int, Character>();
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public void Init()
        {
            Log.Info("CharacterManager initialized");
        }

        /// <summary>
        /// 释放资源：清空所有角色
        /// </summary>
        public void Dispose()
        {
            this.Clear();
        }

        /// <summary>
        /// 清空所有角色
        /// </summary>
        public void Clear()
        {
            this.characters.Clear();
            Log.Info("CharacterManager: All characters cleared");
        }

        #endregion

        #region 角色管理方法

        /// <summary>
        /// 添加角色到管理器
        /// </summary>
        public Character AddCharacter(TCharacter cha)
        {
            if (cha == null)
            {
                Log.Error("CharacterManager.AddCharacter: TCharacter is null");
                return null;
            }

            Character character = new Character(CharacterType.Player, cha);
            this.characters[cha.ID] = character;
            return character;
        }

        /// <summary>
        /// 从管理器中移除角色
        /// </summary>
        public void RemoveCharacter(int charId)
        {
            if (this.characters.ContainsKey(charId))
            {
                this.characters.TryRemove(charId, out _);
            }
        }

        /// <summary>
        /// 根据ID获取角色
        /// </summary>
        public Character GetCharacter(int charId)
        {
            Character character;
            return this.characters.TryGetValue(charId, out character) ? character : null;
        }

        #endregion
    }
}
