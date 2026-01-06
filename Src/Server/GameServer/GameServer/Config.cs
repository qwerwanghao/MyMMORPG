using System;
using System.IO;
using Newtonsoft.Json;

namespace GameServer
{
    /// <summary>
    /// 服务器配置管理器：从 JSON 文件加载服务器和数据库配置
    /// </summary>
    class Config
    {
        #region 嵌套类型

        /// <summary>
        /// 配置数据结构（JSON 反序列化目标）
        /// </summary>
        private class ConfigData
        {
            public string ServerIP { get; set; }
            public int ServerPort { get; set; }

            public string DBServerIP { get; set; }
            public int DBServerPort { get; set; }
            public string DBUser { get; set; }
            public string DBPass { get; set; }
        }

        #endregion

        #region 私有字段

        static ConfigData config;

        #endregion

        #region 公共属性（服务器配置）

        public static string ServerIP { get { return config.ServerIP; } }
        public static int ServerPort { get { return config.ServerPort; } }

        #endregion

        #region 公共属性（数据库配置）

        public static string DBServerIP { get { return config.DBServerIP; } }
        public static int DBServerPort { get { return config.DBServerPort; } }
        public static string DBUser { get { return config.DBUser; } }
        public static string DBPass { get { return config.DBPass; } }

        #endregion

        #region 公共方法

        /// <summary>
        /// 从 JSON 文件加载配置
        /// </summary>
        public static void LoadConfig(string filename)
        {
            string json = File.ReadAllText(filename);
            config = JsonConvert.DeserializeObject<ConfigData>(json);
        }

        #endregion
    }
}
