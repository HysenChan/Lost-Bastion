using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeToLive : MonoBehaviour
{
    public float LifeTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyGO", LifeTime);
    }

    void DestroyGO()
    {
        Destroy(gameObject);
    }
}
