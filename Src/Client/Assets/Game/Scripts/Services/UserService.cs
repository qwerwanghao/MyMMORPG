using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Network;
using UnityEngine;

using SkillBridge.Message;

namespace Services
{
    public class UserService : Singleton<UserService>, IDisposable
    {
        public UnityEngine.Events.UnityAction<Result, string> OnRegister;
        public UnityEngine.Events.UnityAction<Result, string> OnLogin;
        public UnityEngine.Events.UnityAction<Result, string> OnCreateCharacter;
        public UnityEngine.Events.UnityAction<Result, string> OnDeleteCharacter;
        NetMessage pendingMessage = null;
        bool connected = false;
        public bool IsBusy { get; private set; } = false;

        public UserService()
        {
            NetClient.Instance.OnConnect += OnGameServerConnect;
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(this.OnUserRegister);
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Subscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);
            MessageDistributer.Instance.Subscribe<UserDeleteCharacterResponse>(this.OnUserDeleteCharacter);

        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(this.OnUserRegister);
            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Unsubscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);
            MessageDistributer.Instance.Unsubscribe<UserDeleteCharacterResponse>(this.OnUserDeleteCharacter);
            NetClient.Instance.OnConnect -= OnGameServerConnect;
            NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;
        }

        public void Init()
        {

        }

        public void ConnectToServer()
        {
            Log.Info("ConnectToServer() Start ");
            //NetClient.Instance.CryptKey = this.SessionId;
            NetClient.Instance.Init("127.0.0.1", 8000);
            NetClient.Instance.Connect();
        }


        void OnGameServerConnect(int result, string reason)
        {
            Log.InfoFormat("LoadingMesager::OnGameServerConnect :{0} reason:{1}", result, reason);
            if (NetClient.Instance.Connected)
            {
                this.connected = true;
                if (this.pendingMessage != null)
                {
                    NetClient.Instance.SendMessage(this.pendingMessage);
                    this.pendingMessage = null;
                }
            }
            else
            {
                if (!this.DisconnectNotify(result, reason))
                {
                    // MessageBox.Show(string.Format("网络错误，无法连接到服务器！\n RESULT:{0} ERROR:{1}", result, reason), "错误", MessageBoxType.Error);
                }
            }
        }

        public void OnGameServerDisconnect(int result, string reason)
        {
            this.DisconnectNotify(result, reason);
            return;
        }

        bool DisconnectNotify(int result, string reason)
        {
            this.IsBusy = false;
            if (this.pendingMessage != null)
            {
                if (this.pendingMessage.Request.userRegister != null)
                {
                    if (this.OnRegister != null)
                    {
                        this.OnRegister(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                else if (this.pendingMessage.Request.userLogin != null)
                {
                    if (this.OnLogin != null)
                    {
                        this.OnLogin(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                else if (this.pendingMessage.Request.createChar != null)
                {
                    if (this.OnCreateCharacter != null)
                    {
                        this.OnCreateCharacter(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                else if (this.pendingMessage.Request.deleteChar != null)
                {
                    if (this.OnDeleteCharacter != null)
                    {
                        this.OnDeleteCharacter(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                return true;
            }
            return false;
        }

        public void SendRegister(string user, string psw)
        {
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            Log.InfoFormat("UserRegisterRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.userRegister = new UserRegisterRequest();
            message.Request.userRegister.User = user;
            message.Request.userRegister.Passward = psw;

            if (this.connected && Network.NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                Network.NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        public void SendCreateCharacter(string name, CharacterClass cls)
        {
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            Log.InfoFormat("UserCreateCharacterRequest::Name :{0} Class:{1}", name, cls);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.createChar = new UserCreateCharacterRequest();
            message.Request.createChar.Name = name;
            message.Request.createChar.Class = cls;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        public void SendDeleteCharacter(int id)
        {
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            Log.InfoFormat("UserDeleteCharacterRequest::Id :{0}", id);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.deleteChar = new UserDeleteCharacterRequest();
            message.Request.deleteChar.characterId = id;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        public void SendLogin(string user, string psw)
        {
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            Log.InfoFormat("UserLoginRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.userLogin = new UserLoginRequest();
            message.Request.userLogin.User = user;
            message.Request.userLogin.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        void OnUserRegister(object sender, UserRegisterResponse response)
        {
            Log.InfoFormat("OnUserRegister:{0} [{1}]", response.Result, response.Errormsg);

            if (this.OnRegister != null)
            {
                this.OnRegister(response.Result, response.Errormsg);
            }
            this.IsBusy = false;
        }

        void OnUserLogin(object sender, UserLoginResponse response)
        {
            Log.InfoFormat("OnUserLogin:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success)
            {
                //登录成功逻辑
                Models.User.Instance.SetupUserInfo(response.Userinfo);
            }

            if (this.OnLogin != null)
            {
                this.OnLogin(response.Result, response.Errormsg);
            }
            this.IsBusy = false;
        }

        void OnUserCreateCharacter(object sender, UserCreateCharacterResponse response)
        {
            Log.InfoFormat("OnUserCreateCharacter:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success && Models.User.Instance.Info != null && Models.User.Instance.Info.Player != null)
            {
                Models.User.Instance.Info.Player.Characters.Clear();
                if (response.Characters != null)
                {
                    Models.User.Instance.Info.Player.Characters.AddRange(response.Characters);
                }
            }

            if (this.OnCreateCharacter != null)
            {
                this.OnCreateCharacter(response.Result, response.Errormsg);
            }
            this.IsBusy = false;
        }

        void OnUserDeleteCharacter(object sender, UserDeleteCharacterResponse response)
        {
            Log.InfoFormat("OnUserDeleteCharacter:{0} [{1}]", response.Result, response.Errormsg);
            Log.InfoFormat("OnUserDeleteCharacter PayloadCount:{0}", response.Characters != null ? response.Characters.Count : 0);

            if (response.Result == Result.Success && Models.User.Instance.Info != null && Models.User.Instance.Info.Player != null)
            {
                Models.User.Instance.Info.Player.Characters.Clear();
                if (response.Characters != null)
                {
                    Models.User.Instance.Info.Player.Characters.AddRange(response.Characters);
                }
            }

            if (this.OnDeleteCharacter != null)
            {
                this.OnDeleteCharacter(response.Result, response.Errormsg);
            }
            this.IsBusy = false;
        }
    }
}
