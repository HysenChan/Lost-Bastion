using UnityEngine;
using UnityEngine.UI;

public class HUDPlayerPortrait : MonoBehaviour
{

    public Image playerPortrait;

    void Start()
    {
        //加载在角色选择屏幕中，已选择的角色图标
        if (playerPortrait && GlobalPlayerData.PlayerHUDProtrait)
        {
            playerPortrait.overrideSprite = GlobalPlayerData.PlayerHUDProtrait;
        }
    }
}
