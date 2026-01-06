using SkillBridge.Message;
using UnityEngine;
using UnityEngine.UI;

public class UICharInfo : MonoBehaviour
{
    public NCharacterInfo info;

    public Text charClass;
    public Text charName;
    public Button deleteButton;

    private UICharacterSelect characterSelect;

    internal void SetCharacterInfo(NCharacterInfo cha, int i, UICharacterSelect uICharacterSelect)
    {
        this.info = cha;
        this.charClass.text = cha.Class.ToString();
        this.charName.text = cha.Name;
        this.characterSelect = uICharacterSelect;

        if (deleteButton == null)
        {
            // 预制体上命名为 ButtonDelete
            deleteButton = transform.Find("ButtonDelete")?.GetComponent<Button>();
        }
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                if (this.info != null && this.characterSelect != null)
                {
                    this.characterSelect.OnClickDeleteById(this.info.Id);
                }
            });
        }
    }

    private void Start()
    {
        if (info != null)
        {
            this.charClass.text = this.info.Class.ToString();
            this.charName.text = this.info.Name;
        }
    }

    private void Update()
    {
    }
}
