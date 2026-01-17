using Common.Data;
using System;
using UnityEngine;

namespace Models
{
    class User : Singleton<User>
    {
        private SkillBridge.Message.NUserInfo userInfo;
        private GameObject currentCharacterObject;

        public SkillBridge.Message.NUserInfo Info
        {
            get { return userInfo; }
        }

        public SkillBridge.Message.NCharacterInfo CurrentCharacter { get; set; }
         
        public MapDefine CurrentMapData { get; set; }
         
        public event Action<GameObject> CurrentCharacterObjectChanged;

        public GameObject CurrentCharacterObject
        {
            get => this.currentCharacterObject;
            set
            {
                if (this.currentCharacterObject == value) return;
                this.currentCharacterObject = value;
                this.CurrentCharacterObjectChanged?.Invoke(this.currentCharacterObject);
            }
        }

        public void SetupUserInfo(SkillBridge.Message.NUserInfo info)
        {
            this.userInfo = info;
        }
    }
}
