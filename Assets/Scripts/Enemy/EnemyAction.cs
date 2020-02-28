using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class EnemyAction : MonoBehaviour
{
    [Header("Linked components")]
    public GameObject target; //当前目标
    public PlayerAnimator playerAnimator; //动画控制器组件
    public GameObject GFX; //音效
    public Rigidbody rb; //刚体组件
    public CapsuleCollider capsule; //胶囊碰撞盒

    [Header("Attack Data")]
    public DamageObject[] AttackList; //攻击列表
    public bool PickRandomAttack; //从列表中选择一个随机攻击
    public float hitZRange = 2; //敌人攻击的z轴范围
    public float defendChance = 0; //抵御即将来临的攻击几率（百分比）
    public float hitRecoveryTime = 0.4f; //击中玩家后多久才可以继续采取行动
    public float standUpTime = 1.1f; //敌人站起来所需的时间
    public bool canDefendDuringAttack; //如果敌人在进行自己的攻击时能够防御来袭，则为真
    public bool AttackPlayerAirborne; //攻击在空中的玩家
    private DamageObject lastAttack; //最近的一次攻击
    private int AttackCounter = 0; //当前攻击次数
    public bool canHitEnemies; //这个敌人可以打到其他敌人
    public bool canHitDestroyableObjects; //敌人可以击中诸如板条箱，木桶之类的可摧毁物体。
    [HideInInspector]
    public float lastAttackTime; //上次攻击时间

    [Header("Settings")]
    public bool pickARandomName; //分配一个随机名称
    public TextAsset enemyNamesList; //敌人名字列表
    public string enemyName = ""; //这个敌人的名字
    public float attackRangeDistance = 1.4f; //敌人能够攻击目标的距离
    public float closeRangeDistance = 2f; //距目标近距离的距离
    public float midRangeDistance = 3f; //距目标中距离的距离
    public float farRangeDistance = 4.5f; //距目标远距离的距离
    public float RangeMarging = 1f; //重新定位之前，玩家和敌人之间允许的空间量
    public float walkSpeed = 1.95f; //行走速度
    public float walkBackwardSpeed = 1.2f; //向后走的速度
    public float sightDistance = 10f; //可以看到目标的距离
    public float attackInterval = 1.2f; //攻击的时间间隔
    public float rotationSpeed = 15f; //切换方向时的转速
    public float lookaheadDistance; //检查障碍物的距离
    public bool ignoreCliffs; //忽略悬崖检测
    public float KnockdownTimeout = 0f; //击倒后敌人站起来之前的时间
    public float KnockdownUpForce = 5f; //击倒的垂直力
    public float KnockbackForce = 4; //击倒的水平力
    private LayerMask HitLayerMask; //损坏对象的LayerMask
    public LayerMask CollisionLayer; //检查碰撞的层
    public bool randomizeValues = true; //随机化值以避免敌人同步
    [HideInInspector]
    public float zSpreadMultiplier = 2f; //敌人之间Z轴距离的乘数

    [Header("Stats")]
    public RANGE range;
    public ENEMYTACTIC enemyTactic;
    public PLAYERSTATE enemyState;
    public DIRECTION currentDirection;
    public bool targetSpotted;
    public bool wallspotted;
    public bool cliffSpotted;
    public bool isGrounded;
    public bool isDead;
    private Vector3 moveDirection;
    public float distance;
    private Vector3 fixedVelocity;
    private bool updateVelocity;

    [HideInInspector]
    public float ZSpread; //Z轴上敌人之间的距离

    public Vector3 distanceToTarget;

    //敌人无法移动的状态列表
    private List<PLAYERSTATE> NoMovementStates = new List<PLAYERSTATE> {
    PLAYERSTATE.DEATH,
    PLAYERSTATE.ATTACK,
    PLAYERSTATE.DEFEND,
    PLAYERSTATE.GROUNDHIT,
    PLAYERSTATE.HIT,
    PLAYERSTATE.IDLE,
    PLAYERSTATE.KNOCKDOWNGROUNDED,
    PLAYERSTATE.STANDUP,
  };

    //可以击中敌人的状态列表
    private List<PLAYERSTATE> HitableStates = new List<PLAYERSTATE> {
    PLAYERSTATE.ATTACK,
    PLAYERSTATE.DEFEND,
    PLAYERSTATE.HIT,
    PLAYERSTATE.IDLE,
    PLAYERSTATE.KICK,
    PLAYERSTATE.PUNCH,
    PLAYERSTATE.STANDUP,
    PLAYERSTATE.WALK,
    PLAYERSTATE.KNOCKDOWNGROUNDED,
  };

    //敌人能够防御来袭攻击的状态列表
    private List<PLAYERSTATE> defendableStates = new List<PLAYERSTATE>
    {
        PLAYERSTATE.IDLE,
        PLAYERSTATE.WALK,
        PLAYERSTATE.DEFEND
    };

    //委托敌人的全局事件处理
    public delegate void UnitEventHandler(GameObject Unit);

    //销毁单位的全局事件
    public static event UnitEventHandler OnUnitDestroy;

    public void OnStart()
    {
        //给这个敌人起个名字
        if (pickARandomName)
        {
            enemyName = GetRandomName();
        }

        //设置玩家作为敌人的目标
        if (target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }

        //通知EnemyManager更新活动敌人的列表
        EnemyManager.GetActiveEnemies();

        //在攻击期间启用防御
        if (canDefendDuringAttack)
        {
            defendableStates.Add(PLAYERSTATE.ATTACK);
        }

        //设置HitLayerMask
        HitLayerMask = 1 << LayerMask.NameToLayer("Player");
        if (canHitEnemies)
        {
            HitLayerMask |= (1 << LayerMask.NameToLayer("Enemy"));
        }
        if (canHitDestroyableObjects)
        {
            HitLayerMask |= (1 << LayerMask.NameToLayer("DestroyableObject"));
        }
    }

    #region 更新

    public void OnLateUpdate()
    {
        //将任何根运动偏移量应用于父级
        if (playerAnimator && playerAnimator.GetComponent<Animator>().applyRootMotion && playerAnimator.transform.localPosition != Vector3.zero)
        {
            Vector3 offset = playerAnimator.transform.localPosition;
            playerAnimator.transform.localPosition = Vector3.zero;
            transform.position += offset * (int)currentDirection;
        }
    }

    public void OnFixedUpdate()
    {
        if (updateVelocity)
        {
            rb.velocity = fixedVelocity;
            updateVelocity = false;
        }
    }

    private void SetVelocity(Vector3 velocity)
    {
        fixedVelocity = velocity;
        updateVelocity = true;
    }

    #endregion

    #region 攻击

    public void ATTACK()
    {
        //玩家跳跃时不要攻击
        var playerMovement = target.GetComponent<PlayerMovement>();
        if (!AttackPlayerAirborne && playerMovement != null && playerMovement.jumpInProgress)
        {
            return;
        }
        else
        {
            enemyState = PLAYERSTATE.ATTACK;
            Move(Vector3.zero, 0f);
            LookAtTarget(target.transform);
            TurnToDir(currentDirection);

            //选择随机攻击
            if (PickRandomAttack)
            {
                AttackCounter = Random.Range(0, AttackList.Length);
            }

            //播放动画
            playerAnimator.SetAnimatorTrigger(AttackList[AttackCounter].animTrigger);

            //转到列表中的下一个攻击
            if (!PickRandomAttack)
            {
                AttackCounter += 1;
                if (AttackCounter >= AttackList.Length)
                {
                    AttackCounter = 0;
                }
            }

            lastAttackTime = Time.time;
            lastAttack = AttackList[AttackCounter];
            lastAttack.inflictor = gameObject;

            //恢复
            Invoke("Ready", AttackList[AttackCounter].duration);
        }
    }

    #endregion

    #region 移动

    /// <summary>
    /// 向目标移动
    /// </summary>
    /// <param name="proximityRange">接近范围</param>
    /// <param name="movementMargin"></param>
    public void WalkTo(float proximityRange, float movementMargin)
    {
        Vector3 dirToTarget;
        LookAtTarget(target.transform);
        enemyState = PLAYERSTATE.WALK;

        //接合时将zspread夹在AttackDistance上，否则可能根本无法到达玩家位置
        if (enemyTactic == ENEMYTACTIC.ENGAGE)
        {
            dirToTarget = target.transform.position - (transform.position + new Vector3(0, 0, Mathf.Clamp(ZSpread, 0, attackRangeDistance)));
        }
        else
        {
            dirToTarget = target.transform.position - (transform.position + new Vector3(0, 0, ZSpread));
        }

        //距离太远
        if (distance >= proximityRange)
        {
            moveDirection = new Vector3(dirToTarget.x, 0, dirToTarget.z);
            if (IsGrounded() && !WallSpotted() && !PitfallSpotted())
            {
                Move(moveDirection.normalized, walkSpeed);
                playerAnimator.SetAnimatorFloat("MovementSpeed", rb.velocity.sqrMagnitude);
                return;
            }
        }

        //距离太近
        if (distance <= proximityRange - movementMargin)
        {
            moveDirection = new Vector3(-dirToTarget.x, 0, 0);
            if (IsGrounded() && !WallSpotted() && !PitfallSpotted())
            {
                Move(moveDirection.normalized, walkBackwardSpeed);
                playerAnimator.SetAnimatorFloat("MovementSpeed", -rb.velocity.sqrMagnitude);
                return;
            }
        }

        Move(Vector3.zero, 0f);
        playerAnimator.SetAnimatorFloat("MovementSpeed", 0);
    }

    public void Move(Vector3 vector, float speed)
    {
        if (!NoMovementStates.Contains(enemyState))
        {
            SetVelocity(new Vector3(vector.x * speed, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, vector.z * speed));
        }
        else
        {
            SetVelocity(Vector3.zero);
        }
    }

    /// <summary>
    /// 发现墙体碰撞盒，则返回true
    /// </summary>
    /// <returns></returns>
    private bool WallSpotted()
    {
        Vector3 Offset = moveDirection.normalized * lookaheadDistance;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + capsule.center + Offset, capsule.radius, CollisionLayer);

        int i = 0;
        bool hasHitwall = false;
        while (i < hitColliders.Length)
        {
            if (CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer))
            {
                hasHitwall = true;
            }
            i++;
        }
        wallspotted = hasHitwall;
        return hasHitwall;
    }

    /// <summary>
    /// 发现前面有悬崖，则返回true
    /// </summary>
    /// <returns></returns>
    private bool PitfallSpotted()
    {
        if (!ignoreCliffs)
        {
            float lookDownDistance = 1f;
            Vector3 StartPoint = transform.position + (Vector3.up * .3f) + (Vector3.right * (capsule.radius + lookaheadDistance) * moveDirection.normalized.x);
            RaycastHit hit;

#if UNITY_EDITOR
            Debug.DrawRay(StartPoint, Vector3.down * lookDownDistance, Color.red);
#endif

            if (!Physics.Raycast(StartPoint, Vector3.down, out hit, lookDownDistance, CollisionLayer))
            {
                cliffSpotted = true;
                return true;
            }
        }
        cliffSpotted = false;
        return false;
    }

    /// <summary>
    /// 返回是否在地面上
    /// </summary>
    /// <returns></returns>
    public bool IsGrounded()
    {
        float colliderSize = capsule.bounds.extents.y - .1f;
        if (Physics.CheckCapsule(capsule.bounds.center, capsule.bounds.center + Vector3.down * colliderSize, capsule.radius, CollisionLayer))
        {
            isGrounded = true;
            return true;
        }
        else
        {
            isGrounded = false;
            return false;
        }
    }

    /// <summary>
    /// 转向
    /// </summary>
    /// <param name="dir"></param>
    public void TurnToDir(DIRECTION dir)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward * (int)dir);
    }

    #endregion

    #region 被攻击

    /// <summary>
    /// 被击中
    /// </summary>
    /// <param name="d"></param>
    public void Hit(DamageObject d)
    {
        if (HitableStates.Contains(enemyState))
        {
            //只在敌人被击倒时才允许地面攻击击中
            if (enemyState == PLAYERSTATE.KNOCKDOWNGROUNDED && !d.isGroundAttack)
                return;

            CancelInvoke();
            StopAllCoroutines();
            playerAnimator.StopAllCoroutines();
            Move(Vector3.zero, 0f);

            //增加攻击时间，使玩家被击中后无法立即攻击
            lastAttackTime = Time.time;

            //当敌人在空中时不能被击中
            if ((enemyState == PLAYERSTATE.KNOCKDOWNGROUNDED || enemyState == PLAYERSTATE.GROUNDHIT) && !d.isGroundAttack)
                return;

            //防御
            if (!d.DefenceOverride && defendableStates.Contains(enemyState))
            {
                int rand = Random.Range(0, 100);
                if (rand < defendChance)
                {
                    Defend();
                    return;
                }
            }

            //受伤音效
            GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position);

            //被命中后展示特效
            ShowHitEffectAtPosition(new Vector3(transform.position.x, d.inflictor.transform.position.y + d.collHeight, transform.position.z));

            //相机抖动
            CameraShake camShake = Camera.main.GetComponent<CameraShake>();
            if (camShake != null)
                camShake.Shake(0.1f);

            //激活慢动作相机
            if (d.slowMotionEffect)
            {
                CameraSlowMotion cmd = Camera.main.GetComponent<CameraSlowMotion>();
                if (cmd != null)
                    cmd.StartSlowMotionDelay(.2f);
            }

            //减少生命值
            HealthSystem hs = GetComponent<HealthSystem>();
            if (hs != null)
            {
                hs.SubstractHealth(d.damageCount);
                if (hs.CurrentHp == 0)
                    return;
            }

            //地面攻击
            if (enemyState == PLAYERSTATE.KNOCKDOWNGROUNDED)
            {
                StopAllCoroutines();
                enemyState = PLAYERSTATE.GROUNDHIT;
                StartCoroutine(GroundHit());
                return;
            }

            //转向攻击来袭的方向
            int dir = d.inflictor.transform.position.x > transform.position.x ? 1 : -1;
            TurnToDir((DIRECTION)dir);

            //检查是否被击倒
            if (d.knockDown)
            {
                StartCoroutine(KnockDownSequence(d.inflictor));
                return;
            }
            else
            {
                //默认攻击
                int rand = Random.Range(1, 3);
                playerAnimator.SetAnimatorTrigger("Hit" + rand);
                enemyState = PLAYERSTATE.HIT;

                //从冲击中增加很小的力量
                LookAtTarget(d.inflictor.transform);
                playerAnimator.AddForce(-KnockbackForce);

                //受到攻击时将敌人的状态从被动变为主动
                if (enemyTactic != ENEMYTACTIC.ENGAGE)
                {
                    EnemyManager.SetAgressive(gameObject);
                }

                Invoke("Ready", hitRecoveryTime);
                return;
            }
        }
    }

    /// <summary>
    /// 防御
    /// </summary>
    private void Defend()
    {
        enemyState = PLAYERSTATE.DEFEND;
        playerAnimator.ShowDefendEffect();
        playerAnimator.SetAnimatorTrigger("Defend");
        GlobalAudioPlayer.PlaySFX("DefendHit");
        playerAnimator.SetDirection(currentDirection);
    }

    #endregion

    #region 检查击中状态

    /// <summary>
    /// 检查敌人是否击中了某些物体（动画事件）
    /// </summary>
    public void CheckForHit()
    {
        //在敌人前面绘制一个攻击范围盒，以查看哪些对象与它重叠
        Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
        Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
        Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            //击中一个可损坏物品
            IDamagable<DamageObject> damagableObject = hitColliders[i].GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
            if (damagableObject != null && damagableObject != (IDamagable<DamageObject>)this)
            {
                damagableObject.Hit(lastAttack);
            }
            i++;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在Unity编辑器中显示攻击范围框+前瞻球体（调试）
    /// </summary>
    private void OnDrawGizmos()
    {
        //可视化攻击范围框
        if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration)
        {
            Gizmos.color = Color.red;
            Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) + Vector3.right * ((int)currentDirection * lastAttack.collDistance);
            Vector3 boxSize = new Vector3(lastAttack.CollSize, lastAttack.CollSize, hitZRange);
            Gizmos.DrawWireCube(boxPosition, boxSize);
        }

        //可视化前瞻球体
        Gizmos.color = Color.yellow;
        Vector3 offset = -moveDirection.normalized * lookaheadDistance;
        Gizmos.DrawWireSphere(transform.position + capsule.center - offset, capsule.radius);
    }

