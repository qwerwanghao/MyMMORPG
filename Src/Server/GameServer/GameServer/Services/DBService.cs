using Common;

namespace GameServer.Services
{
    /// <summary>
    /// DBService：数据库服务，管理 Entity Framework 上下文
    /// </summary>
    class DBService : Singleton<DBService>, IService
    {
        #region 私有字段

        private ExtremeWorldEntities entities;

        #endregion

        #region 公共属性

        /// <summary>
        /// Entity Framework 数据库上下文
        /// </summary>
        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        #endregion

        #region 生命周期方法

        /// <summary>
        /// 初始化：创建 Entity Framework 上下文
        /// </summary>
        public void Init()
        {
            entities = new ExtremeWorldEntities();
        }

        /// <summary>
        /// 启动（IService 接口实现）
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// 停止（IService 接口实现）
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// 更新（IService 接口实现）
        /// </summary>
        public void Update()
        {
        }

        #endregion
    }
}
