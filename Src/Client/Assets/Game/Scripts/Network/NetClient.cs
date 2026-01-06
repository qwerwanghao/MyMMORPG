using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Common;
using SkillBridge.Message;
using UnityEngine;

namespace Network
{
    public class NetClient : MonoSingleton<NetClient>
    {

        const int DEF_POLL_INTERVAL_MILLISECONDS = 100; //default network thread hold on interval
        const int DEF_TRY_CONNECT_TIMES = 3;            //default the number of retries the server
        const int DEF_RECV_BUFFER_SIZE = 64 * 1024;     //default initial buffer size of recvStream
        const int DEF_PACKAGE_HEADER_LENGTH = 4;        //default package header size
        const int DEF_SEND_PING_INTERVAL = 30;          //default send ping package interval
        const int NetConnectTimeout = 10000;    //default connect wait milliseconds
        const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   //default wait some milliseconds then show load wheel
        const int NetReconnectPeriod = 10;              //default reconnect seconds

        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;           //协议错误
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       //发送异常
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      //接受到错误数据包
        public const int NET_ERROR_ZERO_BYTE = 1002;            //收发0字节
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      //收包超时
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        //proxy超时
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      //3次连接不上
        public const int NET_ERROR_PROXY_ERROR = 1006;          //proxy重启
        public const int NET_ERROR_ON_DESTROY = 1007;           //结束的时候，关闭网络连接
        public const int NET_ERROR_ON_KICKOUT = 25;           //被踢了
        public const int NET_ERROR_REMOTE_CLOSED = 1008;
        public const int MAX_SEND_QUEUE = 200;
        private float lastConnectAttemptTime = -999f;
        public const float RECONNECT_COOLDOWN = 3f; // 秒


        /// <summary>
        /// Callback signature for connection state changes. result == 0 表示成功，非 0 为错误码；reason 为可读原因。
        /// </summary>
        public delegate void ConnectEventHandler(int result, string reason);
        /// <summary>
        /// 当等待服务端回包超时/恢复时使用的简易回调。
        /// </summary>
        public delegate void ExpectPackageEventHandler();

        /// <summary>连接完成（成功/失败）时触发。</summary>
        public event ConnectEventHandler OnConnect;
        /// <summary>连接被关闭或发生错误时触发。</summary>
        public event ConnectEventHandler OnDisconnect;
        /// <summary>等待预期回包超时时触发。</summary>
        public event ExpectPackageEventHandler OnExpectPackageTimeout;
        /// <summary>超时后又收到回包时触发。</summary>
        public event ExpectPackageEventHandler OnExpectPackageResume;

        //socket instance
        /// <summary>服务端地址（由 Init 设置）。</summary>
        private IPEndPoint address;
        /// <summary>底层 TCP Socket（连接成功后设为非阻塞）。</summary>
        private Socket clientSocket;
        /// <summary>待发送数据缓冲（从 sendQueue 序列化后写入）。</summary>
        private MemoryStream sendBuffer = new MemoryStream();
        /// <summary>接收缓冲区，供 ProcessRecv 填充后交给 PackageHandler 解析。</summary>
        private MemoryStream receiveBuffer = new MemoryStream(DEF_RECV_BUFFER_SIZE);
        /// <summary>待发送消息队列（先入先出）。</summary>
        private Queue<NetMessage> sendQueue = new Queue<NetMessage>();

        /// <summary>是否正在进行连接尝试（避免并发调用 Connect）。</summary>
        private bool connecting = false;

        /// <summary>连续连接失败次数（成功后清零）。</summary>
        private int retryTimes = 0;
        /// <summary>达到该阈值后会通知连接失败（但并不禁止后续重试）。</summary>
        private int retryTimesTotal = DEF_TRY_CONNECT_TIMES;
        /// <summary>最近一次入队发送的时间戳（可用于心跳或超时判断）。</summary>
        private float lastSendTime = 0;
        /// <summary>sendBuffer 已发送的偏移（支持分片发送）。</summary>
        private int sendOffset = 0;

