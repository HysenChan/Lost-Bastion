using UnityEngine;

public static class GlobalAudioPlayer
{
    //音效
    public static AudioPlayer audioPlayer;

    /// <summary>
    /// 播放指定音乐文件
    /// </summary>
    /// <param name="sfxName">音乐名称</param>
    public static void PlaySFX(string sfxName)
    {
        if (audioPlayer != null && sfxName != "")
        {
            audioPlayer.playSFX(sfxName);
        }
    }

    /// <summary>
    /// 在指定位置播放音乐
    /// </summary>
    /// <param name="sfxName">音乐名称</param>
    /// <param name="position">播放的位置</param>
    public static void PlaySFXAtPosition(string sfxName, Vector3 position)
    {
        if (audioPlayer != null && sfxName != "")
        {
            audioPlayer.playSFXAtPosition(sfxName, position);
        }
    }

    /// <summary>
    /// 在指定位置播放音乐
    /// </summary>
    /// <param name="sfxName">音乐名称</param>
    /// <param name="position">播放的位置</param>
    /// <param name="parent"></param>
    public static void PlaySFXAtPosition(string sfxName, Vector3 position, Transform parent)
    {
        if (audioPlayer != null && sfxName != "")
        {
            audioPlayer.playSFXAtPosition(sfxName, position, parent);
        }
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="musicName"></param>
    public static void PlayMusic(string musicName)
    {
        {
            if (audioPlayer != null) audioPlayer.playMusic(musicName);
        }
    }
}
