using Services;
using UnityEngine;
using UnityEngine.UI;

public class UIRegister : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public InputField passwordConfirm;
    public Button buttonRegister;
    public GameObject loginPanel;

    private void Start()
    {
        UserService.Instance.OnRegister = this.OnRegister;
    }

    private void Update()
    {
    }

    public void OnClickRegister()
    {
        if (string.IsNullOrEmpty(this.username.text))
        {
            MessageBox.Show("请输入账号");
            return;
        }
        if (string.IsNullOrEmpty(this.password.text))
        {
            MessageBox.Show("请输入密码");
            return;
        }
        if (string.IsNullOrEmpty(this.passwordConfirm.text))
        {
            MessageBox.Show("请输入确认密码");
            return;
        }
        if (this.password.text != this.passwordConfirm.text)
        {
            MessageBox.Show("两次输入的密码不一致");
            return;
        }
        UserService.Instance.SendRegister(this.username.text, this.password.text);
    }

    private void OnRegister(SkillBridge.Message.Result result, string msg)
    {
        // 显示注册结果
        MessageBox.Show(string.Format("结果：{0} msg:{1}", result, msg));
        if (result == SkillBridge.Message.Result.Success)
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
                this.gameObject.SetActive(false);
                var login = loginPanel.GetComponent<UILogin>();
                if (login != null)
                {
                    login.SetLoginInfo(this.username.text, this.password.text);
                }
            }
        }
        // 清空输入框
        this.username.text = "";
        this.password.text = "";
        this.passwordConfirm.text = "";
    }
}
