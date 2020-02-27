using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public GameObject defaultPlayerPrefab;

    private void Awake()
    {
        //角色
        if (GlobalPlayerData.PlayerPrefab)
        {
            LoadPlayer(GlobalPlayerData.PlayerPrefab);
            return;
        }

        if (defaultPlayerPrefab)
        {
            LoadPlayer(defaultPlayerPrefab);
        }
        else
        {
            Debug.Log("找不到默认角色预制体");
        }
    }

    /// <summary>
    /// 加载角色
    /// </summary>
    /// <param name="playerPrefab">角色预制体</param>
    private void LoadPlayer(GameObject playerPrefab)
    {
        GameObject player = GameObject.Instantiate(playerPrefab) as GameObject;
        player.transform.position = transform.position;
    }
}
