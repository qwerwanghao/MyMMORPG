using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using SkillBridge.Message;

/// <summary>
/// PlayerInputController：本地玩家输入驱动器。
/// - 读取 Unity Input（Horizontal/Vertical/Jump）。
/// - 驱动 Character 的逻辑状态（speed/direction/position），并通知 EntityController 播放动画。
/// - 同时直接控制 Rigidbody 产生移动（本地玩家的“权威移动”）。
/// </summary>
public class PlayerInputController : MonoBehaviour
{

    public Rigidbody rb;
    SkillBridge.Message.CharacterState state;

    /// <summary>绑定的逻辑角色（Entities.Character）。</summary>
    public Character character;

    public float rotateSpeed = 2.0f;

    public float turnAngle = 10;

    public int speed;

    public EntityController entityController;

    public bool onAir = false;

    // Use this for initialization
    void Start()
    {
        state = SkillBridge.Message.CharacterState.Idle;
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

            if (entityController != null) entityController.entity = this.character;
        }
    }


    void FixedUpdate()
    {
        if (character == null)
            return;

        // 前后移动：更新角色逻辑速度 + 发送动画事件；并用 Rigidbody 推动实际移动
        float v = Input.GetAxis("Vertical");
        if (v > 0.01)
        {
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                state = SkillBridge.Message.CharacterState.Move;
                this.character.MoveForward();
                this.SendEntityEvent(EntityEvent.MoveFwd);
            }
            this.rb.linearVelocity = this.rb.linearVelocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
        }
        else if (v < -0.01)
        {
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                state = SkillBridge.Message.CharacterState.Move;
                this.character.MoveBack();
                this.SendEntityEvent(EntityEvent.MoveBack);
            }
            this.rb.linearVelocity = this.rb.linearVelocity.y * Vector3.up + GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
        }
        else
        {
            if (state != SkillBridge.Message.CharacterState.Idle)
            {
                state = SkillBridge.Message.CharacterState.Idle;
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

        float h = Input.GetAxis("Horizontal");
        if (h < -0.1 || h > 0.1)
        {
            this.transform.Rotate(0, h * rotateSpeed, 0);
            Vector3 dir = GameObjectTool.LogicToWorld(character.direction);
            Quaternion rot = new Quaternion();
            rot.SetFromToRotation(dir, this.transform.forward);

            // 只有偏转角超过阈值时才更新逻辑朝向（避免频繁同步/抖动）
            if (rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))
            {
                character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
                rb.transform.forward = this.transform.forward;
                this.SendEntityEvent(EntityEvent.None);
            }

        }
    }
    Vector3 lastPos;
    float lastSync = 0;
    private void LateUpdate()
    {
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

    void SendEntityEvent(EntityEvent entityEvent)
    {
        if (entityController != null)
            entityController.OnEntityEvent(entityEvent);
    }
}
