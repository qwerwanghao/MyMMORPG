using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;
using SkillBridge.Message;
public class UICharacterSelect : MonoBehaviour
{

    public GameObject panelCreate;
    public GameObject panelSelect;

    public GameObject btnCreateCancel;

    public InputField charName;
    CharacterClass charClass;

    public Transform uiCharList;
    public GameObject uiCharInfo;

    public List<GameObject> uiChars = new List<GameObject>();

    public Image[] titles;

    public Text descs;


    public Text[] names;

    private int selectCharacterIdx = -1;
    
    private int selectCharacterFirstIdx = 0;

    public UICharacterView characterView;

    // Use this for initialization
    void Start()
    {
        InitCharacterSelect(true);
        UserService.Instance.OnCreateCharacter = this.OnCharacterCreate;
        UserService.Instance.OnDeleteCharacter = this.OnCharacterDelete;
    }


    public void InitCharacterSelect(bool init)
    {
        panelCreate.SetActive(false);
        panelSelect.SetActive(true);

        if (init)
        {
            foreach (var old in uiChars)
            {
                Destroy(old);
            }
            uiChars.Clear();

            for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++)
            {
                var cha = User.Instance.Info.Player.Characters[i];
                var charInfo = Instantiate(uiCharInfo, uiCharList);
                Button btn = charInfo.GetComponent<Button>();
                int idx = i;
                btn.onClick.AddListener(() =>
                    {
                        RefreshSelection(idx);
                    });
                RefreshSelection(selectCharacterFirstIdx);
                charInfo.SetActive(true);
                charInfo.GetComponent<UICharInfo>().SetCharacterInfo(cha, i, this);
                uiChars.Add(charInfo);
            }
        }
    }

    public void InitCharacterCreate()
    {
        panelCreate.SetActive(true);
        panelSelect.SetActive(false);

        OnSelectClass(1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickCreate()
    {
        var name = this.charName.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("请输入角色名");
            return;
        }

        if (Services.UserService.Instance.IsBusy)
        {
            MessageBox.Show("正在处理上一次请求，请稍候");
            return;
        }

        UserService.Instance.SendCreateCharacter(name, this.charClass);
    }

    public void OnSelectClass(int charClass)
    {
        this.charClass = (CharacterClass)charClass;

        characterView.CurrectCharacter = charClass - 1;

        for (int i = 0; i < 3; i++)
        {
            titles[i].gameObject.SetActive(i == charClass - 1);
            names[i].text = DataManager.Instance.Characters[i + 1].Name;
        }

        descs.text = DataManager.Instance.Characters[charClass].Description;
    }


    void OnCharacterCreate(Result result, string message)
    {
        if (result == Result.Success)
        {
            InitCharacterSelect(true);

            this.charName.text = "";
            MessageBox.Show("角色创建成功！");
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    void OnCharacterDelete(Result result, string message)
    {
        if (result == Result.Success)
        {
            InitCharacterSelect(true);
            this.selectCharacterIdx = -1;
            User.Instance.CurrentCharacter = null;
            MessageBox.Show("角色删除成功！");
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    public void RefreshSelection(int idx)
    {
        this.selectCharacterIdx = idx;
        var cha = User.Instance.Info.Player.Characters[idx];
        Debug.LogFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);
        User.Instance.CurrentCharacter = cha;
        // Map selected list item to the actual character class model
        characterView.CurrectCharacter = ((int)cha.Class) - 1;
    }

    public void OnClickDeleteById(int charId)
    {
        if (Services.UserService.Instance.IsBusy)
        {
            MessageBox.Show("正在处理上一次请求，请稍候");
            return;
        }

        Debug.LogFormat("UICharacterSelect OnClickDeleteById charId:{0}", charId);
        var confirm = MessageBox.Show("确定要删除该角色吗？", "删除角色", MessageBoxType.Confirm);
        confirm.OnYes = () =>
        {
            UserService.Instance.SendDeleteCharacter(charId);
        };
    }
    public void OnClickPlay()
    {
        if (selectCharacterIdx >= 0)
        {
            MessageBox.Show("进入游戏", "进入游戏", MessageBoxType.Confirm);
        }
    }
}
