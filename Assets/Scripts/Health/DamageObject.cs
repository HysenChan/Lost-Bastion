using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageObject
{
    public int damageCount;
    public string animTrigger = "";
    public float duration = 1f;
    public float comboResetTime = 0.5f;//攻击重置事件为0.5s
    public string hitSFX = "";
    public bool knockDown;
    public bool slowMotionEffect;
    public bool DefenceOverride;
    public bool isGroundAttack;

    [Header("Hit Collider Settings")]
    public float CollSize;
    public float collDistance;
    public float collHeight;

    [HideInInspector]
    public GameObject inflictor;

    public DamageObject(int damageCount,GameObject inflictor)
    {
        this.damageCount = damageCount;
        this.inflictor = inflictor;
    }
}