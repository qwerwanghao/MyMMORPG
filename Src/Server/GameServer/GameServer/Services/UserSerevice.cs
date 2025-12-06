using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;

namespace GameServer.Services
{
    /// <summary>
    /// UserService 负责处理账号注册 / 登录 / 创角三个网关消息。
    /// - 所有入口均由 MessageDistributer 分发（网络层解出 NetMessage 后调用）。
    /// - 依赖 EF 上下文 DBService.Instance.Entities 进行数据库读写。
    /// - 登录成功后会把 EF 的 TUser 存到 NetSession，后续请求用 session 判断登录态。
    /// </summary>
    class UserService : Singleton<UserService>
    {

        public UserService()
        {
            // 注册网络消息回调：NetConnection.PackageHandler 解出包后交给 MessageDistributer 触发对应处理。
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCreateCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserDeleteCharacterRequest>(this.OnDeleteCharacter);
        }

        public void Init()
        {

        }

        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)
        {
            // 流程：校验重名 → 写库（User + Player）→ 回包
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userRegister = new UserRegisterResponse();

            // 从数据库查重：用户名唯一
            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)
            {
                message.Response.userRegister.Result = Result.Failed;
                message.Response.userRegister.Errormsg = "用户已存在.";
            }
            else
            {
                // 创建 Player（1:1 对应 User），让 EF 生成自增主键
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());
                DBService.Instance.Entities.Users.Add(new TUser() { Username = request.User, Password = request.Passward, Player = player });
                DBService.Instance.Entities.SaveChanges();
                message.Response.userRegister.Result = Result.Success;
                message.Response.userRegister.Errormsg = "None";
            }

            // 回写 NetMessage：网络层直接发送字节，不再做额外包装
            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)
        {
            // 流程：校验账号/密码 → 构造 NUserInfo（含角色列表）→ 把 EF User 绑定进 Session 标记登录态
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Passward);

            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.userLogin = new UserLoginResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user == null)
            {
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "用户不存在";
            }
            else if (user.Password != request.Passward)
            {
                message.Response.userLogin.Result = Result.Failed;
                message.Response.userLogin.Errormsg = "密码错误";
            }
            else
            {
                message.Response.userLogin.Result = Result.Success;
                message.Response.userLogin.Errormsg = "登录成功";
                message.Response.userLogin.Userinfo = new NUserInfo();
                message.Response.userLogin.Userinfo.Id = 1; // 协议里保留的用户 Id，这里 demo 直接写死
                message.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                message.Response.userLogin.Userinfo.Player.Id = (int)user.Player.ID;
                sender.Session.User = user;     // 将 EF User 放入 session，后续请求可直接访问
                sender.Verified = true;         // 标记连接已通过登录校验
                foreach (var character in user.Player.Characters)
                {
                    NCharacterInfo charinfo = new NCharacterInfo();
                    charinfo.Id = (int)character.ID;
                    charinfo.Name = character.Name;
                    charinfo.Class = (CharacterClass)character.Class;
                    // 目前只返回少量字段（没有地图坐标等），客户端仅用于展示列表
                    message.Response.userLogin.Userinfo.Player.Characters.Add(charinfo);
                }
            }

            byte[] data = PackageHandler.PackMessage(message);
            sender.SendData(data, 0, data.Length);
        }

        private void OnCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest message)
        {
            // 流程：校验登录态 → 校验重名（同一 Player 下不重复）→ 创建 TCharacter → 回传最新角色列表
            Log.InfoFormat("UserCreateCharacterRequest: Name:{0}  Class:{1}", message.Name, message.Class);

            NetMessage response = new NetMessage();
            response.Response = new NetMessageResponse();
            response.Response.createChar = new UserCreateCharacterResponse();

            if (sender.Session.User == null || sender.Session.User.Player == null)
            {
                response.Response.createChar.Result = Result.Failed;
                response.Response.createChar.Errormsg = "未登录";
            }
            else if (sender.Session.User.Player.Characters.Any(c => c.Name == message.Name))
            {
                response.Response.createChar.Result = Result.Failed;
                response.Response.createChar.Errormsg = "角色名已存在";
            }
            else
            {
                TCharacter character = new TCharacter()
                {
                    // 这里未接入职业配置表，暂用 Class 作为 TID；若有模板表可替换为真实 tid。
                    TID = (int)message.Class,
                    Name = message.Name,
                    Class = (int)message.Class,
                    MapID = 1,
                    MapPosX = 0,
                    MapPosY = 0,
                    MapPosZ = 0,
                    Player = sender.Session.User.Player
                };

                DBService.Instance.Entities.Characters.Add(character);
                DBService.Instance.Entities.SaveChanges();

                response.Response.createChar.Result = Result.Success;
                response.Response.createChar.Errormsg = "None";
                // 返回当前玩家的所有角色，以便客户端刷新列表
                foreach (var cha in sender.Session.User.Player.Characters)
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = (int)cha.ID;
                    info.Name = cha.Name;
                    info.Class = (CharacterClass)cha.Class;
                    info.Tid = cha.TID;
                    info.Level = 1;
                    info.mapId = cha.MapID;
                    response.Response.createChar.Characters.Add(info);
                }
            }

            byte[] data = PackageHandler.PackMessage(response);
            sender.SendData(data, 0, data.Length);
        }

        private void OnDeleteCharacter(NetConnection<NetSession> sender, UserDeleteCharacterRequest message)
        {
            // 校验登录态后删除指定角色，并返回最新角色列表
            Log.InfoFormat("UserDeleteCharacterRequest: Id:{0}", message.characterId);

            NetMessage response = new NetMessage();
            response.Response = new NetMessageResponse();
            response.Response.deleteChar = new UserDeleteCharacterResponse();

            if (sender.Session.User == null || sender.Session.User.Player == null)
            {
                Log.Warning("UserDeleteCharacterRequest failed: not logged in");
                response.Response.deleteChar.Result = Result.Failed;
                response.Response.deleteChar.Errormsg = "未登录";
            }
            else
            {
                var player = sender.Session.User.Player;
                Log.InfoFormat("DeleteChar PlayerId:{0} CurrentCount:{1}", player.ID, player.Characters.Count);
                var target = player.Characters.FirstOrDefault(c => c.ID == message.characterId);
                if (target == null)
                {
                    Log.WarningFormat("DeleteChar target not found: Id:{0}", message.characterId);
                    response.Response.deleteChar.Result = Result.Failed;
                    response.Response.deleteChar.Errormsg = "角色不存在";
                }
                else
                {
                    Log.InfoFormat("DeleteChar removing: Id:{0} Name:{1}", target.ID, target.Name);
                    player.Characters.Remove(target);
                    DBService.Instance.Entities.Characters.Remove(target);
                    DBService.Instance.Entities.SaveChanges();

                    response.Response.deleteChar.Result = Result.Success;
                    response.Response.deleteChar.Errormsg = "None";

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
                    Log.InfoFormat("DeleteChar success, RemainingCount:{0}", player.Characters.Count);
                }
            }

            byte[] data = PackageHandler.PackMessage(response);
            sender.SendData(data, 0, data.Length);
        }
    }
}
