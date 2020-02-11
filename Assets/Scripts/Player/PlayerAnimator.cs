﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//需要Animator组件
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [HideInInspector]
    public DIRECTION currentDirection;
    public Animator animator;
    private bool isPlayer;

    [Header("Effects")]
    public GameObject DustEffectLand;
    public GameObject DustEffectJump;
    public GameObject HitEffect;
    public GameObject DefendEffect;

    private void Awake()
    {
        if (animator==null)
        {
            animator = GetComponent<Animator>();
        }
        //检查该对象标签是否为Player
        isPlayer = transform.parent.CompareTag("Player");
        //设置角色初始位置为向右
        currentDirection = DIRECTION.Right;
    }

    //在动画控制器设置一个Trigger
    public void SetAnimatorTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    //在动画控制器设置一个bool
    public void SetAnimatorBool(string name,bool state)
    {
        animator.SetBool(name, state);
    }

    //在动画控制器设置一个float
    public void SetAnimatorFloat(string name,float value)
    {
        animator.SetFloat(name, value);
    }

    //设置方向
    public void SetDirection(DIRECTION dir)
    {
        currentDirection = dir;
    }

    //动画准备
    public void Ready()
    {
        if (isPlayer)
        {
            
        }
        else
        {

        }
    }

    /// <summary>
    /// 展示跳跃特效
    /// </summary>
    public void ShowDustEffectJump()
    {
        GameObject.Instantiate(DustEffectJump, transform.position + Vector3.up * 0.13f, Quaternion.identity);
    }

    /// <summary>
    /// 展示着陆特效
    /// </summary>
    public void ShowDustEffectLand()
    {
        GameObject.Instantiate(DustEffectLand, transform.position + Vector3.up * 0.13f, Quaternion.identity);
    }
}
