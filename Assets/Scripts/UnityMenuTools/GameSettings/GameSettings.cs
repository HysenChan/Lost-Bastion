﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : ScriptableObject
{
    [Header("Application Settings")]
    public float timeScale = 1f;
    public int framerate = 60;
    public bool showFPSCounter = false;

    [Header("Audio Settings")]
    public float MusicVolume = 0.7f;
    public float SFXVolume = 0.9f;
}