        /// <summary>网络循环开关；为 false 时 Update 不做任何处理。</summary>
        public bool running { get; set; }

        /// <summary>封包/拆包工具（4 字节长度前缀 + Protobuf NetMessage）。</summary>
        public PackageHandler packageHandler = new PackageHandler(null);

        void Awake()
        {
            running = true;
        }

        /// <summary>
        /// MonoSingleton 生命周期回调：开启消息分发器在处理异常时直接抛出，便于开发期发现问题。
        /// </summary>
        protected override void OnStart()
        {
            MessageDistributer.Instance.ThrowException = true;
        }

        /// <summary>
        /// 触发连接完成事件。
        /// </summary>
        /// <param name="result">0 成功，非 0 为错误码。</param>
        /// <param name="reason">可读原因。</param>
        protected virtual void RaiseConnected(int result, string reason)
        {
            ConnectEventHandler handler = OnConnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        /// <summary>
        /// 触发断开连接事件。
        /// </summary>
        /// <param name="result">导致断开的错误码。</param>
        /// <param name="reason">附加信息。</param>
        public virtual void RaiseDisonnected(int result, string reason = "")
        {
            ConnectEventHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        /// <summary>
        /// 触发“等待回包超时”。
        /// </summary>
        protected virtual void RaiseExpectPackageTimeout()
        {
            ExpectPackageEventHandler handler = OnExpectPackageTimeout;
            if (handler != null)
            {
                handler();
            }
        }
        /// <summary>
        /// 触发“超时后恢复收到回包”。
        /// </summary>
        protected virtual void RaiseExpectPackageResume()
        {
            ExpectPackageEventHandler handler = OnExpectPackageResume;
            if (handler != null)
            {
                handler();
            }
        }

        public bool Connected
        {
            get
            {
                return (clientSocket != default(Socket)) ? clientSocket.Connected : false;
            }
        }

        /// <summary>
        /// 构造函数（由 Unity 创建）。
        /// </summary>
        public NetClient()
        {
        }

        /// <summary>
        /// 重置运行时状态：清空消息与缓冲、计时与回调。不会修改目标地址或主动重连。
        /// </summary>
        public void Reset()
        {
            MessageDistributer.Instance.Clear();
            this.sendQueue.Clear();

            this.sendOffset = 0;

            this.connecting = false;

            this.retryTimes = 0;
            this.lastSendTime = 0;

            this.OnConnect = null;
            this.OnDisconnect = null;
            this.OnExpectPackageTimeout = null;
            this.OnExpectPackageResume = null;
        }

        /// <summary>
        /// 设置要连接的服务端地址。
        /// </summary>
        /// <param name="serverIP">IPv4 地址（如 127.0.0.1）。</param>
        /// <param name="port">端口。</param>
        public void Init(string serverIP, int port)
        {
            this.address = new IPEndPoint(IPAddress.Parse(serverIP), port);
        }

        /// <summary>
        /// Connect
        /// asynchronous connect.
        /// Please use OnConnect handle connect event 
        /// </summary>
        /// <param name="retryTimes"></param>
        /// <returns></returns>
        /// <summary>
        /// 发起一次连接尝试（节流由 KeepConnect 控制）。完成后回调 OnConnect。
        /// </summary>
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)
        {
            this.lastConnectAttemptTime = Time.time;

            if (this.connecting)
            {
                return;
            }

            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }
            if (this.address == default(IPEndPoint))
            {
                throw new Exception("Please Init first.");
            }
            Log.InfoFormat("DoConnect");
            this.connecting = true;
            this.lastSendTime = 0;

            this.DoConnect();
        }

        /// <summary>
        /// 组件销毁时关闭连接。
        /// </summary>
        public void OnDestroy()
        {
            Log.InfoFormat("OnDestroy NetworkManager.");
            this.CloseConnection(NET_ERROR_ON_DESTROY);
        }

