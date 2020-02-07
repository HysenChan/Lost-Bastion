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

    
}
