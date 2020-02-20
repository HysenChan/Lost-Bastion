using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerState))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Linked Components")]
    private PlayerAnimator animator;
    private Rigidbody rb;
    private PlayerState playerState;
    private CapsuleCollider playerCapsuleCollider;

    [Header("Player Settings")]
    public float walkSpeed = 3f;        //走路速度
    public float ZSpeed = 1.5f;         //z轴移动速度
    public float jumpForce = 8f;        //跳起来的力度
    public bool AllowAirControl;
    public bool AllowDepthJumping;
    public float AirAcceleration = 3f;  //空气加速度
    public float AirMaxSpeed = 3f;      //空气最大速度
    public float rotationSpeed = 15f;   //旋转速度
    public float jumpRotationSpeed = 30f;   //跳起的旋转速度
    public float lookAheadDistance = 0.2f;
    public float landRecoveryTime = 0.1f;   //地面恢复时间
    public LayerMask CollisionLayer;

    [Header("Audio")]
    public string jumpUpVoice = "";
    public string jumpLandVoice = "";

    [Header("States")]
    public DIRECTION currentDirection;
    public Vector2 inputDirection;
    public bool jumpInProgress;
    public bool isGrounded;

    private bool isDead = false;
    private Vector3 fixedVelocity;
    private bool updateVelocity;
    private Plane[] frustrumPlanes;//相机视图视锥
    public bool playerInCameraView;//如果玩家在相机视图内，则为true

    //该组件可以影响的状态列表
    private List<PlayerState.PLAYERSTATE> MovementStates = new List<PlayerState.PLAYERSTATE>()
    {
        PlayerState.PLAYERSTATE.IDLE,
        PlayerState.PLAYERSTATE.WALK,
        PlayerState.PLAYERSTATE.JUMPING,
        PlayerState.PLAYERSTATE.JUMPKICK,
        PlayerState.PLAYERSTATE.LAND,
    };

    private void OnEnable()
    {
        //订阅移动事件
        InputManager.onMoveInputEvent += MoveInputEvent;
        //订阅战斗事件
        InputManager.onCombatInputEvent += CombatInputEvent;
    }

    private void OnDisable()
    {
        //取消订阅移动事件
        InputManager.onMoveInputEvent -= MoveInputEvent;
        //取消订阅战斗事件
        InputManager.onCombatInputEvent -= CombatInputEvent;
    }

    private void Start()
    {
        //找到对应的组件
        if (!animator)
        {
            animator = GetComponentInChildren<PlayerAnimator>();
        }
        else
        {
            Debug.LogError(gameObject.name+"找不到animator组件");
        }
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError(gameObject.name + "找不到Rigidbody组件");
        }
        if (!playerState)
        {
            playerState = GetComponent<PlayerState>();
        }
        else
        {
            Debug.LogError(gameObject.name + "找不到PlayerState脚本");
        }
        if (!playerCapsuleCollider)
        {
            playerCapsuleCollider = GetComponent<CapsuleCollider>();
        }
        else
        {
            Debug.LogError(gameObject.name + "找不到CapsuleCollider组件");
        }
    }

    private void FixedUpdate()
    {
        //检查是否站在地面上
        isGrounded = IsGrounded();
        //设置控制器bool值
        if (animator)
        {
            //设置地面
            animator.SetAnimatorBool("isGrounded", isGrounded);
            //检查是否下落
            animator.SetAnimatorBool("Falling", !isGrounded && rb.velocity.y < 0.1f && playerState.currentState != PlayerState.PLAYERSTATE.KNOCKDOWN);
            //更新方向
            animator.currentDirection = currentDirection;
        }

        //拿到角色是否在摄像机范围内
        playerInCameraView = PlayerInsideCamViewArea();

        //更新角色移动速度
        if (updateVelocity&&MovementStates.Contains(playerState.currentState))
        {
            rb.velocity = fixedVelocity;
            updateVelocity = false;
        }
    }

    /// <summary>
    /// 在下一次固定更新时设置速度
    /// </summary>
    /// <param name="velocity">速度</param>
    private void SetVelocity(Vector3 velocity)
    {
        fixedVelocity = velocity;
        updateVelocity = true;
    }

    /// <summary>
    /// 判断是否在地面上
    /// </summary>
    /// <returns>是则返回true</returns>
    public bool IsGrounded()
    {
        //检查与胶囊碰撞盒向下偏移0.1,用作胶囊碰撞检测
        Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (playerCapsuleCollider.radius - 0.1f);
        if (Physics.CheckCapsule(transform.position+playerCapsuleCollider.center,bottomCapsulePos,playerCapsuleCollider.radius,CollisionLayer))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        return isGrounded;
    }

    /// <summary>
    /// 在地面上移动
    /// </summary>
    private void MoveGrounded()
    {
        //跳跃着陆时不能移动
        if (playerState.currentState==PlayerState.PLAYERSTATE.LAND)
        {
            return;
        }

        //设置刚体速度
        if (rb!=null&&(inputDirection.sqrMagnitude>0)&&!WallInFront()&&PlayerInsideCamViewArea())
        {
            SetVelocity(new Vector3(inputDirection.x * -walkSpeed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, inputDirection.y * -ZSpeed));
            SetPlayerState(PlayerState.PLAYERSTATE.WALK);
        }
        else
        {
            SetVelocity(new Vector3(0, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0));
            SetPlayerState(PlayerState.PLAYERSTATE.IDLE);
        }

        //当玩家位于屏幕边缘时允许上/下移动，不允许左/右移动
        if (!PlayerInsideCamViewArea()&&Mathf.Abs(inputDirection.y)>0)
        {
            Vector3 dirToCamera = (transform.position - Camera.main.transform.position) * inputDirection.y;
            SetVelocity(new Vector3(dirToCamera.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, dirToCamera.z));
        }

        //根据输入向量设置当前方向。（通过使用Mathf.Sign忽略上下移动，因为我们希望玩家在上下移动时保持在当前方向上）
        int dir = Mathf.RoundToInt(Mathf.Sign((float) - inputDirection.x));
        if (Mathf.Abs(inputDirection.x)>0)
        {
            currentDirection = (DIRECTION)dir;
        }

        //将运动速度发送给动画控制器
        if (animator)
        {
            animator.SetAnimatorFloat("MovementSpeed", rb.velocity.magnitude);
        }

        //朝行进方向看
        LookToDir(currentDirection);
    }

    /// <summary>
    /// 在空中移动
    /// </summary>
    private void MoveAir()
    {
        if (!WallInFront()&&PlayerInsideCamViewArea())
        {
            //在空中保持方向，不能转向
            int lastKnownDirection = (int)currentDirection;
            if (Mathf.Abs(inputDirection.x) > 0)
            {
                lastKnownDirection = Mathf.RoundToInt(-inputDirection.x);
            }
            LookToDir((DIRECTION)lastKnownDirection);

            //移动方向基于当前输入方向
            int dir = Mathf.Clamp(Mathf.RoundToInt(-inputDirection.x), -1, 1);
            float speed = Mathf.Clamp(rb.velocity.x + AirMaxSpeed * dir * Time.fixedDeltaTime * AirAcceleration, -AirMaxSpeed, AirMaxSpeed);

            //应用移动
            if (!updateVelocity)
            {
                if (AllowDepthJumping)
                {
                    SetVelocity(new Vector3(speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, -inputDirection.y * ZSpeed));
                }
                else
                {
                    SetVelocity(new Vector3(speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0));
                }
            }
        }
        else
        {
            if (!updateVelocity)
            {
                SetVelocity(new Vector3(0f, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0f));
            }
        }
    }

    /// <summary>
    /// 设置玩家状态
    /// </summary>
    /// <param name="state"></param>
    public void SetPlayerState(PlayerState.PLAYERSTATE state)
    {
        if (playerState!=null)
        {
            playerState.SetState(state);
        }
        else
        {
            Debug.Log("playerState not found on this gameObject");
        }
    }

    /// <summary>
    /// 判断玩家是否在摄像机范围内
    /// </summary>
    /// <returns>在范围内则返回true</returns>
    private bool PlayerInsideCamViewArea()
    {
        if (Camera.main!=null)
        {
            //计算视景平面
            frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            //边界的原点和大小
            Bounds bounds = new Bounds(transform.position + Vector3.right * (int)currentDirection, Vector3.one * playerCapsuleCollider.radius);
            //检测玩家是否能被摄像机捕抓到，如果能则返回true
            return GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds);
        }
        return true;
    }

    /// <summary>
    /// 判断面前是否有墙壁碰撞盒
    /// </summary>
    /// <returns>有墙壁则返回true</returns>
    private bool WallInFront()
    {
        Vector3 MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
        var cc = GetComponent<CapsuleCollider>();
        //返回球型半径之内（包括半径）的所有碰撞体 collider[]。
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up * (cc.radius + 0.1f) + -MovementOffset, cc.radius, CollisionLayer);

        int i = 0;
        bool hasHitWall = false;
        while (i<hitColliders.Length)
        {
            if (CollisionLayer==(CollisionLayer | 1<<hitColliders[i].gameObject.layer))
            {
                hasHitWall = true;
            }
            i++;
        }
        return hasHitWall;
    }

    /// <summary>
    /// 设定当前方向
    /// </summary>
    /// <param name="dir"></param>
    public void SetDirection(DIRECTION dir)
    {
        currentDirection = dir;
        LookToDir(currentDirection);
    }

    /// <summary>
    /// 返回当前方向
    /// </summary>
    /// <returns></returns>
    public DIRECTION GetCurrentDirection()
    {
        return currentDirection;
    }

    /// <summary>
    /// 朝一个方向看
    /// </summary>
    /// <param name="dir">方向</param>
    public void LookToDir(DIRECTION dir)
    {
        Vector3 newDir = Vector3.zero;  //初始化方向
        if (dir==DIRECTION.Right||dir==DIRECTION.Left)
        {
            if (isGrounded)
            {
                newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * -(int)dir, rotationSpeed * Time.deltaTime, 0.0f);
            }
            else
            {
                newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * (int)dir, jumpRotationSpeed * Time.deltaTime, 0.0f);
            }

            transform.rotation = Quaternion.LookRotation(newDir);
            currentDirection = dir;
        }
    }

    /// <summary>
    /// 玩家是否死亡，如果死亡则设置速度为(0,0,0)
    /// </summary>
    private void Death()
    {
        isDead = true;
        SetVelocity(Vector3.zero);
    }

    /// <summary>
    /// 移动的输入事件
    /// </summary>
    /// <param name="dir">方向</param>
    private void MoveInputEvent(Vector2 dir)
    {
        inputDirection = dir;
        if (MovementStates.Contains(playerState.currentState)&&!isDead)
        {
            if (jumpInProgress)
            {
                MoveAir();
            }
            else
            {
                MoveGrounded();
            }
        }
    }

    /// <summary>
    /// 战斗的输入事件
    /// </summary>
    /// <param name="combatAction">输入的战斗状态</param>
    private void CombatInputEvent(InputManager.COMBATACTION combatAction)
    {
        if (MovementStates.Contains(playerState.currentState)&&!isDead)
        {
            if (combatAction==InputManager.COMBATACTION.JUMP)
            {
                if (playerState.currentState!=PlayerState.PLAYERSTATE.JUMPING&&IsGrounded())
                {
                    StopAllCoroutines();
                    StartCoroutine(DoJump());
                }
            }
        }
    }

    /// <summary>
    /// 跳跃和落地的协程
    /// </summary>
    /// <returns></returns>
    IEnumerator DoJump()
    {
        //设置跳的状态
        jumpInProgress = true;
        playerState.SetState(PlayerState.PLAYERSTATE.JUMPING);

        //设置跳的条件
        animator.SetAnimatorBool("JumpInProgress", true);
        animator.SetAnimatorTrigger("JumpUp");
        //跳起的特效
        animator.ShowDustEffectJump();

        //TODO:播放跳的音效
        if (jumpUpVoice!="")
        {
            //后期加上
        }

        //等待直到下一个固定帧速率更新函数
        yield return new WaitForFixedUpdate();

        //开始跳跃
        while (isGrounded)
        {
            SetVelocity(Vector3.up * jumpForce);
            yield return new WaitForFixedUpdate();
        }

        //继续直到墙边
        while (!isGrounded)
        {
            yield return new WaitForFixedUpdate();
        }

        //跳起来着陆后
        playerState.SetState(PlayerState.PLAYERSTATE.LAND);
        SetVelocity(Vector3.zero);
        //着陆后的状态设置
        animator.SetAnimatorFloat("MovementSpeed", 0f);
        animator.SetAnimatorBool("JumpInProgress", false);
        animator.SetAnimatorBool("JumpKickActive", false);
        //着地的特效
        animator.ShowDustEffectLand();

        //TODO:修改跳起音效为行走音效

        //更新跳的状态
        jumpInProgress = false;

        //角色着陆后设置当前角色状态
        if (playerState.currentState==PlayerState.PLAYERSTATE.LAND)
        {
            yield return new WaitForSeconds(landRecoveryTime);
            SetPlayerState(PlayerState.PLAYERSTATE.IDLE);
        }
    }

    /// <summary>
    /// 中断正在进行的跳跃
    /// </summary>
    public void CancelJump()
    {
        jumpInProgress = false;
        StopAllCoroutines();
    }

    /// <summary>
    /// 在统一编辑器中绘制前瞻球体
    /// </summary>
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var c = GetComponent<CapsuleCollider>();
        Gizmos.color = Color.yellow;
        Vector3 MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * (c.radius + 0.1f) + -MovementOffset, c.radius);
    }
#endif
}

/// <summary>
/// 枚举基础移动的方向
/// </summary>
public enum DIRECTION
{
    Right = -1,
    Left = 1,
    Up = 2,
    Down = -2,
};
