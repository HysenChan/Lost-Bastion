using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float speed = 10;
    public DIRECTION direction;
    public bool destroyOnHit;
    public GameObject EffectOnSpawn;
    private DamageObject damage;

    private void Start()
    {
        GetComponent<Rigidbody>().velocity = new Vector2((int)direction * speed, 0);
        GetComponent<Collider>().isTrigger = true;

        //产生该子弹时显示效果
        if (EffectOnSpawn)
        {
            GameObject effect = GameObject.Instantiate(EffectOnSpawn) as GameObject;
            effect.transform.position = transform.position;
        }
    }

    /// <summary>
    /// 子弹的触发效果
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //击中一个敌人对象
            IDamagable<DamageObject> damagableObject = other.GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
            if (damagableObject!=null)
            {
                damagableObject.Hit(damage);
                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 设置伤害
    /// </summary>
    /// <param name="damageObject"></param>
    public void SetDamage(DamageObject damageObject)
    {
        damage = damageObject;
    }
}
