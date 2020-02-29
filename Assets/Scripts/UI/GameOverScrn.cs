using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverScrn : MonoBehaviour
{
    public Text text;
    public Gradient ColorTransition;
    public float speed = 3.5f;
    public UIFader fader;
    private bool restartInProgress = false;

    private void OnEnable()
    {
        InputManager.onCombatInputEvent += InputEvent;
    }

    private void OnDisable()
    {
        InputManager.onCombatInputEvent -= InputEvent;
    }

    /// <summary>
    /// 输入按钮
    /// </summary>
    /// <param name="action"></param>
    private void InputEvent(INPUTACTION action)
    {
        if (action == INPUTACTION.PUNCH || action == INPUTACTION.KICK)
            RestartLevel();
    }

    void Update()
    {
        if (text != null && text.gameObject.activeSelf)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            text.color = ColorTransition.Evaluate(t);
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))
        {
            RestartLevel();
        }
    }

    /// <summary>
    /// 重打当前关卡
    /// </summary>
    void RestartLevel()
    {
        if (!restartInProgress)
        {
            restartInProgress = true;

            //播放音效
            GlobalAudioPlayer.PlaySFX("ButtonStart");

            ButtonFlicker bf = GetComponentInChildren<ButtonFlicker>();
            if (bf != null) bf.StartButtonFlicker();

            fader.Fade(UIFader.FADE.FadeOut, 0.5f, 0.5f);

            Invoke("RestartScene", 1f);
        }
    }

    void RestartScene()
    {
        restartInProgress = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
