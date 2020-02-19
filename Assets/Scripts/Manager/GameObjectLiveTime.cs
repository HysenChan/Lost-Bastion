using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当前对象的生命周期时间
/// </summary>
public class GameObjectLiveTime : MonoBehaviour
{
    public float LiveTime = 1;

    void Start()
    {
        //LiveTime结束后执行方法
        Invoke("DestoryGameObject", LiveTime);
    }

    void DestoryGameObject()
    {
        Destroy(gameObject);
    }
}
