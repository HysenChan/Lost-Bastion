using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInit : MonoBehaviour
{
    [Space(5)]
    [Header("Settings")]
    public string LevelMusic = "Music";
    public string showMenuAtStart = "";
    public bool playMusic = true;
    public bool createUI;
    public bool createInputManager;
    public bool createAudioPlayer;
    public bool createGameCamera;
    private GameObject audioPlayer;
    private GameSettings settings;

    private void Awake()
    {
        //游戏设置
        settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (settings!=null)
        {
            Time.timeScale = settings.timeScale;
            Application.targetFrameRate = settings.framerate;
        }

        //创建输入管理器
        if (!GameObject.FindObjectOfType<InputManager>()&&createInputManager)
        {
            GameObject.Instantiate(Resources.Load("InputManager"),Vector3.zero,Quaternion.identity);
        }

        //创建游戏摄像机
        if (!GameObject.FindObjectOfType<CameraFollow>()&&createGameCamera)
        {
            GameObject.Instantiate(Resources.Load("GameCamera"), Vector3.zero, Quaternion.identity);
        }

        //创建游戏音效
        if (!GameObject.FindObjectOfType<AudioPlayer>() && createAudioPlayer)
        {
            audioPlayer = GameObject.Instantiate(Resources.Load("AudioPlayer"), Vector3.zero, Quaternion.identity) as GameObject;
        }

        //开始游戏背景音乐
        if (playMusic&&createAudioPlayer)
        {
            Invoke("PlayMusic", 1f);
        }

        //TODO:开始游戏打开菜单
        //if (!string.IsNullOrEmpty(showMenuAtStart))
        //{
        //    ShowStartMenu();
        //}
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    private void PlayMusic()
    {
        if (audioPlayer!=null)
        {
            audioPlayer.GetComponent<AudioPlayer>().playMusic(LevelMusic);
        }
    }

    //开始游戏菜单
    private void ShowStartMenu()
    {
        
    }
}