#endif

    #endregion

    #region 倒地

    IEnumerator KnockDownSequence(GameObject inflictor)
    {
        enemyState = PLAYERSTATE.KNOCKDOWN;
        yield return new WaitForFixedUpdate();

        //朝着即将来临攻击的方向
        int dir = 1;
        if (inflictor != null)
        {
            dir = inflictor.transform.position.x > transform.position.x ? 1 : -1;
        }
        currentDirection = (DIRECTION)dir;
        playerAnimator.SetDirection(currentDirection);
        TurnToDir(currentDirection);

        //增加击退力
        playerAnimator.SetAnimatorTrigger("KnockDown_Up");
        while (IsGrounded())
        {
            SetVelocity(new Vector3(KnockbackForce * -dir, KnockdownUpForce, 0));
            yield return new WaitForFixedUpdate();
        }

        while (rb.velocity.y >= 0)
        {
            yield return new WaitForFixedUpdate();
        }

        playerAnimator.SetAnimatorTrigger("KnockDown_Down");

        while (!IsGrounded())
        {
            yield return new WaitForFixedUpdate();
        }

        //击中地面
        playerAnimator.SetAnimatorTrigger("KnockDown_End");
        GlobalAudioPlayer.PlaySFXAtPosition("Drop", transform.position);
        playerAnimator.SetAnimatorFloat("MovementSpeed", 0f);
        playerAnimator.ShowDustEffectLand();
        enemyState = PLAYERSTATE.KNOCKDOWNGROUNDED;
        Move(Vector3.zero, 0f);

        //相机抖动
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        if (camShake != null)
        {
            camShake.Shake(.3f);
        }

        //播放倒地扬起尘埃的特效
        playerAnimator.ShowDustEffectLand();

        //停止滑动
        float t = 0;
        float speed = 2;
        Vector3 fromVelocity = rb.velocity;
        while (t < 1)
        {
            SetVelocity(Vector3.Lerp(new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z), new Vector3(0, rb.velocity.y, 0), t));
            t += Time.deltaTime * speed;
            yield return new WaitForFixedUpdate();
        }

        //倒地时间
        Move(Vector3.zero, 0f);
        yield return new WaitForSeconds(KnockdownTimeout);

        //站立起来
        enemyState = PLAYERSTATE.STANDUP;
        playerAnimator.SetAnimatorTrigger("StandUp");
        Invoke("Ready", standUpTime);
    }

    /// <summary>
    /// 地面攻击
    /// </summary>
    /// <returns></returns>
    public IEnumerator GroundHit()
    {
        CancelInvoke();
        GlobalAudioPlayer.PlaySFXAtPosition("EnemyGroundPunchHit", transform.position);
        playerAnimator.SetAnimatorTrigger("GroundHit");
        yield return new WaitForSeconds(KnockdownTimeout);
        if (!isDead)
        {
            playerAnimator.SetAnimatorTrigger("StandUp");
        }
        Invoke("Ready", standUpTime);
    }
    #endregion

    /// <summary>
    /// 敌人已准备
    /// </summary>
    public void Ready()
    {
        enemyState = PLAYERSTATE.IDLE;
        playerAnimator.SetAnimatorTrigger("Idle");
        playerAnimator.SetAnimatorFloat("MovementSpeed", 0f);
        Move(Vector3.zero, 0f);
    }

    /// <summary>
    /// 展示被打击的特效
    /// </summary>
    /// <param name="pos">特效播放位置</param>
    public void ShowHitEffectAtPosition(Vector3 pos)
    {
        GameObject.Instantiate(Resources.Load("HitEffect"), pos, Quaternion.identity);
    }

    /// <summary>
    /// 敌人方向朝向目标方向
    /// </summary>
    /// <param name="_target"></param>
    public void LookAtTarget(Transform _target)
    {
        if (_target != null)
        {
            Vector3 newDir = Vector3.zero;
            int dir = _target.transform.position.x >= transform.position.x ? 1 : -1;
            currentDirection = (DIRECTION)dir;
            if (playerAnimator != null)
            {
                playerAnimator.currentDirection = currentDirection;
            }
            newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * dir, rotationSpeed * Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    /// <summary>
    /// 获得随机名称
    /// </summary>
    /// <returns></returns>
    private string GetRandomName()
    {
        if (enemyNamesList == null)
        {
            Debug.Log("找不到'EnemyNames.txt'文件，请创建并放于Resources文件夹内！");
            return "";
        }

        //将txt文件的行转换为数组
        string data = enemyNamesList.ToString();
        string cReturns = System.Environment.NewLine + "\n" + "\r";
        string[] lines = data.Split(cReturns.ToCharArray());

        //从列表中选择一个随机名称
        string name = "";
        int cnt = 0;
        while (name.Length == 0 && cnt < 100)
        {
            int rand = Random.Range(0, lines.Length);
            name = lines[rand];
            cnt += 1;
        }

        return name;
    }

    /// <summary>
    /// 销毁敌人事件
    /// </summary>
    public void DestroyUnit()
    {
        if (OnUnitDestroy != null)
        {
            OnUnitDestroy(gameObject);
        }
    }

    /// <summary>
    /// 随机化值
    /// </summary>
    public void SetRandomValues()
    {
        walkSpeed *= Random.Range(.8f, 1.2f);
        walkBackwardSpeed *= Random.Range(.8f, 1.2f);
        attackInterval *= Random.Range(.7f, 1.5f);
        KnockdownTimeout *= Random.Range(.7f, 1.5f);
        KnockdownUpForce *= Random.Range(.8f, 1.2f);
        KnockbackForce *= Random.Range(.7f, 1.5f);
    }
}
