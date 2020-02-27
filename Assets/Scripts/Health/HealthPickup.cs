using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HealthPickup : MonoBehaviour
{
    public int RestoreHP = 0;
    public string pickupSFX = "";
    public GameObject pickupEffect;
    public float pickUpRange = 1;

    private GameObject[] Players;

    private void Start()
    {
        Players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void LateUpdate()
    {
        foreach (GameObject player in Players)
        {
            if (player)
            {
                float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

                //玩家在交互范围内
                if (distanceToPlayer< pickUpRange)
                {
                    AddHealthToPlayer(player);
                }
            }
        }
    }

    /// <summary>
    /// 玩家增加血量
    /// </summary>
    /// <param name="player"></param>
    private void AddHealthToPlayer(GameObject player)
    {
        HealthSystem hs = player.GetComponent<HealthSystem>();

        //调用HealthSystem添加血量方法
        if (hs!=null)
        {
            hs.AddHealth(RestoreHP);
        }
        else
        {
            Debug.Log(player.gameObject.name + "找不到HealthSystem组件！");
        }

        //播放音效
        if (pickupSFX!="")
        {
            GlobalAudioPlayer.PlaySFXAtPosition(pickupSFX, transform.position);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
    }
#endif

}
