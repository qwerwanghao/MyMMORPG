using System;

namespace GameServer
{
    /// <summary>
    /// 命令行助手：处理服务器控制台命令（exit/help）
    /// </summary>
    class CommandHelper
    {
        #region 公共方法

        /// <summary>
        /// 启动命令行循环处理
        /// </summary>
        public static void Run()
        {
            bool run = true;
            while (run)
            {
                Console.Write(">");
                string line = Console.ReadLine();
                switch (line.ToLower().Trim())
                {
                    case "exit":
                        run = false;
                        break;
                    default:
                        Help();
                        break;
                }
            }
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        public static void Help()
        {
            Console.Write(@"
Help:
    exit    Exit Game Server
    help    Show Help
");
        }

        #endregion
    }
}