        /// <summary>
        /// 关闭 Socket，清理缓冲与队列，并发出断线事件。
        /// </summary>
        /// <param name="errCode">错误码。</param>
        public void CloseConnection(int errCode)
        {
            Log.WarningFormat("CloseConnection(), errorCode: " + errCode.ToString());
            this.connecting = false;
            if (this.clientSocket != null)
            {
                this.clientSocket.Close();
            }

            //清空缓冲区
            MessageDistributer.Instance.Clear();
            this.sendQueue.Clear();

            this.receiveBuffer.Position = 0;
            this.sendBuffer.Position = sendOffset = 0;

            switch (errCode)
            {
                case NET_ERROR_UNKNOW_PROTOCOL:
                    {
                        //致命错误，停止网络服务
                        this.running = false;
                    }
                    break;
                case NET_ERROR_FAIL_TO_CONNECT:
                case NET_ERROR_PROXY_TIMEOUT:
                case NET_ERROR_PROXY_ERROR:
                    //NetworkManager.Instance.dropCurMessage();
                    //NetworkManager.Instance.Connect();
                    break;
                //离线处理
                case NET_ERROR_ON_KICKOUT:
                case NET_ERROR_ZERO_BYTE:
                case NET_ERROR_ILLEGAL_PACKAGE:
                case NET_ERROR_SEND_EXCEPTION:
                case NET_ERROR_PACKAGE_TIMEOUT:
                case NET_ERROR_REMOTE_CLOSED:
                default:
                    this.lastSendTime = 0;
                    this.RaiseDisonnected(errCode);
                    break;
            }

        }

        //send a Protobuf message
        /// <summary>
        /// 发送一条 Protobuf NetMessage。
        /// 未连接时会先触发连接并返回，等连上后才会开始真正发送。
        /// </summary>
        public void SendMessage(NetMessage message)
        {
            if (!running)
            {
                return;
            }

            if (!this.Connected)
            {
                this.receiveBuffer.Position = 0;
                this.sendBuffer.Position = sendOffset = 0;

                this.Connect();
                Log.InfoFormat("Connect Server before Send Message!");
                return;
            }

            if (sendQueue.Count >= MAX_SEND_QUEUE)
            {
                Log.WarningFormat("Send queue overflow, dropping oldest. size={0}", sendQueue.Count);
                sendQueue.Dequeue();
            }
            sendQueue.Enqueue(message);

            if (this.lastSendTime == 0)
            {
                this.lastSendTime = Time.time;
            }
        }

        /// <summary>
        /// 执行连接流程：创建 Socket、等待连接完成、成功后切为非阻塞并触发 OnConnect。
        /// </summary>
        void DoConnect()
        {
            Log.InfoFormat("NetClient.DoConnect on " + this.address.ToString());
            try
            {
                if (this.clientSocket != null)
                {
                    this.clientSocket.Close();
                }


                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.clientSocket.Blocking = true;

                Log.InfoFormat(string.Format("Connect[{0}] to server {1}", this.retryTimes, this.address) + "\n");
                IAsyncResult result = this.clientSocket.BeginConnect(this.address, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout);
                if (success)
                {
                    this.clientSocket.EndConnect(result);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    this.CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                }
                Log.ErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)
            {
                Log.InfoFormat("DoConnect Exception:" + e.ToString() + "\n");
            }

            if (this.clientSocket.Connected)
            {
                this.retryTimes = 0;
                this.clientSocket.Blocking = false;
                this.RaiseConnected(0, "Success");
            }
            else
            {
                this.retryTimes++;
                if (this.retryTimes >= this.retryTimesTotal)
                {
                    this.RaiseConnected(1, "Cannot connect to server");
                }
            }
            this.connecting = false;
        }

        /// <summary>
        /// 连接维持器：每帧被 Update 调用，已连接返回 true；
        /// 未连接则按冷却时间触发 Connect。
        /// </summary>
        bool KeepConnect()
        {
            if (this.connecting)
                return false;
            if (this.address == null)
                return false;
            if (this.Connected)
                return true;
            if (Time.time - this.lastConnectAttemptTime < RECONNECT_COOLDOWN)
                return false;
            //if (this.retryTimes < this.retryTimesTotal)
            this.Connect();

            return false;
        }

