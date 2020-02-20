using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public AnimationCurve camShakeX;
    public AnimationCurve camShakeY;
    public AnimationCurve camShakeZ;
    public float multiplier = 1f;
    public float time = 0.5f;
    public bool randomize;//通过将-1或1乘以动画曲线的方向来使其随机化

    public void Shake(float intensity)
    {
        StartCoroutine(DoShake(intensity));
    }

    IEnumerator DoShake(float scale)
    {
        Vector3 rand = new Vector3(GetRandomValue(), GetRandomValue(), GetRandomValue());
        scale *= multiplier;

        float t = 0;
        while (t<time)
        {
            if (randomize)
            {
                transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale * rand.x, camShakeY.Evaluate(t) * scale * rand.y, camShakeZ.Evaluate(t) * scale * rand.z);
            }
            else
            {
                transform.localPosition = new Vector3(camShakeX.Evaluate(t) * scale, camShakeY.Evaluate(t) * scale, camShakeZ.Evaluate(t) * scale);
            }
            t += Time.deltaTime / time;
            yield return null;
        }
        transform.localPosition = Vector3.zero;
    }

    //返回一个随机数是-1或者1
    int GetRandomValue()
    {
        int[] i = { -1, 1 };
        return i[Random.Range(0, 2)];
    }
}
