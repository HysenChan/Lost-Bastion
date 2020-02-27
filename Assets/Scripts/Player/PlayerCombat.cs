using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Linked Components")]
    public Transform weaponBone; //将武器作为骨骼的Parents(手掌)

    [Header("Attack Data & Combos")]
    public float hitZRange = 2;//z轴的攻击范围

    private PlayerAnimator playerAnimator;//拿到动画制作器组件
    private PlayerState playerState;//拿到玩家的状态
    private Rigidbody rb;//拿到刚体
    private InputManager inputManager;//拿到输入管理器

    [Header("Setting")]
    public bool blockAttacksFromBehind = false; //阻止来自后面的敌人袭击
    public bool comboContinueOnHit = true;//仅在前一次攻击被击中时才继续使用连击
    public bool resetComboChainOnChangeCombo; //切换到其他组合链时重新启动组合
    public bool invulnerableDuringJump = false; //检查是否可以在跳跃过程中击中玩家
    public bool canTurnWhileDefending;  //当在防御的时候能不能转身
    public float hitRecoveryTime = 0.3f; //从被攻击恢复所需的时间
    public float hitThreshold = 0.2f;//再次受到打击之前的时间
    public float hitKnockBackForce = 1.5f; //被击中向后退的力
    public float GroundAttackDistance = 1.5f;//距敌人可进行地面攻击的距离
    public int knockdownHitCount = 3;//玩家被击倒之前可以被击中的次数
    public float KnockdownTimeout = 0; //击倒后反应过来的时间
    public float KnockdownUpForce = 5; //击倒的垂直力
    public float KnockbackForce = 4; //击倒的水平力
    public float KnockdownStandUpTime = 0.8f; //站立动画完成所需的时间

    [Header("States")]
    public DIRECTION currentDirection;//当前角色的朝向
    public GameObject itemInRange;//当前处于可交互范围内的物品
    private Weapon currentWeapon;//玩家当前使用的武器
    private float lastAttackTime = 0;//上次攻击时间
    private bool continuePunchCombo;//如果需要继续拳击，则为true
    private bool continueKickCombo;//如果需要继续脚踢，则为true
    private INPUTACTION lastAttackInput;
    private DIRECTION lastAttackDirection;
    [SerializeField]
    private bool targetHit; //如果最后一次击中目标，则为true

    [Header("GameObject")]
    public DamageObject[] PunchCombo; //拳头攻击列表
    public DamageObject[] KickCombo; //脚踢攻击列表
    public DamageObject GroundPunchData; //拳击数据
    public DamageObject GroundKickData; //脚踢数据
    public DamageObject JumpKickData; //跳踢攻击数据
    private DamageObject lastAttack;//来自最近一次攻击的数据

    [Header("Audio")]
    public string knockdownVoiceSFX = "";
    public string hitVoiceSFX = "";
    public string DeathVoiceSFX = "";
    public string defenceHitSFX = "";

    private int EnemyLayer;//敌人的层级
    private int DestroyableObjectLayer;//破坏对象的层级
    private int EnvironmentLayer;//环境的层级
    private LayerMask HitLayerMask;//所有可击中对象的列表
    private bool isGrounded;
    private bool isDead = false;
    private Vector3 fixedVelocity;
    private bool updateVelocity;
    private int attackNum = -1;//当前攻击组合编号
    private float LastHitTime = 0; // 最后一次被打中的时间
    private int hitKnockDownCount = 0;//玩家连续被击中的次数
    private int hitKnockDownResetTime = 2;//重置被击倒前时间

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
        InputManager.onMoveInputEvent += MovementInputEvent;
    }

    private void OnDisable()
    {
        InputManager.onCombatInputEvent -= CombatInputEvent;
        InputManager.onMoveInputEvent -= MovementInputEvent;
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

        //在跳跃过程中设置玩家为无敌
        if (!invulnerableDuringJump)
        {
            HitableStates.Add(PLAYERSTATE.JUMPING);
            HitableStates.Add(PLAYERSTATE.JUMPKICK);
        }
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

    /// <summary>
    /// 移动事件
    /// </summary>
    /// <param name="inputVector"></param>
    private void MovementInputEvent(Vector2 inputVector)
    {
        int dir = Mathf.RoundToInt(Mathf.Sign((float)-inputVector.x));
        if (Mathf.Abs(inputVector.x)>0)
        {
            currentDirection = (DIRECTION)dir;
        }
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

            //捡起物品
            if (action==INPUTACTION.PUNCH&&itemInRange!=null&&isGrounded&&currentWeapon==null)
            {
                InteractWithItem();
                return;
            }

            //使用武器
            if (action==INPUTACTION.PUNCH&&isGrounded&&currentWeapon!=null)
            {
                UseCurrentWeapon();
                return;
            }

            //如果在拳头攻击中已按下，则进行组合打击
            if (action==INPUTACTION.PUNCH&&(playerState.currentState==PLAYERSTATE.PUNCH)&&!continuePunchCombo&&isGrounded)
            {
                if (attackNum<PunchCombo.Length-1)
                {
                    continuePunchCombo = true;
                    continueKickCombo = false;
                    return;
                }
            }

            //如果在脚踢攻击中已按下，则进行组合打击
            if (action == INPUTACTION.KICK && (playerState.currentState == PLAYERSTATE.KICK) && !continueKickCombo && isGrounded)
            {
                if (attackNum < KickCombo.Length - 1)
                {
                    continueKickCombo = true;
                    continuePunchCombo = false;
                    return;
                }
            }

            //敌人倒地地面拳击
            if (action == INPUTACTION.PUNCH && (playerState.currentState != PLAYERSTATE.PUNCH && NearbyEnemyDown()) && isGrounded)
            {
                if (GroundPunchData.animTrigger.Length > 0)
                {
                    DoAttack(GroundPunchData, PLAYERSTATE.GROUNDPUNCH, INPUTACTION.PUNCH);
                }
                return;
            }

            //敌人倒地地面脚踢
            if (action == INPUTACTION.KICK && (playerState.currentState != PLAYERSTATE.KICK && NearbyEnemyDown()) && isGrounded)
            {
                if (GroundKickData.animTrigger.Length > 0)
                {
                    DoAttack(GroundKickData, PLAYERSTATE.GROUNDKICK, INPUTACTION.KICK);
                }
                return;
            }

            //切换到另一个组合链时重置组合（用户设置）
            if (resetComboChainOnChangeCombo&&(action!=lastAttackInput))
            {
                attackNum = -1;
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

    /// <summary>
    /// 使用目前装备的武器
    /// </summary>
    private void UseCurrentWeapon()
    {
        playerState.SetState(PLAYERSTATE.USEWEAPON);
        TurnToDir(currentDirection);
        SetVelocity(Vector3.zero);

        lastAttackInput = INPUTACTION.WEAPONATTACK;
        lastAttackTime = Time.time;
        lastAttack = currentWeapon.damageObject;
        lastAttack.inflictor = gameObject;
        lastAttackDirection = currentDirection;

        if (!string.IsNullOrEmpty(currentWeapon.damageObject.animTrigger))
        {
            playerAnimator.SetAnimatorTrigger(currentWeapon.damageObject.animTrigger);
        }
        if (!string.IsNullOrEmpty(currentWeapon.useSound))
        {
            GlobalAudioPlayer.PlaySFX(currentWeapon.useSound);
        }
        Invoke("Ready", currentWeapon.damageObject.duration);
        if (currentWeapon.damageType==WEAPONDAMAGETYPE.WEAPONONUSE)
        {
            currentWeapon.UseWeapon();
        }

        //上一次使用
        if (currentWeapon.damageType==WEAPONDAMAGETYPE.WEAPONONUSE&&currentWeapon.timesToUse==0)
        {
            StartCoroutine(DestroyCurrentWeapon(currentWeapon.damageObject.duration));
        }
        if (currentWeapon.damageType == WEAPONDAMAGETYPE.WEAPONONHIT && currentWeapon.timesToUse == 1)
        {
            StartCoroutine(DestroyCurrentWeapon(currentWeapon.damageObject.duration));
        }
    }

    /// <summary>
    /// 移除当前武器
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator DestroyCurrentWeapon(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentWeapon.damageType==WEAPONDAMAGETYPE.WEAPONONUSE)
        {
            GlobalAudioPlayer.PlaySFX(currentWeapon.breakSound);
        }
        Destroy(currentWeapon.playerHandPrefab);
        currentWeapon.BreakWeapon();
        currentWeapon = null;
    }

    #endregion

    #region 物品交互

    /// <summary>
    /// 物品在交互范围内
    /// </summary>
    /// <param name="item"></param>
    public void ItemInRange(GameObject item)
    {
        itemInRange = item;
    }

    /// <summary>
    /// 物品不在交互范围内
    /// </summary>
    /// <param name="item"></param>
    public void ItemOutOfRange(GameObject item)
    {
        if (itemInRange==item)
        {
            itemInRange = null;
        }
    }

    /// <summary>
    /// 与范围内的物品互动
    /// </summary>
    public void InteractWithItem()
    {
        if (itemInRange!=null)
        {
            playerAnimator.SetAnimatorTrigger("Pickup");
            playerState.SetState(PLAYERSTATE.PICKUPITEM);
            SetVelocity(Vector3.zero);
            Invoke("Ready", 0.3f);
            Invoke("PickupItem", 0.2f);
        }
    }

    /// <summary>
    /// 捡起物品
    /// </summary>
    void PickupItem()
    {
        if (itemInRange!=null)
        {
            itemInRange.SendMessage("OnPickup", gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// 装备当前武器
    /// </summary>
    /// <param name="weapon"></param>
    public void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        currentWeapon.damageObject.inflictor = gameObject;

        //添加武器到玩家手中
        if (weapon.playerHandPrefab!=null)
        {
            GameObject playerWeapon = GameObject.Instantiate(weapon.playerHandPrefab, weaponBone) as GameObject;
            currentWeapon.playerHandPrefab = playerWeapon;
        }
    }

    #endregion

    #region 检查是否击中

    /// <summary>
    /// 检查我们是否碰了点东西（动画事件）
    /// </summary>
    public void CheckForHit()
    {
        //在角色前面绘制一个点击框，以查看与之碰撞的对象
        Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)lastAttackDirection * lastAttack.collDistance);
        Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
        Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {

            //击中一个损坏的物体
            IDamagable<DamageObject> damagableObject = hitColliders[i].GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
            if (damagableObject != null)
            {
                damagableObject.Hit(lastAttack);

                //击中了物体，返回true
                targetHit = true;
            }
            i++;
        }

        //武器击中
        if (lastAttackInput==INPUTACTION.WEAPONATTACK&&targetHit)
        {
            currentWeapon.OnHitSomething();
        }

        //什么都没击中
        if (hitColliders.Length==0)
        {
            targetHit = false;
        }
    }

    /// <summary>
    /// 在编辑器状态显示角色攻击范围盒子方便调试
    /// </summary>
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration)
        {
            Gizmos.color = Color.red;
            Vector3 boxPositon = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)lastAttackDirection * lastAttack.collDistance);
            Vector3 boxSize = new Vector3(lastAttack.CollSize, lastAttack.CollSize, hitZRange);
            Gizmos.DrawCube(boxPositon, boxSize);
        }
    }
