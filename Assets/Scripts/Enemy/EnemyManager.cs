using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager
{
    public static List<GameObject> enemyList = new List<GameObject>(); //当前关卡的敌人总数
    public static List<GameObject> enemiesAttackingPlayer = new List<GameObject>(); //当前正在攻击的敌人列表
    public static List<GameObject> activeEnemies = new List<GameObject>(); //当前活动的敌人列表

    /// <summary>
    /// 从敌人名单中移除一个敌人
    /// </summary>
    /// <param name="gameObject"></param>
    public static void RemoveEnemyFromList(GameObject gameObject)
    {
        enemyList.Remove(gameObject);
        SetEnemyTactics();
    }

    /// <summary>
    /// 为当前波次中的每个敌人设定战术
    /// </summary>
    public static void SetEnemyTactics()
    {
        GetActiveEnemies();
        if (activeEnemies.Count > 0)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                if (i < MaxEnemyAttacking())
                {
                    activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);
                }
                else
                {
                    activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.KEEPMEDIUMDISTANCE);
                }
            }
        }
    }

    /// <summary>
    /// 敌人使用某种战术
    /// </summary>
    /// <param name="tactic"></param>
    public static void ForceEnemyTactic(ENEMYTACTIC tactic)
    {
        GetActiveEnemies();
        if (activeEnemies.Count > 0)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                activeEnemies[i].GetComponent<EnemyAI>().SetEnemyTactic(tactic);
            }
        }
    }

    /// <summary>
    /// 禁用所有敌人AI
    /// </summary>
    public static void DisableAllEnemyAIs()
    {
        GetActiveEnemies();
        if (activeEnemies.Count > 0)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                activeEnemies[i].GetComponent<EnemyAI>().enableAI = false;
            }
        }
    }

    /// <summary>
    /// 返回当前活跃的敌人列表
    /// </summary>
    public static void GetActiveEnemies()
    {
        activeEnemies.Clear();
        foreach (GameObject enemy in enemyList)
        {
            if (enemy != null && enemy.activeSelf)
            {
                activeEnemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// 玩家已死亡时，则禁用所有敌人
    /// </summary>
    public static void PlayerHasDied()
    {
        DisableAllEnemyAIs();
        enemyList.Clear();
    }

    /// <summary>
    /// 返回一次可以攻击玩家的最大敌人数（工具/游戏设置）
    /// </summary>
    /// <returns></returns>
    static int MaxEnemyAttacking()
    {
        EnemyWaveSystem EWS = GameObject.FindObjectOfType<EnemyWaveSystem>();
        if (EWS != null) return EWS.MaxAttackers;
        return 3;
    }

    /// <summary>
    /// 将敌人置于进攻状态
    /// </summary>
    /// <param name="enemy"></param>
    public static void SetAgressive(GameObject enemy)
    {
        enemy.GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.ENGAGE);

        //set another enemy to passive state
        foreach (GameObject e in activeEnemies)
        {
            if (e != enemy)
            {
                e.GetComponent<EnemyAI>().SetEnemyTactic(ENEMYTACTIC.KEEPMEDIUMDISTANCE);
                return;
            }
        }
    }
}
