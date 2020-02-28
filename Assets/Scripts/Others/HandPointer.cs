using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HandPointer : MonoBehaviour
{
    public Image sprite;
    public float speed = 0.8f;
    public float delayBeforeStart = 1;
    public string sfx = "";
    public AnimationCurve alphaCurve;
    public bool HandActive;
    float t = 0;
    float startTime;

    private void Awake()
    {
        sprite.enabled = false;
    }

    private void Update()
    {
        if (t > 0 && Time.time > startTime)
        {
            sprite.enabled = true;
            t -= Time.deltaTime * speed;
            sprite.color = new Color(1, 1, 1, alphaCurve.Evaluate(1 - t));
        }
        else
        {
            sprite.enabled = false;
        }

        if (HandActive && t <= 0 && Time.time > startTime)
        {
            t = 1;
            if (sfx != "") GlobalAudioPlayer.PlaySFX(sfx);
        }
    }

    /// <summary>
    /// 激活
    /// </summary>
    public void ActivateHandPointer()
    {
        startTime = Time.time + delayBeforeStart;
        HandActive = true;
    }

    /// <summary>
    /// 取消激活
    /// </summary>
    public void DeActivateHandPointer()
    {
        HandActive = false;
    }
}
