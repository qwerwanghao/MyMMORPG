using UnityEngine;

public class Login : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Network.NetClient.Instance.Init("127.0.0.1", 8000);
        Network.NetClient.Instance.Connect();
        
        SkillBridge.Message.NetMessage msg = new SkillBridge.Message.NetMessage();
        msg.Request = new SkillBridge.Message.NetMessageRequest();
        msg.Request.firstRequest = new SkillBridge.Message.FirstTestRequest();
        msg.Request.firstRequest.Helloworld = "Hello World!";
        Network.NetClient.Instance.SendMessage(msg);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
