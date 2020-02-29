using UnityEngine;

public class UIManager : MonoBehaviour
{

    public UIFader UI_fader;
    public UI_Screen[] UIMenus;

    void Awake()
    {
        DisableAllScreens();

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 展示菜单UI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="disableAllScreens"></param>
    public void ShowMenu(string name, bool disableAllScreens)
    {
        if (disableAllScreens) DisableAllScreens();
        GameObject UI_Gameobject = null;
        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Name == name)
            {
                UI_Gameobject = UI.UI_Gameobject;
            }
        }

        if (UI_Gameobject != null)
        {
            UI_Gameobject.SetActive(true);
        }
        else
        {
            Debug.Log("找不到该UI名称： " + name);
        }

        if (UI_fader != null)
            UI_fader.gameObject.SetActive(true);
        UI_fader.Fade(UIFader.FADE.FadeIn, 1f, .3f);
    }

    /// <summary>
    /// 展示菜单UI
    /// </summary>
    /// <param name="name"></param>
    public void ShowMenu(string name)
    {
        ShowMenu(name, true);
    }

    /// <summary>
    /// 关闭菜单UI
    /// </summary>
    /// <param name="name"></param>
    public void CloseMenu(string name)
    {
        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Name == name) UI.UI_Gameobject.SetActive(false);
        }
    }

    /// <summary>
    /// 关闭所有菜单UI
    /// </summary>
    public void DisableAllScreens()
    {
        foreach (UI_Screen UI in UIMenus)
        {
            if (UI.UI_Gameobject != null)
                UI.UI_Gameobject.SetActive(false);
            else
                Debug.Log("找不到该UI名称： " + UI.UI_Name);
        }
    }
}

[System.Serializable]
public class UI_Screen
{
    public string UI_Name;
    public GameObject UI_Gameobject;
}
