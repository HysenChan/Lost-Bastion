using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakableObject : MonoBehaviour,IDamagable<DamageObject>
{
    public string hitSFX = "";

    [Header("GameObject Destroyed")]
    public GameObject destroyedGO;
    public bool orientToImpactDir;

    [Header("Spawn an item")]
    public GameObject spawnItem;
    public float spawnChance = 100;

    [Space(10)]
    public bool destroyOnHit;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("DestroyableObject");
    }

    public void Hit(DamageObject damageObject)
    {
        //TODO:添加音效

        //生成破坏的游戏对象版本
        if (destroyedGO!=null)
        {
            GameObject brokenGO = GameObject.Instantiate(destroyedGO);
            brokenGO.transform.position = transform.position;

            //基于影响方向的机会方向
            if (orientToImpactDir&&damageObject.inflictor!=null)
            {
                float dir = Mathf.Sign(damageObject.inflictor.transform.position.x - transform.position.x);
                brokenGO.transform.rotation = Quaternion.LookRotation(Vector3.forward * dir);
            }
        }

        //产生一个物品
        if (spawnItem!=null)
        {
            if (Random.Range(0,100)<spawnChance)
            {
                GameObject item = GameObject.Instantiate(spawnItem);
                item.transform.position = transform.position;

                //对物体加力
                item.GetComponent<Rigidbody>().velocity = Vector3.up * 8f;
            }
        }

        //清除
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
