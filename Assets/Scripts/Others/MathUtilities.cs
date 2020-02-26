using UnityEngine;

public static class MathUtilities {

    //曲线计算以减轻影响
    public static float Sinerp(float start, float end, float value)	{
		return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
	}

    //曲线计算
    public static float Coserp(float start, float end, float value)	{
		return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
	}

    //开始+结束时的缓和曲线计算
    public static float CoSinLerp(float start, float end, float value) {
        return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
    }
}
