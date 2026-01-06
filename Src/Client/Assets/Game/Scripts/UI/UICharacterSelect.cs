using System.Collections.Generic;
using Common;
using Models;
using Services;
using SkillBridge.Message;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSelect : MonoBehaviour
{
    public GameObject panelCreate;
    public GameObject panelSelect;
    public GameObject btnCreateCancel;
    public InputField charName;
    public Transform uiCharList;
    public GameObject uiCharInfo;
    public Image[] titles;
    public Text descs;
    public Text[] names;
    public UICharacterView characterView;

    public List<GameObject> uiChars = new List<GameObject>();

    private CharacterClass charClass;
    private int selectCharacterIdx = -1;

    private void Start()
    {
        InitCharacterSelect(true);
        UserService.Instance.OnCreateCharacter = this.OnCharacterCreate;
        UserService.Instance.OnDeleteCharacter = this.OnCharacterDelete;
        UserService.Instance.OnGameEnter = this.OnGameEnter;
    }

    private void Update()
    {
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
                charInfo.SetActive(true);
                charInfo.GetComponent<UICharInfo>().SetCharacterInfo(cha, i, this);
                uiChars.Add(charInfo);
            }

            RefreshSelection(0);
        }
    }

    public void InitCharacterCreate()
    {
        panelCreate.SetActive(true);
        panelSelect.SetActive(false);

        OnSelectClass(1);
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

    public void OnClickDeleteById(int charId)
    {
        if (Services.UserService.Instance.IsBusy)
        {
            MessageBox.Show("正在处理上一次请求，请稍候");
            return;
        }

        Log.InfoFormat("UICharacterSelect OnClickDeleteById charId:{0}", charId);
        var confirm = MessageBox.Show("确定要删除该角色吗？", "删除角色", MessageBoxType.Confirm);
        confirm.OnYes = () =>
        {
            UserService.Instance.SendDeleteCharacter(charId);
        };
    }

    public void OnClickPlay()
    {
        // 检查是否有角色
        if (User.Instance.Info.Player.Characters == null || User.Instance.Info.Player.Characters.Count == 0)
        {
            MessageBox.Show("没有可用角色，请先创建角色", "提示", MessageBoxType.Error);
            return;
        }

        // 检查是否选择了角色
        if (selectCharacterIdx < 0 || selectCharacterIdx >= User.Instance.Info.Player.Characters.Count)
        {
            MessageBox.Show("请先选择一个角色", "提示", MessageBoxType.Error);
            return;
        }

        // 检查当前角色数据是否有效
        if (User.Instance.CurrentCharacter == null)
        {
            MessageBox.Show("当前角色数据无效，请重新选择角色", "提示", MessageBoxType.Error);
            return;
        }

        // 发送进入游戏请求到服务器
        UserService.Instance.SendGameEnter(selectCharacterIdx);
    }

    private void OnCharacterCreate(Result result, string message)
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

    private void OnCharacterDelete(Result result, string message)
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

    private void OnGameEnter(Result result, string message)
    {
        if (result != Result.Success)
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    private void RefreshSelection(int idx)
    {
        if (idx < 0 || idx >= User.Instance.Info.Player.Characters.Count)
        {
            Log.WarningFormat("RefreshSelection: Invalid index {0}", idx);
            return;
        }
        this.selectCharacterIdx = idx;
        var cha = User.Instance.Info.Player.Characters[idx];
        Log.InfoFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);
        User.Instance.CurrentCharacter = cha;
        // Map selected list item to the actual character class model
        characterView.CurrectCharacter = ((int)cha.Class) - 1;
    }
}
