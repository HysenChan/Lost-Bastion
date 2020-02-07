#if UNITY_EDITOR 
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class GameSettingsEditor : EditorWindow
{
    //拿到GameSettings的脚本
    private GameSettings settings;
    //设置保存的位置
    private string databasePath = "Assets/Scripts/UnityMenuTools/GameSettings.asset";
    [MenuItem("Tools/Game Settings")]

    //创建窗口
    public static void Init()
    {
        GameSettingsEditor window = EditorWindow.GetWindow<GameSettingsEditor>();
        window.minSize = new Vector2(350, 200);//设置界面最小值,防止缩放超过最小值后看不到内容
        window.Show();
    }

    private void OnEnable()
    {
        if (settings==null)
        {
            LoadSettings();
        }
    }

    private void OnDisable()
    {
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DisplayMainArea();//显示内容
        EditorGUILayout.EndVertical();
    }

    //加载或创建脚本
    void LoadSettings()
    {
        settings = (GameSettings)AssetDatabase.LoadAssetAtPath(databasePath, typeof(GameSettings));
        if (settings==null)
        {
            CreateDatabase();
        }
    }

    //创建脚本
    void CreateDatabase()
    {
        settings = ScriptableObject.CreateInstance<GameSettings>();
        AssetDatabase.CreateAsset(settings, databasePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //保存脚本
    void SaveSettings()
    {
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(settings);
    }

    //标签头部
    private void BeginHeader(string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);//粗体
    }

    ////提醒文本
    //private void DisplayWarningText(string label)
    //{
    //    GUIStyle style2 = new GUIStyle(EditorStyles.wordWrappedLabel);//换行
    //    style2.wordWrap = true;
    //    style2.stretchHeight = true;
    //    style2.normal.textColor = Color.red;
    //    EditorGUILayout.LabelField(label, style2);
    //}

    ////组标签
    //GUIContent label(string label)
    //{
    //    return new GUIContent(label);
    //}

    //编辑现有项目的窗口
    void DisplayMainArea()
    {
        EditorGUIUtility.labelWidth = 300;

        EditorGUILayout.Space();
        BeginHeader("Global Game Settings");
        settings.timeScale = EditorGUILayout.FloatField(new GUIContent("TimeScale:"), settings.timeScale);
        settings.framerate = EditorGUILayout.IntField(new GUIContent("Framerate: "), settings.framerate);
        settings.showFPSCounter = EditorGUILayout.Toggle(new GUIContent("Show FPS counter: "), settings.showFPSCounter);
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        BeginHeader("Global Audio Settings");
        settings.SFXVolume = EditorGUILayout.FloatField(new GUIContent("SFX Volume: "), Mathf.Clamp(settings.SFXVolume, 0f, 1f));
        settings.MusicVolume = EditorGUILayout.FloatField(new GUIContent("Music Volume: "), Mathf.Clamp(settings.MusicVolume, 0f, 1f));
        EditorGUILayout.Space();
    }
}
