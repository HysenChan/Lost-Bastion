using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UICharSelectionPortrait : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    [Header("The Player Character Prefab")]
    public GameObject PlayerPrefab;
    [Space(15)]

    public Image Border;
    public Color BorderColorDefault;
    public Color BorderColorOver;
    public Color BorderColorHighlight;
    public string PlaySFXOnClick = "";
    public bool Selected;

    [Header("HUD Portrait")]
    public Sprite HUDPortrait;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Select();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Deselect();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    public void Select()
    {
        if (Border && !Selected) Border.color = BorderColorOver;
    }

    public void Deselect()
    {
        if (Border && !Selected) Border.color = BorderColorDefault;
    }

    public void OnClick()
    {
        ResetAllButtons();
        Selected = true;
        if (Border) Border.color = BorderColorHighlight;

        //播放点击音效
        GlobalAudioPlayer.PlaySFX(PlaySFXOnClick);

        //设置两个角色的预制体
        CharSelection Cs = GameObject.FindObjectOfType<CharSelection>();
        if (Cs)
            Cs.SelectPlayer(PlayerPrefab);

        //为此玩家设置平视显示器图标
        GlobalPlayerData.PlayerHUDProtrait = HUDPortrait;
    }

    /// <summary>
    /// 重置所有按钮的状态
    /// </summary>
    public void ResetAllButtons()
    {
        UICharSelectionPortrait[] allButtons = GameObject.FindObjectsOfType<UICharSelectionPortrait>();
        foreach (UICharSelectionPortrait button in allButtons)
        {
            button.Border.color = button.BorderColorDefault;
            button.Selected = false;
        }
    }
}