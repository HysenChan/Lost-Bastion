using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerState))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Linked Components")]
    private PlayerAnimator playerAnimator;//拿到动画制作器组件
    private PlayerState playerState;//拿到玩家的状态
    private Rigidbody rb;//拿到刚体
    private InputManager inputManager;//拿到输入管理器

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
    }
}
