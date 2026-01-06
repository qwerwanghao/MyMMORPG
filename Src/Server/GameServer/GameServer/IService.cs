namespace GameServer
{
    /// <summary>
    /// 服务接口：定义服务的生命周期方法（Init/Start/Stop/Update）
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 初始化服务
        /// </summary>
        void Init();

        /// <summary>
        /// 启动服务
        /// </summary>
        void Start();

        /// <summary>
        /// 停止服务
        /// </summary>
        void Stop();

        /// <summary>
        /// 更新服务（每帧调用）
        /// </summary>
        void Update();
    }
}
