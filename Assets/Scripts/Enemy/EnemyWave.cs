using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWave
{
    public string WaveName = "Wave";
    public BoxCollider AreaCollider; //防止玩家离开某个区域的对撞机
    public List<GameObject> EnemyList = new List<GameObject>();

    public bool WaveComplete()
    {
        return EnemyList.Count == 0;
    }

    public void RemoveEnemyFromWave(GameObject gameObject)
    {
        if (EnemyList.Contains(gameObject))
        {
            EnemyList.Remove(gameObject);
        }
    }
}
