using Common;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;
using System.Linq;

namespace GameServer.Services
{
    /// <summary>
    /// UserService 负责处理账号注册 / 登录 / 创角三个网关消息
    /// - 所有入口均由 MessageDistributer 分发（网络层解出 NetMessage 后调用）
    /// - 依赖 EF 上下文 DBService.Instance.Entities 进行数据库读写
    /// - 登录成功后会把 EF 的 TUser 存到 NetSession，后续请求用 session 判断登录态
    /// </summary>
    class UserService : Singleton<UserService>, IService
    {
        #region 构造函数

        /// <summary>
        /// 构造函数：注册所有网络消息处理器
        /// </summary>
        public UserService()
        {
            // 注册网络消息回调：NetConnection.PackageHandler 解出包后交给 MessageDistributer 触发对应处理
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCreateCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserDeleteCharacterRequest>(this.OnDeleteCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnGameEnter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnGameLeave);
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化（IService 接口实现）
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 启动（IService 接口实现）
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// 停止（IService 接口实现）
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// 更新（IService 接口实现）
        /// </summary>
        public void Update()
        {
        }

        #endregion

        #region 消息处理器

        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            // 0.记录请求日志
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Password);

            // 1.创建响应消息对象
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userRegister = new UserRegisterResponse();

            // 2.从数据库查询用户名是否已存在（用户名必须唯一）
            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                // 2.1 用户名已存在，返回失败响应
                message.Response.userRegister.Result = Result.Failed;
                message.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                // 3.创建新的用户账号
                // 3.1 先创建Player对象（1:1对应User），让EF生成自增主键
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());

                // 3.2 创建User对象并关联到Player
                DBService.Instance.Entities.Users.Add(new TUser()
                {
                    Username = request.User,
                    Password = request.Password,
                    Player = player
                });

                // 3.3 保存到数据库
                DBService.Instance.Entities.SaveChanges();

