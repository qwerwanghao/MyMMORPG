using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Network;
using UnityEngine;
using UnityEngine.Events;

using Entities;
using SkillBridge.Message;

namespace Services
{
    /// <summary>
    /// CharacterManager：客户端角色管理器（逻辑层）。
    /// - 维护当前客户端已知的所有角色（来自服务器 MapCharacterEnterResponse 等）。
    /// - 提供 OnCharacterEnter 回调，供 GameObjectManager 创建/更新场景表现。
    /// </summary>
    class CharacterManager : Singleton<CharacterManager>, IDisposable
    {
        /// <summary>当前已知角色：key=角色ID（NCharacterInfo.Id）。</summary>
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();


        /// <summary>当新增角色进入时触发（例如进入地图全量/增量同步）。</summary>
        public UnityAction<Character> OnCharacterEnter;

        public CharacterManager()
        {

        }

        public void Dispose()
        {
        }

        public void Init()
        {

        }

        public void Clear()
        {
            this.Characters.Clear();
        }

        public void AddCharacter(SkillBridge.Message.NCharacterInfo cha)
        {
            // 将协议角色信息转换为客户端运行时角色对象（Entities.Character）
            Log.InfoFormat("AddCharacter:{0}:{1} Map:{2} Entity:{3}", cha.Id, cha.Name, cha.mapId, cha.Entity.String());
            Character character = new Character(cha);
            this.Characters[cha.Id] = character;

            if(OnCharacterEnter!=null)
            {
                OnCharacterEnter(character);
            }
        }


        public void RemoveCharacter(int characterId)
        {
            Log.InfoFormat("RemoveCharacter:{0}", characterId);
            this.Characters.Remove(characterId);

        }
    }
}
