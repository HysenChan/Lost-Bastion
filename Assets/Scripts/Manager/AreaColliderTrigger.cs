using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaColliderTrigger : MonoBehaviour
{
    public EnemyWaveSystem EnemyWaveSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (EnemyWaveSystem!=null)
            {
                EnemyWaveSystem.StartNewWave();
                Destroy(gameObject);
            }
        }
    }
}
