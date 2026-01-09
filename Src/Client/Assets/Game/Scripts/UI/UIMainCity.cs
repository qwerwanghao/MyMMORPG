using SkillBridge.Message;
using UnityEngine;
using UnityEngine.UI;
using Common;

public class UIMainCity : MonoBehaviour
{
    [SerializeField]
    private Text playerNameText;
    [SerializeField]
    private Text playerLevelText;

    void Start()
    {
        UpdatePlayerInfo();
    }

    void Update()
    {

    }

    /// <summary>
    /// 更新玩家信息显示
    /// </summary>
    private void UpdatePlayerInfo()
    {
        if (Models.User.Instance?.CurrentCharacter != null)
        {
            NCharacterInfo characterInfo = Models.User.Instance.CurrentCharacter;

            if (playerNameText != null)
            {
                playerNameText.text = string.Format("{0} [#{1}]", characterInfo.Name, characterInfo.Id);
            }

            if (playerLevelText != null)
            {
                playerLevelText.text = $"{characterInfo.Level}";
            }

            Log.InfoFormat("UIMainCity: 玩家名称={0}, 等级={1}", characterInfo.Name, characterInfo.Level);
        }
        else
        {
            Log.Warning("UIMainCity: 当前角色信息为空");
        }
    }
}
