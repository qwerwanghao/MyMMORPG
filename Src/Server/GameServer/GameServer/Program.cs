using System;
using System.IO;
using Common;

namespace GameServer
{
    /// <summary>
    /// 游戏服务器入口类
    /// </summary>
    class Program
    {
        #region 入口方法

        /// <summary>
        /// 应用程序主入口：初始化日志 → 启动服务器 → 等待命令 → 退出清理
        /// </summary>
        static void Main(string[] args)
        {
            // 初始化日志系统
            FileInfo fi = new FileInfo("log4net.xml");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
            Log.Init("GameServer");
            Log.Info("Game Server Init");

            // 启动服务器
            GameServer server = new GameServer();
            server.Init();
            server.Start();
            Console.WriteLine("Game Server Running......");

            // 进入命令行循环（阻塞直到 exit）
            CommandHelper.Run();

            // 退出清理
            Log.Info("Game Server Exiting...");
            server.Stop();
            Log.Info("Game Server Exited");
        }

        #endregion
    }
}
