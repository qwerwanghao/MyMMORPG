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
using System.Diagnostics;

namespace GameServer
{
    class GameServer
    {
        Thread thread;
        bool running = false;
        NetService network;

        public bool Init()
        {
            network = new NetService();
            network.Init(8000);

            HelloWorldService.Instance.Init();
            //DBService.Instance.Init();
            //var a = DBService.Instance.Entities.Characters.Where(s => s.TID == 1);
            //var character = a.FirstOrDefault<TCharacter>();
            //if (character != null)
            //{
            //    Console.WriteLine("{0}", character.Name);
            //}
            //else
            //{
            //    Console.WriteLine("Character not found.");
            //}
            thread = new Thread(new ThreadStart(this.Update));

            return true;
        }

        public void Start()
        {
            network.Start();
            running = true;
            thread.Start();
            HelloWorldService.Instance.Start();
        }


        public void Stop()
        {
            network.Stop();
            running = false;
            thread.Join();
        }

        public void Update()
        {
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
            }
        }
    }
}
