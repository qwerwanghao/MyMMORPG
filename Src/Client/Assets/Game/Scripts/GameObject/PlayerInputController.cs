using System;
using Entities;
using SkillBridge.Message;
using UnityEngine;

/// <summary>
/// PlayerInputController：本地玩家输入驱动器。
/// - 读取 Unity Input（Horizontal/Vertical/Jump）。
/// - 驱动 Character 的逻辑状态（speed/direction/position），并通知 EntityController 播放动画。
/// - 同时直接控制 Rigidbody 产生移动（本地玩家的"权威移动"）。
/// </summary>
public class PlayerInputController : MonoBehaviour
{
    #region Unity 可配置字段
    // 供 Inspector 配置的公共属性（组件引用/参数）

    public Rigidbody rb;
    public Character character;
    public EntityController entityController;
    public float rotateSpeed = 2.0f;
    public float turnAngle = 1;
    public int speed;
    public bool onAir = false;

    #endregion

    #region 内部状态
    // 运行时私有状态（角色状态/位置记录）

    private CharacterState state;
    private Vector3 lastPos;

    #endregion

    #region Unity 生命周期方法
    // Start：初始化 / FixedUpdate：物理更新 / LateUpdate：动画同步

    private void Start()
    {
        state = CharacterState.Idle;
        if (this.character == null)
        {
            // 兜底/测试逻辑：当未绑定角色时，创建一个临时角色用于本地调试。
            DataManager.Instance.Load();
            NCharacterInfo cinfo = new NCharacterInfo();
            cinfo.Id = 1;
            cinfo.Name = "Test";
            cinfo.Tid = 1;
            cinfo.Entity = new NEntity();
            cinfo.Entity.Position = new NVector3();
            cinfo.Entity.Direction = new NVector3();
            cinfo.Entity.Direction.X = 0;
            cinfo.Entity.Direction.Y = 100;
            cinfo.Entity.Direction.Z = 0;
            this.character = new Character(cinfo);

            if (entityController != null)
                entityController.entity = this.character;
        }
    }

    private void FixedUpdate()
    {
        if (character == null)
            return;

        // 前后移动：更新角色逻辑速度 + 发送动画事件；并用 Rigidbody 推动实际移动
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (v > 0.01)
        {
            if (state != CharacterState.Move)
            {
                state = CharacterState.Move;
                this.character.MoveForward();
                this.SendEntityEvent(EntityEvent.MoveFwd);
            }
            // 前进：使用当前实际朝向，使用速度的绝对值
            float absSpeed = Math.Abs(this.character.speed);
            Vector3 moveDir = this.transform.forward * (absSpeed + 9.81f) / 100f;
            this.rb.linearVelocity = this.rb.linearVelocity.y * Vector3.up + moveDir;
        }
        else if (v < -0.01)
        {
            if (state != CharacterState.Move)
            {
                state = CharacterState.Move;
                this.character.MoveBack();
                this.SendEntityEvent(EntityEvent.MoveBack);
            }
            // 后退：使用当前实际朝向的负方向，使用速度的绝对值
            float absSpeed = Math.Abs(this.character.speed);
            Vector3 moveDir = -this.transform.forward * (absSpeed + 9.81f) / 100f;
            this.rb.linearVelocity = this.rb.linearVelocity.y * Vector3.up + moveDir;
        }
        else
        {
            if (state != CharacterState.Idle)
            {
                state = CharacterState.Idle;
                this.rb.linearVelocity = Vector3.zero;
                this.character.Stop();
                this.SendEntityEvent(EntityEvent.Idle);
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            // 当前只触发动画事件；跳跃物理可后续补充
            this.SendEntityEvent(EntityEvent.Jump);
        }

        // 旋转处理
        if (h < -0.1 || h > 0.1)
        {
            Vector3 oldForward = this.transform.forward;
            this.transform.Rotate(0, h * rotateSpeed, 0);
            Vector3 newForward = this.transform.forward;

            Vector3 dir = GameObjectTool.LogicToWorld(character.direction);

            // 计算水平旋转角度（忽略 X/Z 轴，只看 Y 轴旋转）
            Vector3 dirFlat = new Vector3(dir.x, 0, dir.z).normalized;
            Vector3 forwardFlat = new Vector3(this.transform.forward.x, 0, this.transform.forward.z).normalized;
            float angle = Vector3.Angle(dirFlat, forwardFlat);

            // 只有偏转角超过阈值时才更新逻辑朝向（避免频繁同步/抖动）
            if (angle > this.turnAngle)
            {
                character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
                rb.transform.forward = this.transform.forward;
                this.SendEntityEvent(EntityEvent.None);
            }
        }
    }

    private void LateUpdate()
    {
        if (this.character == null)
            return;
        
        // 计算当前帧速度（用于同步或动画参数）
        Vector3 offset = this.rb.transform.position - lastPos;
        this.speed = (int)(offset.magnitude * 100f / Time.deltaTime);
        this.lastPos = this.rb.transform.position;

        // 当物理位置与逻辑位置偏差较大时，回写逻辑位置并触发同步/刷新
        if ((GameObjectTool.WorldToLogic(this.rb.transform.position) - this.character.position).magnitude > 50)
        {
            this.character.SetPosition(GameObjectTool.WorldToLogic(this.rb.transform.position));
            this.SendEntityEvent(EntityEvent.None);
        }

        // 让控制对象与刚体保持一致
        this.transform.position = this.rb.transform.position;
    }

    #endregion

    #region 私有辅助方法
    // 通知 EntityController 播放动画/更新状态

    private void SendEntityEvent(EntityEvent entityEvent)
    {
        if (entityController != null)
            entityController.OnEntityEvent(entityEvent);
    }

    #endregion
}
