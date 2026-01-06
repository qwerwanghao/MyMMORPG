using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using Services;
using SkillBridge.Message;
using Common;

/// <summary>
/// GameObjectManager：根据 CharacterManager 中的逻辑角色列表，创建/管理 Unity 场景中的角色 GameObject。
/// - 监听 CharacterManager.OnCharacterEnter：服务器通知进入地图时，创建对应的角色对象。
/// - 初始化时遍历已有角色列表，补齐场景内对象。
/// - 对本地玩家：启用 PlayerInputController，并把 MainPlayerCamera 跟随目标设置为该角色。
/// </summary>
public class GameObjectManager : MonoBehaviour
{

    /// <summary>已创建的角色 GameObject：key=角色ID（NCharacterInfo.Id）。</summary>
    Dictionary<int, GameObject> Characters = new Dictionary<int, GameObject>();
    // Use this for initialization
    void Start()
    {
        // 先创建当前已有角色的对象，再监听后续进入事件
        StartCoroutine(InitGameObjects());
        CharacterManager.Instance.OnCharacterEnter = OnCharacterEnter;
    }

    private void OnDestroy()
    {
        CharacterManager.Instance.OnCharacterEnter = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCharacterEnter(Character cha)
    {
        // 收到“角色进入地图”事件：创建对应的 GameObject 表现
        CreateCharacterObject(cha);
    }

    IEnumerator InitGameObjects()
    {
        foreach (var cha in CharacterManager.Instance.Characters.Values)
        {
            CreateCharacterObject(cha);
            yield return null;
        }
    }

    private void CreateCharacterObject(Character character)
    {
        if (!Characters.ContainsKey(character.Info.Id) || Characters[character.Info.Id] == null)
        {
            // 从 Resources 加载角色预制体（资源路径由 CharacterDefine.Resource 配置）
            Object obj = Resloader.Load<Object>(character.Define.Resource);
            if (obj == null)
            {
                Log.ErrorFormat("Character[{0}] Resource[{1}] not existed.", character.Define.TID, character.Define.Resource);
                return;
            }
            GameObject go = (GameObject)Instantiate(obj);
            go.name = "Character_" + character.Info.Id + "_" + character.Info.Name;

            // 初始位置/朝向：逻辑坐标转世界坐标
            go.transform.position = GameObjectTool.LogicToWorld(character.position);
            go.transform.forward = GameObjectTool.LogicToWorld(character.direction);
            Characters[character.Info.Id] = go;

            EntityController ec = go.GetComponent<EntityController>();
            if (ec != null)
            {
                ec.entity = character;
                ec.isPlayer = character.IsPlayer;
            }

            PlayerInputController pc = go.GetComponent<PlayerInputController>();
            if (pc != null)
            {
                if (character.Info.Id == Models.User.Instance.CurrentCharacter.Id)
                {
                    // 本地玩家：启用输入，并把相机跟随指向当前角色
                    MainPlayerCamera.Instance.player = go;
                    pc.enabled = true;
                    pc.character = character;
                    pc.entityController = ec;
                }
                else
                {
                    // 非本地玩家：禁用输入组件（由同步/EntityController 驱动）
                    pc.enabled = false;
                }
            }
        }
    }
}

