using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Weapon weapon;

    private GameObject[] Players;
    private GameObject playerInRange;

    [Header("Pickup Settings")]
    public string SFX = "";
    public GameObject pickupEffect;
    public float pickupRange=1;

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

                //物品是否在角色交互范围内
                if (distanceToPlayer<pickupRange&&playerInRange==null)
                {
                    playerInRange = player;
                    player.SendMessage("ItemInRange", gameObject, SendMessageOptions.DontRequireReceiver);
                    return;
                }

                //物品超出角色交互范围
                if (distanceToPlayer>pickupRange&&playerInRange!=null)
                {
                    player.SendMessage("ItemOutRange", gameObject, SendMessageOptions.DontRequireReceiver);
                    playerInRange = null;
                }
            }
        }
    }

    //展示捡起动画
    public void OnPickup(GameObject player)
    {
        if (pickupEffect)
        {
            GameObject effect = GameObject.Instantiate(pickupEffect);
            effect.transform.position = transform.position;
        }

        //TODO:声音
        if (SFX != null)
        {
            
        }

        //武器装扮到玩家手上
        GiveWeaponToPlayer(player);

        //移除
        Destroy(gameObject);
        
    }

    public void GiveWeaponToPlayer(GameObject player)
    {
        PlayerCombat pc = player.GetComponent<PlayerCombat>();
        if (pc)
        {
            pc.EquipWeapon(weapon);
        }
    }

#if UNITY_EDITOR
    //在编辑器状态展示能捡起来的范围
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
#endif
}
