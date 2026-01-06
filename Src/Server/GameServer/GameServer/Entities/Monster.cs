using GameServer.Core;
using SkillBridge.Message;

namespace GameServer.Entities
{
    /// <summary>
    /// 怪物类：继承自 CharacterBase，表示游戏中的怪物实例
    /// </summary>
    class Monster : CharacterBase
    {
        /// <summary>
        /// 创建怪物实例
        /// </summary>
        /// <param name="tid">怪物模板ID</param>
        /// <param name="level">怪物等级</param>
        /// <param name="pos">生成位置</param>
        /// <param name="dir">朝向</param>
        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir)
            : base(CharacterType.Monster, tid, level, pos, dir)
        {
        }
    }
}
