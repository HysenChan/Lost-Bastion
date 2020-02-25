﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Linked Components")]
    public Transform weaponBone; //将武器作为骨骼的Parents

    private PlayerAnimator playerAnimator;//拿到动画制作器组件
    private PlayerState playerState;//拿到玩家的状态
    private Rigidbody rb;//拿到刚体
    private InputManager inputManager;//拿到输入管理器

    [Header("Setting")]
    public bool canTurnWhileDefending;  //当在防御的时候能不能转身
    public bool comboContinueOnHit = true;//仅在前一次攻击被击中时才继续使用连击

    [Header("States")]
    public DIRECTION currentDirection;//当前角色的朝向
    public GameObject itemInRange;//当前处于可交互范围内的项目
    private float lastAttackTime = 0;//上次攻击时间
    private bool continuePunchCombo;//如果需要继续拳击，则为true
    private bool continueKickCombo;//如果需要继续脚踢，则为true
    private INPUTACTION lastAttackInput;
    private DIRECTION lastAttackDirection;
    [SerializeField]
    private bool targetHit; //如果最后一次击中目标，则为true

    [Header("Attack Data & Combos")]
    public float hitZRange = 2;//z轴的攻击范围

    [Header("GameObject")]
    public DamageObject[] PunchCombo; //拳头攻击列表
    public DamageObject[] KickCombo; //脚踢攻击列表
    public DamageObject GroundPunchData; //拳击数据
    public DamageObject GroundKickData; //脚踢数据
    public DamageObject JumpKickData; //跳踢攻击数据
    private DamageObject lastAttack;//来自最近一次攻击的数据

    private int EnemyLayer;//敌人的层级
    private int DestroyableObjectLayer;//破坏对象的层级
    private int EnvironmentLayer;//环境的层级
    private LayerMask HitLayerMask;//所有可击中对象的列表
    private bool isGrounded;
    private bool isDead = false;
    private Vector3 fixedVelocity;
    private bool updateVelocity;
    private int attackNum = -1;//当前攻击组合编号

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

    private void OnEnable()
    {
        InputManager.onCombatInputEvent += CombatInputEvent;
    }

    private void OnDisable()
    {
        InputManager.onCombatInputEvent -= CombatInputEvent;
    }

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

    private void LateUpdate()
    {
        //将任何根运动偏移量应用于父级
        if (playerAnimator&&playerAnimator.GetComponent<Animator>().applyRootMotion&&playerAnimator.transform.localPosition!=Vector3.zero)
        {
            Vector3 offset = playerAnimator.transform.localPosition;
            playerAnimator.transform.localPosition = Vector3.zero;
            transform.position += offset * -(int)currentDirection;
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

    #region 攻击输入相关
    
    /// <summary>
    /// 攻击事件
    /// </summary>
    /// <param name="action"></param>
    private void CombatInputEvent(INPUTACTION action)
    {
        if (AttackStates.Contains(playerState.currentState)&&!isDead)
        {
            //默认拳击
            if (action==INPUTACTION.PUNCH&&playerState.currentState!=PLAYERSTATE.PUNCH&&playerState.currentState!=PLAYERSTATE.KICK&&isGrounded)
            {
                //如果时间在组合窗口内，则继续进行下一次攻击
                bool insideComboWindow = (lastAttack != null && (Time.time < (lastAttackTime + lastAttack.duration + lastAttack.comboResetTime)));
                if (insideComboWindow&&!continuePunchCombo&&(attackNum<KickCombo.Length-1))
                {
                    attackNum += 1;
                }
                else
                {
                    attackNum = 0;
                }

                if (PunchCombo[attackNum]!=null&&PunchCombo[attackNum].animTrigger.Length>0)
                {
                    DoAttack(PunchCombo[attackNum], PLAYERSTATE.PUNCH, INPUTACTION.PUNCH);
                }
            }

            //默认脚踢
            if (action==INPUTACTION.KICK&&playerState.currentState!=PLAYERSTATE.KICK&&playerState.currentState!=PLAYERSTATE.PUNCH&&isGrounded)
            {
                bool insideComboWindow = (lastAttack != null && (Time.time < (lastAttackTime + lastAttack.duration + lastAttack.comboResetTime)));
                if (insideComboWindow&&!continueKickCombo&&(attackNum<KickCombo.Length-1))
                {
                    attackNum += 1;
                }
                else
                {
                    attackNum = 0;
                }

                DoAttack(KickCombo[attackNum], PLAYERSTATE.KICK, INPUTACTION.KICK);
                return;
            }

            //跳踢
            if (action==INPUTACTION.KICK&&!isGrounded)
            {
                if (JumpKickData.animTrigger.Length>0)
                {
                    DoAttack(JumpKickData, PLAYERSTATE.JUMPKICK, INPUTACTION.KICK);
                    StartCoroutine(JumpKickInProgress());
                }
                return;
            }
        }
    }

    #endregion

    #region 攻击方法

    private void DoAttack(DamageObject damage,PLAYERSTATE state,INPUTACTION inputAction)
    {
        playerAnimator.SetAnimatorTrigger(damage.animTrigger);
        playerState.SetState(state);
        lastAttack = damage;
        lastAttack.inflictor = gameObject;
        lastAttackTime = Time.time;
        lastAttackInput = inputAction;
        lastAttackDirection = currentDirection;
        TurnToDir(currentDirection);
        SetVelocity(Vector3.zero);
        if (state==PLAYERSTATE.JUMPKICK)
            return;
        Invoke("Ready", damage.duration);
    }

    #endregion

    #region 物品交互

    public void interactWithItem()
    {
        if (itemInRange!=null)
        {
            playerAnimator.SetAnimatorTrigger("Pickup");
            playerState.SetState(PLAYERSTATE.PICKUPITEM);
            SetVelocity(Vector3.zero);
            //TODO:执行Ready方法和PickUpItem方法
            Invoke("Ready", 0.3f);
            Invoke("pickupItem", 0.2f);
        }
    }

    #endregion

    /// <summary>
    /// 攻击已完成，玩家已准备好采取新动作
    /// </summary>
    public void Ready()
    {
        //当角色碰到东西时才继续连击
        if (comboContinueOnHit&&!targetHit)
        {
            continuePunchCombo = continuePunchCombo = false;
            lastAttackTime = 0;
        }

        //连续拳击
        if (continuePunchCombo)
        {
            continuePunchCombo = continueKickCombo = false;
            if (attackNum<PunchCombo.Length-1)
            {
                attackNum += 1;
            }
            else
            {
                attackNum = 0;
            }
            if (PunchCombo[attackNum]!=null&&PunchCombo[attackNum].animTrigger.Length>0)
            {
                DoAttack(PunchCombo[attackNum], PLAYERSTATE.PUNCH, INPUTACTION.PUNCH);
            }
            return;
        }

        //连续脚踢
        if (continueKickCombo)
        {
            continueKickCombo = continuePunchCombo = false;
            if (attackNum<KickCombo.Length-1)
            {
                attackNum += 1;
            }
            else
            {
                attackNum = 0;
            }
            if (KickCombo[attackNum]!=null&&PunchCombo[attackNum].animTrigger.Length>0)
            {
                DoAttack(PunchCombo[attackNum], PLAYERSTATE.KICK, INPUTACTION.KICK);
            }
            return;
        }

        playerState.SetState(PLAYERSTATE.IDLE);
    }

    /// <summary>
    /// 进行跳踢
    /// </summary>
    /// <returns></returns>
    IEnumerator JumpKickInProgress()
    {
        playerAnimator.SetAnimatorBool("JumpKickActive", true);

        //击中的敌人列表
        List<GameObject> enemiesHit = new List<GameObject>();

        //延迟0.1s，让动画有时间播放
        yield return new WaitForSeconds(0.1f);

        //检查是否击中
        while (playerState.currentState==PLAYERSTATE.JUMPKICK)
        {
            //在角色前面绘制一个击中框，以查看与之碰撞的对象
            Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
            Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
            Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

            //通过将敌人添加到敌人列表中只击中一次
            foreach (Collider col in hitColliders)
            {
                if (!enemiesHit.Contains(col.gameObject))
                {
                    enemiesHit.Add(col.gameObject);

                    //攻击一个对象
                    IDamagable<DamageObject> damagableObject = col.GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
                    if (damagableObject!=null)
                    {
                        damagableObject.Hit(lastAttack);

                        //摄像机抖动
                        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
                        if (camShake!=null)
                        {
                            camShake.Shake(0.1f);
                        }
                    }
                }
            }
            yield return null;
        }
    }
}
