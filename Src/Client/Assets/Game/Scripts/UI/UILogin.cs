using UnityEngine;
using UnityEngine.UI;
using Services;

public class UILogin : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public Button buttonLogin;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UserService.Instance.OnLogin = this.OnLogin;
    }

    void OnLogin(SkillBridge.Message.Result result, string msg)
    {
        buttonLogin.interactable = true;
        MessageBox.Show(string.Format("登录结果：{0} 消息:{1}", result, msg));

        if (result == SkillBridge.Message.Result.Success)
        {
            // 登录成功后的操作，跳转角色选择界面
            SceneManager.Instance.LoadScene("CharSelect");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClickLogin()
    {
        var user = username.text.Trim();
        var pass = password.text.Trim();

        if (string.IsNullOrWhiteSpace(user))
        {
            MessageBox.Show("请输入账号");
            return;
        }
        if (string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("请输入密码");
            return;
        }

        if (Services.UserService.Instance.IsBusy)  // 第4步会加入 IsBusy
        {
            MessageBox.Show("正在处理上一次请求，请稍候…");
            return;
        }

        buttonLogin.interactable = false;
        Services.UserService.Instance.SendLogin(user, pass);
    }

    public void OnClickOpenRegister()
    {
        // 清空输入框
        this.username.text = "";
        this.password.text = "";
    }

    public void SetLoginInfo(string user, string pwd)
    {
        this.username.text = user;
        this.password.text = pwd;
    }
}
