using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public GameObject playerHandPrefab;//玩家手的预制体
    public GameObject WeaponEndState; // 视觉上代表该武器最终状态的游戏对象（例如，破损的球杆或空枪）

    public int timesToUse = 1;//使用武器的时间
    public DamageObject damageObject;
    [Header("Sound Effects")]
    public string useSound = "";
    public string breakSound = "";

    public WEAPONDAMAGETYPE damageType;//选择此武器是在使用时还是在击中时损毁（例如枪在使用时损毁，球杆在击中时损毁）

    /// <summary>
    /// 使用武器
    /// </summary>
    public void UseWeapon()
    {
        timesToUse = Mathf.Clamp(timesToUse - 1, 0, 1000);
    }

    /// <summary>
    /// 击中物体/敌人
    /// </summary>
    public void OnHitSomething()
    {
        if (damageType==WEAPONDAMAGETYPE.WEAPONONHIT)
        {
            UseWeapon();
        }
        if (timesToUse==1)
        {
            damageObject.hitSFX = breakSound;
        }
    }

    /// <summary>
    /// 损毁的武器
    /// </summary>
    public void BreakWeapon()
    {
        if (WeaponEndState)
        {
            GameObject go = GameObject.Instantiate(WeaponEndState) as GameObject;
            go.transform.position = playerHandPrefab.transform.position;
        }
    }
}

/// <summary>
/// 武器通过什么方式受损(使用中/击中)
/// </summary>
public enum WEAPONDAMAGETYPE
{
    WEAPONONUSE,
    WEAPONONHIT,
}
