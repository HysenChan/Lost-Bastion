using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : EnemyAction, IDamagable<DamageObject>
{
    [Space(10)]
    public bool enableAI;

    //执行AI的状态列表
    private List<PLAYERSTATE> ActiveAIStates = new List<PLAYERSTATE> {
    PLAYERSTATE.IDLE,
    PLAYERSTATE.WALK
  };

    private void Start()
    {
        //将此敌人添加到敌人列表中
        EnemyManager.enemyList.Add(gameObject);

        //设置Z轴（Zspread用于保持敌人之间的空间）
        ZSpread = (EnemyManager.enemyList.Count - 1);
        Invoke("SetZSpread", .1f);

        //随机化值以避免同步移动
        if (randomizeValues)
        {
            SetRandomValues();
        }

        OnStart();
    }

    void FixedUpdate()
    {
        OnFixedUpdate();
    }

    void LateUpdate()
    {
        OnLateUpdate();
    }

    private void Update()
    {
        //没有目标或禁用AI时不执行任何操作
        if (target == null || !enableAI)
        {
            Ready();
            return;

        }
        else
        {
            //获取目标范围
            range = GetDistanceToTarget();
        }

        if (!isDead && enableAI)
        {
            if (ActiveAIStates.Contains(enemyState) && targetSpotted)
            {
                //主动式AI
                AI();
            }
            else
            {
                //尝试发现玩家
                if (distanceToTarget.magnitude < sightDistance)
                {
                    targetSpotted = true;
                }
            }
        }
    }

    private void AI()
    {
        LookAtTarget(target.transform);

        if (range==RANGE.ATTACKRANGE)
        {
            //攻击目标
            if (!cliffSpotted)
            {
                if (Time.time-lastAttackTime>attackInterval)
                {
                    ATTACK();
                }
                else
                {
                    Ready();
                }
                return;
            }

            //ATTACKRANGE距离的动作
            if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE)
            {
                WalkTo(closeRangeDistance, 0f);
            }
            if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE)
            {
                WalkTo(midRangeDistance, RangeMarging);
            }
            if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE)
            {
                WalkTo(farRangeDistance, RangeMarging);
            }
            if (enemyTactic == ENEMYTACTIC.STANDSTILL)
            {
                Ready();
            }
        }
        else
        {
            //CLOSERANGE，MIDRANGE和FARRANGE距离的操作
            if (enemyTactic == ENEMYTACTIC.ENGAGE)
            {
                WalkTo(attackRangeDistance, 0f);
            }
            if (enemyTactic == ENEMYTACTIC.KEEPCLOSEDISTANCE)
            {
                WalkTo(closeRangeDistance, RangeMarging);
            }
            if (enemyTactic == ENEMYTACTIC.KEEPMEDIUMDISTANCE)
            {
                WalkTo(midRangeDistance, RangeMarging);
            }
            if (enemyTactic == ENEMYTACTIC.KEEPFARDISTANCE)
            {
                WalkTo(farRangeDistance, RangeMarging);
            }
            if (enemyTactic == ENEMYTACTIC.STANDSTILL)
            {
                Ready();
            }
        }
    }

    /// <summary>
    /// 更新当前范围
    /// </summary>
    /// <returns></returns>
    private RANGE GetDistanceToTarget()
    {
        if (target != null)
        {
            //距目标的距离
            distanceToTarget = target.transform.position - transform.position;
            distance = Vector3.Distance(target.transform.position, transform.position);

            float distX = Mathf.Abs(distanceToTarget.x);
            float distZ = Mathf.Abs(distanceToTarget.z);

            //攻击范围
            if (distX <= attackRangeDistance)
            {
                if (distZ < (hitZRange / 2f))
                    return RANGE.ATTACKRANGE;
                else
                    return RANGE.CLOSERANGE;
            }

            //近距离
            if (distX > attackRangeDistance && distX < midRangeDistance)
            {
                return RANGE.CLOSERANGE;
            }

            //中距离
            if (distX > closeRangeDistance && distance < farRangeDistance)
            {
                return RANGE.MIDRANGE;
            }

            //远距离
            if (distX > farRangeDistance)
            {
                return RANGE.FARRANGE;
            }

        }
        return RANGE.FARRANGE;
    }

    /// <summary>
    /// 制定敌人的战术
    /// </summary>
    /// <param name="tactic"></param>
    public void SetEnemyTactic(ENEMYTACTIC tactic)
    {
        enemyTactic = tactic;
    }

    /// <summary>
    /// 在Z轴范围内散布敌人
    /// </summary>
    private void SetZSpread()
    {
        ZSpread = (ZSpread - (float)(EnemyManager.enemyList.Count - 1) / 2f) * (capsule.radius * 2) * zSpreadMultiplier;
        if (ZSpread > attackRangeDistance)
        {
            ZSpread = attackRangeDistance - 0.1f;
        }
    }

    /// <summary>
    /// 敌人单位死亡
    /// </summary>
    private void Death()
    {
        StopAllCoroutines();
        CancelInvoke();

        enableAI = false;
        isDead = true;
        playerAnimator.SetAnimatorBool("isDead", true);
        Move(Vector3.zero, 0);
        EnemyManager.RemoveEnemyFromList(gameObject);
        gameObject.layer = LayerMask.NameToLayer("Default");

        //地面上死亡
        if (enemyState == PLAYERSTATE.KNOCKDOWNGROUNDED)
        {
            StartCoroutine(GroundHit());
        }
        else
        {
            //正常死亡
            playerAnimator.SetAnimatorTrigger("Death");
        }

        GlobalAudioPlayer.PlaySFXAtPosition("EnemyDeath", transform.position);
        StartCoroutine(playerAnimator.FlickerCoroutine(2));
        enemyState = PLAYERSTATE.DEATH;
        DestroyUnit();
    }
}

public enum ENEMYTACTIC
{
    ENGAGE = 0,
    KEEPCLOSEDISTANCE = 1,
    KEEPMEDIUMDISTANCE = 2,
    KEEPFARDISTANCE = 3,
    STANDSTILL = 4,
}

public enum RANGE
{
    ATTACKRANGE,
    CLOSERANGE,
    MIDRANGE,
    FARRANGE,
}
