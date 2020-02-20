using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSlowMotion : MonoBehaviour
{
    public float slowMotionTimeScale = 0.2f;

    public void StartSlowMotionDelay(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowMotionRoutine(duration));
    }

    //慢动作
    IEnumerator SlowMotionRoutine(float duration)
    {
        //设置缩放时间
        Time.timeScale = slowMotionTimeScale;

        //等待
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup<(startTime+duration))
        {
            yield return null;
        }

        //重设缩放时间
        GameSettings settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (settings!=null)
        {
            Time.timeScale = settings.timeScale;
        }
        else
        {
            Time.timeScale = 1;
        }
    }
}
