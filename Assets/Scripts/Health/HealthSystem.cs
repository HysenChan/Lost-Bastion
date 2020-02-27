using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int MaxHp = 100;
    public int CurrentHp = 100;
    public bool invulnerable;//是否无敌(用于测试)
    public delegate void OnHealthChange(float percentage, GameObject GO);
    public static event OnHealthChange onHealthChange;

    private void Start()
    {
        SendUpdateEvent();
    }

    /// <summary>
    /// 生命值减少
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void SubstractHealth(int damage)
    {
        if (!invulnerable)
        {
            CurrentHp = Mathf.Clamp(CurrentHp -= damage, 0, MaxHp);

            if (IsDead())
            {
                gameObject.SendMessage("Death", SendMessageOptions.DontRequireReceiver);
            }
        }
        //更新血量
        SendUpdateEvent();
    }

    /// <summary>
    /// 生命值增加
    /// </summary>
    /// <param name="amount">补给量</param>
    public void AddHealth(int amount)
    {
        CurrentHp = Mathf.Clamp(CurrentHp += amount, 0, MaxHp);
        SendUpdateEvent();
    }

    /// <summary>
    /// 生命值更新事件
    /// </summary>
    void SendUpdateEvent()
    {
        float CurrentHealthPercentage = 1f / MaxHp * CurrentHp;
        if (onHealthChange != null)
        {
            onHealthChange(CurrentHealthPercentage, gameObject);
        }
    }

    /// <summary>
    /// 判断是否死亡
    /// </summary>
    /// <returns></returns>
    bool IsDead()
    {
        return CurrentHp == 0;
    }
}
