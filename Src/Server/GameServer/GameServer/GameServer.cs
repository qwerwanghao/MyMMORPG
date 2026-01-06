using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

using System.Threading;

using Network;
using GameServer.Services;
using GameServer.Managers;

namespace GameServer
{
    class GameServer
    {
        Thread thread;
        bool running = false;
        private List<IService> services = new List<IService>();

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

        public void Start()
        {
            foreach (var service in services)
            {
                service.Start();
            }

            running = true;
            thread.Start();
        }


        public void Stop()
        {
            running = false;
            thread.Join();

            for (int i = services.Count - 1; i >= 0; i--)
            {
                services[i].Stop();
            }
        }

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
    }
}
