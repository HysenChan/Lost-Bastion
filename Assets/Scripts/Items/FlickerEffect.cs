using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerEffect : MonoBehaviour
{
    public float pauzeBeforeStart = 1.3f;
    public float flickerSpeedStart = 15f;
    public float flickerSpeedEnd = 35f;
    public float Duration = 2f;
    public bool DestroyOnFinish;

    public GameObject[] GFX;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FlickerCoroutine());
    }

    IEnumerator FlickerCoroutine()
    {
        //在开始之前暂停
        yield return new WaitForSeconds(pauzeBeforeStart);

        //闪烁
        float t = 0;
        while (t < 1)
        {
            float speed = Mathf.Lerp(flickerSpeedStart, flickerSpeedEnd, MathUtilities.Coserp(0, 1, t));
            float i = Mathf.Sin(Time.time * speed);
            foreach (GameObject g in GFX) g.SetActive(i > 0);
            t += Time.deltaTime / Duration;
            yield return null;
        }

        //隐藏
        foreach (GameObject g in GFX)
        {
            g.SetActive(false);
        }

        //清除
        if (DestroyOnFinish)
        {
            Destroy(gameObject);
        }
    }
}
