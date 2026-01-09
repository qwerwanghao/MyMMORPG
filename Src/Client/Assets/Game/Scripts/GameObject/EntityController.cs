using Common;
using Entities;
using SkillBridge.Message;
using UnityEngine;

/// <summary>
/// EntityController：把“逻辑实体（Entities.Entity）”绑定到 Unity GameObject 的表现层组件。
/// - 非本地玩家：每个 FixedUpdate 驱动 entity.OnUpdate，并把逻辑坐标同步到 Transform/Rigidbody。
/// - 本地玩家：通常由 PlayerInputController 驱动 Rigidbody/Transform，这里不强制覆盖位姿。
/// - OnEntityEvent：接收输入/同步事件，驱动 Animator 播放移动/待机/跳跃等动作。
/// </summary>
public class EntityController : MonoBehaviour
{
    public Animator anim;
    public Rigidbody rb;

    /// <summary>绑定的逻辑实体（位置/朝向/速度都在 entity 上）。</summary>
    public Entity entity;

    public Vector3 position;
    public Vector3 direction;
    public Vector3 lastPosition;
    public float speed;
    public float animSpeed = 1.5f;
    public float jumpPower = 3.0f;

    /// <summary>是否为本地玩家角色（本地玩家通常由输入控制，不由同步驱动 Transform）。</summary>
    public bool isPlayer = false;

    private AnimatorStateInfo currentBaseState;
    private Quaternion rotation;
    private Quaternion lastRotation;

    private void Start()
    {
        if (entity != null)
        {
            this.UpdateTransform();
        }

        if (!this.isPlayer)
            rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (this.entity == null)
            return;

        // 推进逻辑实体状态（位置/协议数据等）
        this.entity.OnUpdate(Time.fixedDeltaTime);

        if (!this.isPlayer)
        {
            this.UpdateTransform();
        }
    }

    private void OnDestroy()
    {
        if (entity != null)
        {
            Log.InfoFormat("{0} OnDestroy :ID:{1} POS:{2} DIR:{3} SPD:{4} ", this.name, entity.entityId, entity.position, entity.direction, entity.speed);
            if (UIWorldElementManager.Instance != null)
            {
                UIWorldElementManager.Instance.RemoveCharacterElement(this.transform);
            }
        }
    }

    public void OnEntityEvent(EntityEvent entityEvent)
    {
        switch (entityEvent)
        {
            case EntityEvent.Idle:
                anim.SetBool("Move", false);
                anim.SetTrigger("Idle");
                break;
            case EntityEvent.MoveFwd:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.MoveBack:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.Jump:
                anim.SetTrigger("Jump");
                break;
        }
    }

    private void UpdateTransform()
    {
        // 将逻辑坐标（以 1/100 为单位的格点坐标）转换为 Unity 世界坐标
        this.position = GameObjectTool.LogicToWorld(entity.position);
        this.direction = GameObjectTool.LogicToWorld(entity.direction);

        // 非玩家对象：用 MovePosition 做物理移动同步，避免穿透/抖动
        this.rb.MovePosition(this.position);
        this.transform.forward = this.direction;
        this.lastPosition = this.position;
        this.lastRotation = this.rotation;
    }
}
