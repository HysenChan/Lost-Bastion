using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharSelection : MonoBehaviour
{
    public GameObject ContinueButton;
    public string ContinueButtonSFXOnClick = "";
    public string loadLevelOnExit = "GameMenu";
    private bool rightButtonDown;
    private bool leftButtonDown;
    private UICharSelectionPortrait[] portraits;

    void OnEnable()
    {
        InputManager.onMoveInputEvent += InputEvent;
        InputManager.onCombatInputEvent += CombatInputEvent;
        if (ContinueButton)
            ContinueButton.SetActive(false);
    }

    void OnDisable()
    {
        InputManager.onMoveInputEvent -= InputEvent;
        InputManager.onCombatInputEvent -= CombatInputEvent;
    }

    void Start()
    {
        portraits = GetComponentsInChildren<UICharSelectionPortrait>();

        //使用键盘时，默认情况下选择的角色
        InputManager im = GameObject.FindObjectOfType<InputManager>();
        if (im && (im.UseKeyboardInput))
            GetComponentInChildren<UICharSelectionPortrait>().OnClick();
    }

    /// <summary>
    /// 选择角色
    /// </summary>
    /// <param name="playerPrefab"></param>
    public void SelectPlayer(GameObject playerPrefab)
    {
        GlobalPlayerData.PlayerPrefab = playerPrefab;
        SetContinueButtonVisible();
    }

    /// <summary>
    /// 继续开始游戏按钮
    /// </summary>
    public void OnContinueButtonClick()
    {
        GlobalAudioPlayer.PlaySFX(ContinueButtonSFXOnClick);
        UIManager UI = GameObject.FindObjectOfType<UIManager>();
        if (UI)
        {
            UI.UI_fader.Fade(UIFader.FADE.FadeOut, .3f, 0f);
            Invoke("LoadLevel", 0.5f);
        }
    }

    void SetContinueButtonVisible()
    {
        if (ContinueButton)
            ContinueButton.SetActive(true);
    }

    /// <summary>
    /// 加载关卡
    /// </summary>
    void LoadLevel()
    {
        if (!string.IsNullOrEmpty(loadLevelOnExit))
        {
            SceneManager.LoadScene(loadLevelOnExit);
        }
        else
        {
            Debug.Log("找不到该Scene！");
        }
    }

    //键盘输入事件
    void InputEvent(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > 0)
        {
            if (!leftButtonDown && dir.x < 0)
                OnLeftButtonDown();
            if (!rightButtonDown && dir.x > 0)
                OnRightButtonDown();
            return;
        }
        leftButtonDown = rightButtonDown = false;
    }

    /// <summary>
    /// 选择左边角色
    /// </summary>
    void OnLeftButtonDown()
    {
        leftButtonDown = true;

        for (int i = 0; i < portraits.Length; i++)
        {
            if (portraits[i].Selected)
            {
                if (i - 1 >= 0)
                {
                    portraits[i].ResetAllButtons();
                    portraits[i - 1].OnClick();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 选择右边角色
    /// </summary>
    void OnRightButtonDown()
    {
        rightButtonDown = true;

        for (int i = 0; i < portraits.Length; i++)
        {
            if (portraits[i].Selected)
            {
                if (i + 1 < portraits.Length)
                {
                    portraits[i].ResetAllButtons();
                    portraits[i + 1].OnClick();
                    return;
                }
            }
        }
    }

    private void CombatInputEvent(INPUTACTION action)
    {
        OnContinueButtonClick();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.Space))
        {
            OnContinueButtonClick();
        }
    }
}
