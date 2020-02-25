using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageObject
{
    public int damage;
    public string animTrigger = "";
    public float duration = 1f;
    public float comboResetTime = 0.5f;//攻击重置事件为0.5s
    public string hitSFX = "";
    //TODO:被击倒之类

    [Header("Hit Collider Settings")]
    public float CollSize;
    public float collDistance;
    public float collHeight;

    [HideInInspector]
    public GameObject inflictor;

    public DamageObject(int damage,GameObject inflictor)
    {
        this.damage = damage;
        this.inflictor = inflictor;
    }
}