#endif

    #endregion

    #region 被敌人击中

    /// <summary>
    /// 我们被击中
    /// </summary>
    /// <param name="damage">伤害</param>
    public void Hit(DamageObject damage)
    {
        //判断是否可以再次被击中的时间间隔
        if (Time.time<LastHitTime+hitThreshold)
        {
            return;
        }
        //检查我们是否处于可被攻击状态
        if (HitableStates.Contains(playerState.currentState))
        {
            CancelInvoke();

            //被攻击时摄像机抖动
            CameraShake camShake = Camera.main.GetComponent<CameraShake>();
            if (camShake != null)
            {
                camShake.Shake(.1f);
            }

            //防御接下来的攻击
            if (playerState.currentState==PLAYERSTATE.DEFEND&&!damage.DefenceOverride&&(IsFacingTarget(damage.inflictor)||blockAttacksFromBehind))
            {
                Defend(damage);
                return;
            }
            else
            {
                playerAnimator.SetAnimatorBool("Defend", false);
            }

            UpdateHitCounter();
            LastHitTime = Time.time;

            //展示被击中特效
            playerAnimator.ShowHitEffect();

            //减少生命值
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem!=null)
            {
                healthSystem.SubstractHealth(damage.damageCount);
                if (healthSystem.CurrentHp==0)
                {
                    return;
                }
            }

            //检查是否被击倒在地
            if ((hitKnockDownCount >= knockdownHitCount || !IsGrounded() || damage.knockDown) && playerState.currentState != PLAYERSTATE.KNOCKDOWN)
            {
                hitKnockDownCount = 0;
                StopCoroutine("KnockDownSequence");
                StartCoroutine("KnockDownSequence", damage.inflictor);
                GlobalAudioPlayer.PlaySFXAtPosition(damage.hitSFX, transform.position + Vector3.up);
                GlobalAudioPlayer.PlaySFXAtPosition(knockdownVoiceSFX, transform.position + Vector3.up);
                return;
            }

            //默认被击
            int i = Random.Range(1, 3);
            playerAnimator.SetAnimatorTrigger("Hit" + i);
            SetVelocity(Vector3.zero);
            playerState.SetState(PLAYERSTATE.HIT);

            //从冲击中增加一点力
            if (IsFacingTarget(damage.inflictor))
            {
                playerAnimator.AddForce(-1.5f);
            }
            else
            {
                playerAnimator.AddForce(1.5f);
            }

            //音效
            GlobalAudioPlayer.PlaySFXAtPosition(damage.hitSFX, transform.position + Vector3.up);
            GlobalAudioPlayer.PlaySFXAtPosition(hitVoiceSFX, transform.position + Vector3.up);

            Invoke("Ready", hitRecoveryTime);
        }
    }

    /// <summary>
    /// 更新被打了多少次
    /// </summary>
    private void UpdateHitCounter()
    {
        if (Time.time-LastHitTime<hitKnockDownResetTime)
        {
            hitKnockDownCount += 1;
        }
        else
        {
            hitKnockDownCount = 1;
        }
        LastHitTime = Time.time;
    }

    /// <summary>
    /// 重载Defend()方法，用于防御即将到达的伤害
    /// </summary>
    /// <param name="damage"></param>
    private void Defend(DamageObject damage)
    {
        //防御的特效
        playerAnimator.ShowDefendEffect();

        //添加防御的音效
        GlobalAudioPlayer.PlaySFXAtPosition(defenceHitSFX, transform.position + Vector3.up);

        //添加一个用于防御的力
        if (IsFacingTarget(damage.inflictor))
        {
            playerAnimator.AddForce(-hitKnockBackForce);
        }
        else
        {
            playerAnimator.AddForce(hitKnockBackForce);
        }
    }

    /// <summary>
    /// 被击倒
    /// </summary>
    /// <param name="inflictor"></param>
    /// <returns></returns>
    public IEnumerator KnockDownSequence(GameObject inflictor)
    {
        playerState.SetState(PLAYERSTATE.KNOCKDOWN);
        playerAnimator.StopAllCoroutines();
        yield return new WaitForFixedUpdate();

        //倒地方向朝攻击来源方向
        int dir = inflictor.transform.position.x > transform.position.x ? 1 : -1;
        currentDirection = (DIRECTION)dir;
        TurnToDir(currentDirection);

        //更新玩家移动
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement!=null)
        {
            playerMovement.CancelJump();
            playerMovement.SetDirection(currentDirection);
        }

        //击倒的力
        playerAnimator.SetAnimatorTrigger("KnockDown_Up");
        while (IsGrounded())
        {
            SetVelocity(new Vector3(KnockbackForce * -dir, KnockdownUpForce, 0));
            yield return new WaitForFixedUpdate();
        }

        //往上
        while (rb.velocity.y >= 0)
        {
            yield return new WaitForFixedUpdate();
        }

        //往下
        playerAnimator.SetAnimatorTrigger("KnockDown_Down");
        while (!IsGrounded())
        {
            yield return new WaitForFixedUpdate();
        }

        //击中地面
        playerAnimator.SetAnimatorTrigger("KnockDown_End");
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        if (camShake != null) camShake.Shake(.3f);
        playerAnimator.ShowDustEffectLand();

        //播放音效
        GlobalAudioPlayer.PlaySFXAtPosition("Drop", transform.position);

        //地面滑动
        float t = 0;
        float speed = 2;
        Vector3 fromVelocity = rb.velocity;
        while (t < 1)
        {
            SetVelocity(Vector3.Lerp(new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z), new Vector3(0, rb.velocity.y, 0), t));
            t += Time.deltaTime * speed;
            yield return null;
        }

        //倒地时间
        SetVelocity(Vector3.zero);
        yield return new WaitForSeconds(KnockdownTimeout);

        //倒地站起来
        playerAnimator.SetAnimatorTrigger("StandUp");
        playerState.currentState = PLAYERSTATE.STANDUP;

        yield return new WaitForSeconds(KnockdownStandUpTime);
        playerState.currentState = PLAYERSTATE.IDLE;
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

    /// <summary>
    /// 如果最近的敌人处于击倒状态，则返回true
    /// </summary>
    /// <returns></returns>
    bool NearbyEnemyDown()
    {
        float distance = GroundAttackDistance;
        GameObject closestEnemy = null;
        //TODO:敌人处于击倒状态
        return false;
    }

    /// <summary>
    /// 如果玩家面对的是游戏对象，则返回true
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public bool IsFacingTarget(GameObject go)
    {
        return ((go.transform.position.x > transform.position.x && currentDirection == DIRECTION.Left) || (go.transform.position.x < transform.position.x && currentDirection == DIRECTION.Right));
    }

    /// <summary>
    /// 判断是否为地面
    /// </summary>
    /// <returns></returns>
    public bool IsGrounded()
    {
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        float colliderSize = capsuleCollider.bounds.extents.y;
#if UNITY_EDITOR
        Debug.DrawRay(transform.position + capsuleCollider.center, Vector3.down * colliderSize, Color.red);
#endif
        return Physics.Raycast(transform.position + capsuleCollider.center, Vector3.down, colliderSize + 0.1f, 1 << EnvironmentLayer);
    }

    /// <summary>
    /// 返回当前武器
    /// </summary>
    /// <returns></returns>
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Death()
    {
        if (!isDead)
        {
            isDead = true;
            StopAllCoroutines();
            playerAnimator.StopAllCoroutines();
            CancelInvoke();
            SetVelocity(Vector3.zero);
            //TODO:死亡音乐
            playerAnimator.SetAnimatorBool("Death", true);
            //TODO:敌人管理
            StartCoroutine(ReStartLevel());
        }
    }

    /// <summary>
    /// 关卡重来
    /// </summary>
    /// <returns></returns>
    IEnumerator ReStartLevel()
    {
        yield return new WaitForSeconds(2);
        float fadeoutTime = 1.3f;

        //TODO:UI管理

    }
}
