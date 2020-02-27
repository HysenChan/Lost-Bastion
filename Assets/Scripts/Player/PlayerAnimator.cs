using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//需要Animator组件
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [HideInInspector]
    public DIRECTION currentDirection;

    [Header("Effects")]
    public GameObject DustEffectLand;
    public GameObject DustEffectJump;
    public GameObject HitEffect;
    public GameObject DefendEffect;

    [HideInInspector]
    public Animator animator;
    private bool isPlayer;

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
            transform.parent.GetComponent<PlayerCombat>().Ready();
        }
        else
        {
            //TODO:敌人AI
        }
    }

    /// <summary>
    /// 检查是否有东西被击中
    /// </summary>
    public void Check4Hit()
    {
        //检查玩家是否击中了东西
        if (isPlayer)
        {
            PlayerCombat playerCombat = transform.parent.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.CheckForHit();
            }
            else
            {
                Debug.Log("找不到组件 '" + transform.parent.name + "'.");
            }
        }
        else
        {
            //TODO:检查AI是否击中
        }
    }

    /// <summary>
    /// 展示打击特效
    /// </summary>
    public void ShowHitEffect()
    {
        float unitHeight = 1.6f;
        GameObject.Instantiate(HitEffect, transform.position + Vector3.up * unitHeight, Quaternion.identity);
    }

    /// <summary>
    /// 展示防御特效
    /// </summary>
    public void ShowDefendEffect()
    {
        GameObject.Instantiate(DefendEffect, transform.position + Vector3.up * 1.3f, Quaternion.identity);
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

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="sfxName"></param>
    public void PlaySFX(string sfxName)
    {
        GlobalAudioPlayer.PlaySFXAtPosition(sfxName, transform.position + Vector3.up);
    }

    /// <summary>
    /// 增加了很小的前向力
    /// </summary>
    /// <param name="force">力</param>
    public void AddForce(float force)
    {
        StartCoroutine(AddForceCoroutine(force));
    }

    IEnumerator AddForceCoroutine(float force)
    {
        DIRECTION startDir = currentDirection;
        Rigidbody rb = transform.parent.GetComponent<Rigidbody>();
        float speed = 8f;
        float t = 0;

        while (t < 1)
        {
            yield return new WaitForFixedUpdate();
            rb.velocity = Vector2.right * (int)startDir * Mathf.Lerp(force, rb.velocity.y, MathUtilities.Sinerp(0, 1, t));
            t += Time.fixedDeltaTime * speed;
            yield return null;
        }
    }

    /// <summary>
    /// 闪烁效果
    /// </summary>
    /// <param name="delayBeforeStart"></param>
    /// <returns></returns>
    public IEnumerator FlickerCoroutine(float delayBeforeStart)
    {
        yield return new WaitForSeconds(delayBeforeStart);

        //查找此gameObject中的所有渲染器
        Renderer[] CharRenderers = GetComponentsInChildren<Renderer>();

        if (CharRenderers.Length > 0)
        {
            float t = 0;
            while (t < 1)
            {
                float speed = Mathf.Lerp(15, 35, MathUtilities.Coserp(0, 1, t));
                float i = Mathf.Sin(Time.time * speed);
                foreach (Renderer r in CharRenderers)
                    r.enabled = i > 0;
                t += Time.deltaTime / 2;
                yield return null;
            }
            foreach (Renderer r in CharRenderers)
                r.enabled = false;
        }
        Destroy(transform.parent.gameObject);
    }

    /// <summary>
    /// 相机抖动
    /// </summary>
    /// <param name="intensity"></param>
    public void CamShake(float intensity)
    {
        CameraShake camShake = Camera.main.GetComponent<CameraShake>();
        if (camShake != null)
            camShake.Shake(intensity);
    }

    /// <summary>
    /// 产生子弹
    /// </summary>
    /// <param name="name"></param>
    public void SpawnProjectile(string name)
    {
        GameObject projectile = GameObject.Instantiate(Resources.Load(name)) as GameObject;
        PlayerCombat playerCombat = transform.parent.GetComponent<PlayerCombat>();
        if (playerCombat)
        {
            //将子弹方向设置为玩家方向
            Projectile p = projectile.GetComponent<Projectile>();
            if (p)
            {
                p.direction = playerCombat.currentDirection;
                Weapon currentWeapon = playerCombat.GetCurrentWeapon();
                if (currentWeapon != null)
                {
                    p.SetDamage(playerCombat.GetCurrentWeapon().damageObject);
                }
            }

            //检查此武器是否有生成位置
            ProjectileSpawnPos spawnPos = playerCombat.weaponBone.GetComponentInChildren<ProjectileSpawnPos>();
            if (spawnPos)
            {
                //枪在指定位置生成
                projectile.transform.position = spawnPos.transform.position;
            }
            else
            {
                //把枪放在手的位置
                if (playerCombat.weaponBone)
                {
                    projectile.transform.position = playerCombat.weaponBone.transform.position;
                }
            }
        }
    }
}