        /// <summary>
        /// 处理接收：Poll 可读 -> Receive 到缓冲 -> 交给 PackageHandler 解析。
        /// 处理 0 字节/异常为断线，并返回 false。
        /// </summary>
        bool ProcessRecv()
        {
            bool ret = false;
            try
            {
                if (this.clientSocket.Blocking)
                {
                    Log.InfoFormat("this.clientSocket.Blocking = true\n");
                }
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Log.InfoFormat("ProcessRecv Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }

                ret = this.clientSocket.Poll(0, SelectMode.SelectRead);
                if (ret)
                {
                    int n = this.clientSocket.Receive(this.receiveBuffer.GetBuffer(), 0, this.receiveBuffer.Capacity, SocketFlags.None);
                    if (n <= 0)
                    {
                        this.CloseConnection(NET_ERROR_ZERO_BYTE);
                        return false;
                    }

                    this.packageHandler.ReceiveData(this.receiveBuffer.GetBuffer(), 0, n);

                }
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.ConnectionReset ||
                    se.SocketErrorCode == SocketError.ConnectionAborted ||
                    se.SocketErrorCode == SocketError.TimedOut)
                {
                    this.CloseConnection(NET_ERROR_REMOTE_CLOSED);
                    return false; // 已处理，避免重复 CloseConnection
                }
            }
            catch (Exception e)
            {
                Log.InfoFormat("ProcessReceive exception:" + e.ToString() + "\n");

                this.CloseConnection(NET_ERROR_ILLEGAL_PACKAGE);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 处理发送：优先把 sendBuffer 中的未发完数据写出；
        /// 为空时从 sendQueue 取消息打包写入 sendBuffer。
        /// </summary>
        bool ProcessSend()
        {
            bool ret = false;
            try
            {
                if (this.clientSocket.Blocking)
                {
                    Log.InfoFormat("this.clientSocket.Blocking = true\n");
                }
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError);
                if (error)
                {
                    Log.InfoFormat("ProcessSend Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                    return false;
                }
                ret = this.clientSocket.Poll(0, SelectMode.SelectWrite);
                if (ret)
                {
                    //sendStream exist data
                    if (this.sendBuffer.Position > this.sendOffset)
                    {
                        int bufsize = (int)(this.sendBuffer.Position - this.sendOffset);
                        int n = this.clientSocket.Send(this.sendBuffer.GetBuffer(), this.sendOffset, bufsize, SocketFlags.None);
                        if (n <= 0)
                        {
                            this.CloseConnection(NET_ERROR_ZERO_BYTE);
                            return false;
                        }
                        this.sendOffset += n;
                        if (this.sendOffset >= this.sendBuffer.Position)
                        {
                            this.sendOffset = 0;
                            this.sendBuffer.Position = 0;
                            this.sendQueue.Dequeue();//remove message when send complete
                        }
                    }
                    else
                    {
                        //fetch package from sendQueue
                        if (this.sendQueue.Count > 0)
                        {
                            NetMessage message = this.sendQueue.Peek();
                            byte[] package = PackageHandler.PackMessage(message);
                            this.sendBuffer.Write(package, 0, package.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.InfoFormat("ProcessSend exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 分发已解析的消息到订阅者。
        /// </summary>
        void ProceeMessage()
        {
            MessageDistributer.Instance.Distribute();
        }

        //Update need called once per frame
        /// <summary>
        /// Unity 每帧调用：维持连接、接收、发送、分发。
        /// </summary>
        public void Update()
        {
            if (!running)
            {
                return;
            }

            if (this.KeepConnect())
            {
                if (this.ProcessRecv())
                {
                    if (this.Connected)
                    {
                        this.ProcessSend();
                        this.ProceeMessage();
                    }
                }
            }
        }
    }
}
