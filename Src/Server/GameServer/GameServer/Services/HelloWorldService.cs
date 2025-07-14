using Common;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    class HelloWorldService : Singleton<HelloWorldService>
    {
        public void Init() 
        {
            Console.WriteLine("Hello World Service Init");
        }

        public void Start()
        {
            Console.WriteLine("Hello World Service Started");
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FirstTestRequest>(this.OnFirstTestRequest);
        }

        void OnFirstTestRequest(NetConnection<NetSession> sender, FirstTestRequest request)
        {
            Console.WriteLine($"OnFirstTestRequest Helloworld: {request.Helloworld}");
        }

        public void Stop()
        {
            Console.WriteLine("Hello World Service Stopped");
        }
    }
}
