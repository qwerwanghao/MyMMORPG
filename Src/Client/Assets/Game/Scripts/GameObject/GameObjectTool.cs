using SkillBridge.Message;
using UnityEngine;

public class GameObjectTool
{
    /// <summary>
    /// 坐标体系转换工具：
    /// - Logic（协议/服务器）通常用 int，单位为 1/100（即 100 = 1m）；且 Y/Z 轴和 Unity 有映射关系。
    /// - Unity 世界坐标用 float（米）。
    /// </summary>
    public static Vector3 LogicToWorld(NVector3 vector)
    {
        return new Vector3(vector.X / 100f, vector.Z / 100f, vector.Y / 100f);
    }

    public static Vector3 LogicToWorld(Vector3Int vector)
    {
        return new Vector3(vector.x / 100f, vector.z / 100f, vector.y / 100f);
    }

    public static float LogicToWorld(int val)
    {
        return val / 100f;
    }

    public static int WorldToLogic(float val)
    {
        return Mathf.RoundToInt(val * 100f);
    }

    public static NVector3 WorldToLogicN(Vector3 vector)
    {
        return new NVector3()
        {
            X = Mathf.RoundToInt(vector.x * 100),
            Y = Mathf.RoundToInt(vector.z * 100),
            Z = Mathf.RoundToInt(vector.y * 100)
        };
    }

    public static Vector3Int WorldToLogic(Vector3 vector)
    {
        return new Vector3Int()
        {
            x = Mathf.RoundToInt(vector.x * 100),
            y = Mathf.RoundToInt(vector.z * 100),
            z = Mathf.RoundToInt(vector.y * 100)
        };
    }

    public static bool EntityUpdate(NEntity entity, Vector3 position, Quaternion rotation, float speed)
    {
        // 将 Unity Transform 数据转换成协议坐标，检查是否有变化，有变化才写回（减少同步频率/带宽）。
        NVector3 pos = WorldToLogicN(position);
        NVector3 dir = WorldToLogicN(rotation.eulerAngles);
        int spd = WorldToLogic(speed);
        bool updated = false;
        if (!entity.Position.Equal(pos))
        {
            entity.Position = pos;
            updated = true;
        }
        if (!entity.Direction.Equal(dir))
        {
            entity.Direction = dir;
            updated = true;
        }
        if (entity.Speed != spd)
        {
            entity.Speed = spd;
            updated = true;
        }
        return updated;
    }
}
