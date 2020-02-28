using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWaveSystem : MonoBehaviour
{
    public int MaxAttackers = 3; //可以同时攻击玩家的最大敌人数

    [Header("List of enemy Waves")]
    public EnemyWave[] EnemyWaves;
    public int currentWave;

    [Header("Slow Motion Settings")]
    public bool activateSlowMotionOnLastHit;//在最后一击时激活慢动作
    public float effectDuration = 1.5f;//慢动作持续时间

    [Header("Load level On Finish")]
    public bool loadNewLevel;//判断是否加载新关卡
    public string levelName;//新关卡的名称

    private void Awake()
    {
        if (enabled)
        {
            DisableAllEnemies();
        }
    }

    private void Start()
    {
        currentWave = 0;
        UpdateAreaColliders();
        StartNewWave();
    }

    /// <summary>
    /// 杀死所有敌人
    /// </summary>
    void DisableAllEnemies()
    {
        foreach (EnemyWave wave in EnemyWaves)
        {
            for (int i = 0; i < wave.EnemyList.Count; i++)
            {
                if (wave.EnemyList[i] != null)
                {
                    //消灭敌人
                    wave.EnemyList[i].SetActive(false);
                }
                else
                {
                    //从列表中删除空字段
                    wave.EnemyList.RemoveAt(i);
                }
            }
            foreach (GameObject g in wave.EnemyList)
            {
                if (g != null)
                {
                    g.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 发动新的敌人波次
    /// </summary>
    public void StartNewWave()
    {
        //隐藏UI
        HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
        if (hp != null)
        {
            hp.DeActivateHandPointer();
        }

        //激活敌人
        foreach (GameObject g in EnemyWaves[currentWave].EnemyList)
        {
            if (g != null)
            {
                g.SetActive(true);
            }
        }
        Invoke("SetEnemyTactics", .1f);
    }

    /// <summary>
    /// 更新区域碰撞器
    /// </summary>
    void UpdateAreaColliders()
    {
        //将当前区域碰撞器切换到触发器
        if (currentWave > 0)
        {
            BoxCollider areaCollider = EnemyWaves[currentWave - 1].AreaCollider;
            if (areaCollider != null)
            {
                areaCollider.enabled = true;
                areaCollider.isTrigger = true;
                AreaColliderTrigger act = areaCollider.gameObject.AddComponent<AreaColliderTrigger>();
                act.EnemyWaveSystem = this;
            }
        }

        //将下一个碰撞器设置为相机区域触发器
        if (EnemyWaves[currentWave].AreaCollider != null)
        {
            EnemyWaves[currentWave].AreaCollider.gameObject.SetActive(true);
        }

        CameraFollow cf = GameObject.FindObjectOfType<CameraFollow>();
        if (cf != null)
        {
            cf.CurrentAreaCollider = EnemyWaves[currentWave].AreaCollider;
        }

        //显示UI
        HandPointer hp = GameObject.FindObjectOfType<HandPointer>();
        if (hp != null)
        {
            hp.ActivateHandPointer();
        }
    }

    /// <summary>
    /// 所有的敌人被摧毁后
    /// </summary>
    /// <param name="g"></param>
    void OnUnitDestroy(GameObject g)
    {
        if (EnemyWaves.Length > currentWave)
        {
            EnemyWaves[currentWave].RemoveEnemyFromWave(g);
            if (EnemyWaves[currentWave].WaveComplete())
            {
                currentWave += 1;
                if (!AllWavesCompleted())
                {
                    UpdateAreaColliders();
                }
                else
                {
                    StartCoroutine(LevelComplete());
                }
            }
        }
    }

    /// <summary>
    /// 如果所有波次都完成，则为真
    /// </summary>
    /// <returns></returns>
    private bool AllWavesCompleted()
    {
        int waveCount = EnemyWaves.Length;
        int waveFinished = 0;

        for (int i = 0; i < waveCount; i++)
        {
            if (EnemyWaves[i].WaveComplete())
            {
                waveFinished += 1;
            }
        }

        if (waveCount == waveFinished)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 更新敌人战术
    /// </summary>
    void SetEnemyTactics()
    {
        EnemyManager.SetEnemyTactics();
    }

    /// <summary>
    /// 当前关卡完成
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelComplete()
    {
        //激活慢动作效果
        if (activateSlowMotionOnLastHit)
        {
            CameraSlowMotion csm = Camera.main.GetComponent<CameraSlowMotion>();
            if (csm != null)
            {
                csm.StartSlowMotionDelay(effectDuration);
                yield return new WaitForSeconds(effectDuration);
            }
        }

        //Timeout before continuing
        yield return new WaitForSeconds(1f);

        //TODO:完成关卡后调用UI

        ////Fade to black
        //UIManager UI = GameObject.FindObjectOfType<UIManager>();
        //if (UI != null)
        //{
        //    UI.UI_fader.Fade(UIFader.FADE.FadeOut, 2f, 0);
        //    yield return new WaitForSeconds(2f);
        //}

        ////Disable players
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //foreach (GameObject p in players)
        //{
        //    Destroy(p);
        //}

        ////Go to next level or show GAMEOVER screen
        //if (loadNewLevel)
        //{
        //    if (levelName != "")
        //        SceneManager.LoadScene(levelName);

        //}
        //else
        //{

        //    //Show game over screen
        //    if (UI != null)
        //    {
        //        UI.DisableAllScreens();
        //        UI.ShowMenu("LevelComplete");
        //    }
        //}
    }
}
