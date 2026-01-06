using System;
using Common;
using Network;
using SkillBridge.Message;
using UnityEngine;

namespace Services
{
    public class UserService : Singleton<UserService>, IDisposable
    {
        public UnityEngine.Events.UnityAction<Result, string> OnRegister;
        public UnityEngine.Events.UnityAction<Result, string> OnLogin;
        public UnityEngine.Events.UnityAction<Result, string> OnCreateCharacter;
        public UnityEngine.Events.UnityAction<Result, string> OnDeleteCharacter;
        public UnityEngine.Events.UnityAction<Result, string> OnGameEnter;
        public UnityEngine.Events.UnityAction<Result, string> OnGameLeave;
        public bool IsBusy { get; private set; } = false;

        private NetMessage pendingMessage = null;
        private bool connected = false;
        private bool initialized = false;

        public UserService()
        {
        }

        public void Dispose()
        {
            if (!this.initialized)
            {
                return;
            }

            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(this.OnUserRegister);
            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Unsubscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);
            MessageDistributer.Instance.Unsubscribe<UserDeleteCharacterResponse>(this.OnUserDeleteCharacter);
            MessageDistributer.Instance.Unsubscribe<UserGameEnterResponse>(this.OnUserGameEnter);
            MessageDistributer.Instance.Unsubscribe<UserGameLeaveResponse>(this.OnUserGameLeave);
            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterRequest>(this.OnMapCharacterEnter);

            if (NetClient.Instance != null)
            {
                NetClient.Instance.OnConnect -= OnGameServerConnect;
                NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;
            }

            this.initialized = false;
        }

        public void Run()
        {
            if (this.initialized)
            {
                return;
            }

            // Ensure NetClient exists (MonoSingleton requires a scene object)
            if (NetClient.Instance == null)
            {
                var go = new GameObject("NetClient");
                go.AddComponent<NetClient>();
            }

            NetClient.Instance.OnConnect += OnGameServerConnect;
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(this.OnUserRegister);
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Subscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);
            MessageDistributer.Instance.Subscribe<UserDeleteCharacterResponse>(this.OnUserDeleteCharacter);
            MessageDistributer.Instance.Subscribe<UserGameEnterResponse>(this.OnUserGameEnter);
            MessageDistributer.Instance.Subscribe<UserGameLeaveResponse>(this.OnUserGameLeave);
            MessageDistributer.Instance.Subscribe<MapCharacterEnterRequest>(this.OnMapCharacterEnter);

            this.initialized = true;
        }

        public void ConnectToServer()
        {
            Log.Info("ConnectToServer() Start ");
            //NetClient.Instance.CryptKey = this.SessionId;

            string host = "127.0.0.1";
            int port = 8000;

            if (DataManager.Instance != null && DataManager.Instance.Config != null)
            {
                if (!string.IsNullOrEmpty(DataManager.Instance.Config.ServerHost))
                {
                    host = DataManager.Instance.Config.ServerHost;
                }
                if (DataManager.Instance.Config.ServerPort > 0)
                {
                    port = DataManager.Instance.Config.ServerPort;
                }
            }
            else
            {
                Log.Warning("GameServerConfig not loaded; use default server endpoint 127.0.0.1:8000");
            }

            NetClient.Instance.Init(host, port);
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
                else if (this.pendingMessage.Request.gameEnter != null)
                {
                    if (this.OnGameEnter != null)
                    {
                        this.OnGameEnter(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
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
            message.Request.userRegister.Password = psw;

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

        public void SendGameEnter(int characterIdx)
        {
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            Log.InfoFormat("UserGameEnterRequest::characterIdx :{0}", characterIdx);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.gameEnter = new UserGameEnterRequest();
            message.Request.gameEnter.characterIdx = characterIdx;

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

        public void SendGameLeave()
        {
            Log.Info("UserGameLeaveRequest");
            if (this.IsBusy)
            {
                Log.Warning("UserService busy; ignore duplicate submit.");
                return;
            }
            this.IsBusy = true;

            try
            {
                // 空检查：检查 NetClient.Instance 是否为 null
                if (NetClient.Instance == null)
                {
                    Log.Error("SendGameLeave failed: NetClient.Instance is null");
                    this.OnGameLeave?.Invoke(Result.Failed, "网络客户端未初始化");
                    return;
                }

                // 连接状态检查：检查网络是否已连接
                if (!this.connected || !NetClient.Instance.Connected)
                {
                    Log.Warning("SendGameLeave failed: Not connected to server");
                    this.OnGameLeave?.Invoke(Result.Failed, "未连接到服务器");
                    return;
                }

                // 构建并发送消息
                NetMessage message = new NetMessage();
                message.Request = new NetMessageRequest();
                message.Request.gameLeave = new UserGameLeaveRequest();
                NetClient.Instance.SendMessage(message);
            }
            catch (Exception ex)
            {
                // 异常处理：捕获网络异常，防止程序崩溃
                Log.ErrorFormat("SendGameLeave exception: {0}", ex.Message);
                this.OnGameLeave?.Invoke(Result.Failed, "发送离开请求时发生异常");
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
            message.Request.userLogin.Password = psw;

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

        void OnUserGameEnter(object sender, UserGameEnterResponse response)
        {
            Log.InfoFormat("OnUserGameEnter:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success)
            {
                Log.InfoFormat("进入游戏成功！");
            }
            else
            {
                Log.InfoFormat("进入游戏失败：{0}", response.Errormsg);
            }

            this.OnGameEnter?.Invoke(response.Result, response.Errormsg);

            this.IsBusy = false;
        }

        void OnUserGameLeave(object sender, UserGameLeaveResponse response)
        {
            //MapService.Instance.CurrentMapId = 0;
            Log.InfoFormat("OnUserGameLeave:{0} [{1}]", response.Result, response.Errormsg);

            if (this.OnGameLeave != null)
            {
                this.OnGameLeave(response.Result, response.Errormsg);
            }
            this.IsBusy = false;
        }

        void OnMapCharacterEnter(object sender, MapCharacterEnterRequest request)
        {
            Log.InfoFormat("OnMapCharacterEnter: characterId:{0}", request.mapId);

        }
    }
}
