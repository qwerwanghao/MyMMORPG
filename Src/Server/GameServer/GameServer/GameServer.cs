using System;
using System.Collections.Generic;
using System.Threading;
using Network;
using GameServer.Services;
using GameServer.Managers;

namespace GameServer
{
    /// <summary>
    /// 游戏服务器主类：管理服务生命周期和主循环
    /// </summary>
    class GameServer
    {
        #region 私有字段

        Thread thread;
        bool running = false;
        private List<IService> services = new List<IService>();

        #endregion

        #region 公共方法（服务生命周期）

        /// <summary>
        /// 初始化服务器：Managers 和 Services 两阶段初始化
        /// </summary>
        public bool Init()
        {
            // 第一阶段：初始化基础数据管理模块 (Managers)
            // Manager 是被动的数据容器，通常不需要 Update
            DataManager.Instance.Init();
            MapManager.Instance.Init();

            // 第二阶段：初始化业务逻辑服务模块 (Services)
            // Service 是主动的业务驱动者，注册到 services 列表以便统一调用 Init/Start/Stop/Update
            services.Add(DBService.Instance);
            services.Add(UserService.Instance);
            services.Add(MapService.Instance);
            services.Add(new NetService());

            foreach (var service in services)
            {
                service.Init();
            }

            thread = new Thread(new ThreadStart(this.Update));

            return true;
        }

        /// <summary>
        /// 启动服务器：启动所有服务并进入主循环
        /// </summary>
        public void Start()
        {
            foreach (var service in services)
            {
                service.Start();
            }

            running = true;
            thread.Start();
        }

        /// <summary>
        /// 停止服务器：停止主循环和所有服务
        /// </summary>
        public void Stop()
        {
            running = false;
            thread.Join();

            // 逆序停止服务
            for (int i = services.Count - 1; i >= 0; i--)
            {
                services[i].Stop();
            }
        }

        #endregion

        #region 主循环

        /// <summary>
        /// 主循环：30 FPS 固定帧率更新所有服务（供 ThreadStart 调用）
        /// </summary>
        public void Update()
        {
            const int FPS = 30;
            const int frameTime = 1000 / FPS;

            while (running)
            {
                long start = Time.ticks;

                Time.Tick();

                foreach (var service in services)
                {
                    service.Update();
                }

                MapManager.Instance.Update();

                long end = Time.ticks;
                int sleepTime = frameTime - (int)((end - start) / 10000);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        #endregion
    }
}
