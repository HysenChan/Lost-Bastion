using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Linked Components")]
    private PlayerAnimator playerAnimator;//拿到动画制作器组件
    private PlayerState playerState;//拿到玩家的状态
    private Rigidbody rb;//拿到刚体
    private InputManager inputManager;//拿到输入管理器

    [Header("Setting")]
    public bool canTurnWhileDefending;  //当在防御的时候能不能转身

    [Header("States")]
    public DIRECTION currentDirection;//当前角色的朝向

    private int EnemyLayer;//敌人的层级
    private int DestroyableObjectLayer;//破坏对象的层级
    private int EnvironmentLayer;//环境的层级
    private LayerMask HitLayerMask;//所有可击中对象的列表
    private bool isGrounded;
    private bool isDead = false;
    private Vector3 fixedVelocity;
    private bool updateVelocity;

    //玩家可以攻击的状态列表
    private List<PLAYERSTATE> AttackStates = new List<PLAYERSTATE>()
    {
        PLAYERSTATE.IDLE,
        PLAYERSTATE.WALK,
        PLAYERSTATE.JUMPING,
        PLAYERSTATE.PUNCH,
        PLAYERSTATE.KICK,
        PLAYERSTATE.DEFEND,
    };

    //可以击中玩家的状态列表
    private List<PLAYERSTATE> HitableStates = new List<PLAYERSTATE>()
    {
        PLAYERSTATE.DEFEND,
        PLAYERSTATE.HIT,
        PLAYERSTATE.IDLE,
        PLAYERSTATE.LAND,
        PLAYERSTATE.PUNCH,
        PLAYERSTATE.KICK,
        PLAYERSTATE.THROW,
        PLAYERSTATE.WALK,
        PLAYERSTATE.GROUNDKICK,
        PLAYERSTATE.GROUNDPUNCH,
    };

    //玩家可以激活防御的状态列表
    private List<PLAYERSTATE> DefendStates = new List<PLAYERSTATE>()
    {
        PLAYERSTATE.IDLE,
        PLAYERSTATE.DEFEND,
        PLAYERSTATE.WALK,
    };

    private void Start()
    {
        playerAnimator = GetComponentInChildren<PlayerAnimator>();
        playerState = GetComponent<PlayerState>();
        rb = GetComponent<Rigidbody>();
        inputManager = FindObjectOfType<InputManager>();

        //找不到组件时输出到控制台
        if (!playerAnimator)
            Debug.LogError(gameObject.name+"找不到PlayerAnimator脚本组件！");
        if (!playerState)
            Debug.LogError(gameObject.name + "找不到PlayerState脚本组件！");
        if (!rb)
            Debug.LogError(gameObject.name + "找不到Rigidbody组件！");

        //分配Layer和LayerMask
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        DestroyableObjectLayer = LayerMask.NameToLayer("DestroyableObject");
        EnvironmentLayer = LayerMask.NameToLayer("Environment");
        //开启EnemyLayer和DestroyableObjectLayer层
        HitLayerMask = (1 << EnemyLayer) | (1 << DestroyableObjectLayer);

        //TODO:在跳跃过程中设置玩家为无敌

    }

    private void Update()
    {
        //判断玩家控制器是否存在
        if (playerAnimator)
        {
            isGrounded = playerAnimator.animator.GetBool("isGrounded"); //拿到isGrounded的bool值
        }

        //每帧检查是否进入防御状态
        if (!isDead&&isGrounded&&DefendStates.Contains(playerState.currentState))
        {
            Defend(inputManager.isDefendKeyDown());
        }
    }

    private void FixedUpdate()
    {
        if (updateVelocity)
        {
            rb.velocity = fixedVelocity;
            updateVelocity = false;
        }
    }

    /// <summary>
    /// 设置当前防御是否在防御状态
    /// </summary>
    /// <param name="defend"></param>
    private void Defend(bool defend)
    {
        playerAnimator.SetAnimatorBool("Defend", defend);
        if (defend)
        {
            //保持角色当前的朝向
            if (!canTurnWhileDefending)
            {
                //拿到y的旋转角度，进行判断朝左边还是朝右边
                int rot = Mathf.RoundToInt(transform.rotation.eulerAngles.y);
                if (rot>=180&&rot<=256)
                {
                    currentDirection = DIRECTION.Left;
                }
                else
                {
                    currentDirection = DIRECTION.Right;
                }
            }
            //转向到当前朝向
            TurnToDir(currentDirection);
            //设置当前的速度为0
            SetVelocity(Vector3.zero);
            //设置角色状态为防御状态
            playerState.SetState(PLAYERSTATE.DEFEND);
        }
        else
        {
            //当角色不处于防御状态时，则切换为站立状态
            playerState.SetState(PLAYERSTATE.IDLE);
        }
    }

    /// <summary>
    /// 朝向指定的方向
    /// </summary>
    /// <param name="dir">指定的方向</param>
    public void TurnToDir(DIRECTION dir)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward * -(int)dir);
    }

    /// <summary>
    /// 在下一个固定更新中设置速度
    /// </summary>
    /// <param name="velocity"></param>
    public void SetVelocity(Vector3 velocity)
    {
        fixedVelocity = velocity;
        updateVelocity = true;
    }
}