                // 4.构建成功响应
                message.Response.userRegister.Result = Result.Success;
                message.Response.userRegister.Errormsg = "None";
            }

            // 5.打包响应消息并发送给客户端（网络层直接发送字节，不再做额外包装）
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {
            // 0.记录请求日志
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Password);

            // 1.创建响应消息对象
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userLogin = new UserLoginResponse();

            // 2.根据用户名查询用户信息
            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();

            // 3.验证用户是否存在
            if (user == null)
            {
                // 3.1 用户不存在，返回失败响应
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "用户不存在";
            }
            // 4.验证密码是否正确
            else if (user.Password != request.Password)
            {
                // 4.1 密码错误，返回失败响应
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "密码错误";
            }
            else
            {
                // 5.验证通过，构建成功响应
                message.Response.userLogin.Result = Result.Success;
                message.Response.userLogin.Errormsg = "登录成功";

                // 5.1 创建用户信息对象
                message.Response.userLogin.Userinfo = new NUserInfo();
                // 5.2 设置用户ID（demo中直接写死为1）
                message.Response.userLogin.Userinfo.Id = 1;

                // 5.3 创建玩家信息对象
                message.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                // 5.4 设置玩家ID
                message.Response.userLogin.Userinfo.Player.Id = (int)user.Player.ID;

                // 6.将EF User对象放入Session，标记登录状态
                sender.Session.User = user;     // 后续请求可直接访问
                sender.Verified = true;         // 标记连接已通过登录校验

                // 7.构建角色列表返回给客户端
                foreach (var character in user.Player.Characters)
                {
                    NCharacterInfo charinfo = new NCharacterInfo();
                    charinfo.Id = (int)character.ID;
                    charinfo.Name = character.Name;
                    charinfo.Class = (CharacterClass)character.Class;
                    charinfo.mapId = character.MapID;
                    message.Response.userLogin.Userinfo.Player.Characters.Add(charinfo);
                }
            }

            // 8.打包响应消息并发送给客户端
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        private void OnCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest message)
        {
            // 0.记录请求日志
            Log.InfoFormat("UserCreateCharacterRequest: Name:{0}  Class:{1}", message.Name, message.Class);

            // 1.创建响应消息对象
            NetMessage response = new NetMessage();
            response.Response = new NetMessageResponse();
            response.Response.createChar = new UserCreateCharacterResponse();

            // 2.验证用户是否已登录
            if (sender.Session.User == null || sender.Session.User.Player == null)
            {
                // 2.1 用户未登录，返回失败响应
                response.Response.createChar.Result = Result.Failed;
                response.Response.createChar.Errormsg = "未登录";
            }
            // 3.检查角色名是否重复（同一Player下不能重名）
            else if (sender.Session.User.Player.Characters.Any(c => c.Name == message.Name))
            {
                // 3.1 角色名已存在，返回失败响应
                response.Response.createChar.Result = Result.Failed;
                response.Response.createChar.Errormsg = "角色名已存在";
            }
            else
            {
                // 4.创建新角色对象
                TCharacter character = new TCharacter()
                {
                    // 4.1 设置角色模板ID（未接入职业配置表，暂用Class作为TID）
                    TID = (int)message.Class,
                    // 4.2 设置角色名称
                    Name = message.Name,
                    // 4.3 设置角色职业
                    Class = (int)message.Class,
                    // 4.4 设置初始地图ID（默认为1）
                    MapID = 1,
                    // 4.5 设置初始地图坐标（默认为原点）
                    MapPosX = 5000,
                    MapPosY = 4000,
                    MapPosZ = 820,
                    // 4.6 关联到所属玩家
                    Player = sender.Session.User.Player
                };

                // 5.将新角色添加到数据库并保存
                DBService.Instance.Entities.Characters.Add(character);
                DBService.Instance.Entities.SaveChanges();

                // 6.构建成功响应
                response.Response.createChar.Result = Result.Success;
                response.Response.createChar.Errormsg = "None";

                // 7.返回当前玩家的所有角色，以便客户端刷新角色列表
                foreach (var cha in sender.Session.User.Player.Characters)
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = (int)cha.ID;
                    info.Name = cha.Name;
                    info.Class = (CharacterClass)cha.Class;
                    info.Tid = cha.TID;
                    info.Level = 1;  // 角色创建时等级为1
                    info.mapId = cha.MapID;
                    response.Response.createChar.Characters.Add(info);
                }
            }

            // 8.打包响应消息并发送给客户端
            byte[] data = PackageHandler.PackMessage(response);
            sender.SendData(data, 0, data.Length);
        }

        private void OnDeleteCharacter(NetConnection<NetSession> sender, UserDeleteCharacterRequest message)
        {
            // 0.记录请求日志
            Log.InfoFormat("UserDeleteCharacterRequest: Id:{0}", message.characterId);

            // 1.创建响应消息对象
            NetMessage response = new NetMessage();
            response.Response = new NetMessageResponse();
            response.Response.deleteChar = new UserDeleteCharacterResponse();

            // 2.验证用户是否已登录
            if (sender.Session.User == null || sender.Session.User.Player == null)
            {
                // 2.1 用户未登录，返回失败响应
                Log.Warning("UserDeleteCharacterRequest failed: not logged in");
                response.Response.deleteChar.Result = Result.Failed;
                response.Response.deleteChar.Errormsg = "未登录";
            }
            else
            {
                // 3.获取玩家对象，记录当前角色数量
                var player = sender.Session.User.Player;
                Log.InfoFormat("DeleteChar PlayerId:{0} CurrentCount:{1}", player.ID, player.Characters.Count);

                // 4.查找要删除的角色
                var target = player.Characters.FirstOrDefault(c => c.ID == message.characterId);
                if (target == null)
                {
                    // 4.1 角色不存在，返回失败响应
                    Log.WarningFormat("DeleteChar target not found: Id:{0}", message.characterId);
                    response.Response.deleteChar.Result = Result.Failed;
                    response.Response.deleteChar.Errormsg = "角色不存在";
                }
                else
                {
                    // 5.执行删除操作
                    // 5.1 记录删除的角色信息
                    Log.InfoFormat("DeleteChar removing: Id:{0} Name:{1}", target.ID, target.Name);

                    // 5.2 从玩家的角色列表中移除
                    player.Characters.Remove(target);

                    // 5.3 从数据库中删除角色记录
                    DBService.Instance.Entities.Characters.Remove(target);

                    // 5.4 保存数据库更改
                    DBService.Instance.Entities.SaveChanges();

                    // 6.构建成功响应
                    response.Response.deleteChar.Result = Result.Success;
                    response.Response.deleteChar.Errormsg = "None";

                    // 7.重新构建角色列表返回给客户端
                    foreach (var cha in player.Characters.ToList())
                    {
                        NCharacterInfo info = new NCharacterInfo();
                        info.Id = (int)cha.ID;
                        info.Name = cha.Name;
                        info.Class = (CharacterClass)cha.Class;
                        info.Tid = cha.TID;
                        info.Level = 1;
                        info.mapId = cha.MapID;
                        response.Response.deleteChar.Characters.Add(info);
                    }
                    // 8.记录删除成功后的角色数量
                    Log.InfoFormat("DeleteChar success, RemainingCount:{0}", player.Characters.Count);
                }
            }

            // 9.打包响应消息并发送给客户端
            byte[] data = PackageHandler.PackMessage(response);
            sender.SendData(data, 0, data.Length);
        }

        private void OnGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest message)
        {
            Log.InfoFormat("UserGameEnterRequest: characterIdx:{0}", message.characterIdx);

            NetMessage response = new NetMessage();
            response.Response = new NetMessageResponse();
            response.Response.gameEnter = new UserGameEnterResponse();

            if (sender.Session.User == null || sender.Session.User.Player == null)
            {
                response.Response.gameEnter.Result = Result.Failed;
                response.Response.gameEnter.Errormsg = "未登录";
            }
            else
            {
                // 1.验证角色是否存在
                TUser user = sender.Session.User;
                if (message.characterIdx < 0 || message.characterIdx >= user.Player.Characters.Count)
                {
                    response.Response.gameEnter.Result = Result.Failed;
                    response.Response.gameEnter.Errormsg = "角色不存在";
                }
                else
                {
                    // 2.获取选中的角色数据
                    TCharacter dbchar = user.Player.Characters.ElementAt(message.characterIdx);
                    Log.InfoFormat("UserGameEnterRequest: characterID:{0}:{1} Map:{2}", dbchar.ID, dbchar.Name, dbchar.MapID);
                    Character character = CharacterManager.Instance.AddCharacter(dbchar);

                    // 3.在session中记录当前激活的角色逻辑对象
                    sender.Session.Character = character;

                    // 4.填充响应数据并返回
                    response.Response.gameEnter.Result = Result.Success;
                    response.Response.gameEnter.Errormsg = "None";

                    // 5.进入地图逻辑
                    MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);
                }
            }
            byte[] data = PackageHandler.PackMessage(response);
            sender.SendData(data, 0, data.Length);
        }

        void OnGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest request)
        {
        }

        #endregion
    }
}
