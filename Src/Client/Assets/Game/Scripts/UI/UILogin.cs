using Services;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    public InputField username;
    public InputField password;
    public Button buttonLogin;

    private void Start()
    {
        UserService.Instance.OnLogin = this.OnLogin;
    }

    private void Update()
    {
    }

    public void OnClickLogin()
    {
        var user = username.text.Trim();
        var pass = password.text.Trim();

        if (string.IsNullOrWhiteSpace(user))
        {
            MessageBox.Show("è¯·è¾“å…¥è´¦å?");
            return;
        }
        if (string.IsNullOrWhiteSpace(pass))
        {
            MessageBox.Show("è¯·è¾“å…¥å¯†ç ?");
            return;
        }

        if (Services.UserService.Instance.IsBusy)  // ç¬?æ­¥ä¼šåŠ å…¥ IsBusy
        {
            MessageBox.Show("æ­£åœ¨å¤„ç†ä¸Šä¸€æ¬¡è¯·æ±‚ï¼Œè¯·ç¨å€™â€?");
            return;
        }

        buttonLogin.interactable = false;
        Services.UserService.Instance.SendLogin(user, pass);
    }

    public void OnClickOpenRegister()
    {
        // æ¸…ç©ºè¾“å…¥æ¡?
        this.username.text = "";
        this.password.text = "";
    }

    public void SetLoginInfo(string user, string pwd)
    {
        this.username.text = user;
        this.password.text = pwd;
    }

    private void OnLogin(SkillBridge.Message.Result result, string msg)
    {
        buttonLogin.interactable = true;
        MessageBox.Show(string.Format("ç™»å½•ç»“æžœï¼š{0} æ¶ˆæ¯:{1}", result, msg));

        if (result == SkillBridge.Message.Result.Success)
        {
            // ç™»å½•æˆåŠŸåŽçš„æ“ä½œï¼Œè·³è½¬è§’è‰²é€‰æ‹©ç•Œé¢
            SceneManager.Instance.LoadScene("CharSelect");
        }
    }
}
