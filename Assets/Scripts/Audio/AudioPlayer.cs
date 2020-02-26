using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    public AudioItem[] AudioList;
    private AudioSource source;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    void Awake()
    {
        GlobalAudioPlayer.audioPlayer = this;
        source = GetComponent<AudioSource>();

        //设置
        GameSettings settings = Resources.Load("GameSettings", typeof(GameSettings)) as GameSettings;
        if (settings != null)
        {
            musicVolume = settings.MusicVolume;
            sfxVolume = settings.SFXVolume;
        }
    }

    //播放音效
    public void playSFX(string name)
    {
        bool SFXFound = false;
        foreach (AudioItem s in AudioList)
        {
            if (s.name == name)
            {

                //拿到一个随机数
                int rand = Random.Range(0, s.clip.Length);
                source.PlayOneShot(s.clip[rand]);
                source.volume = s.volume * sfxVolume;
                source.loop = s.loop;
                SFXFound = true;
            }
        }
        if (!SFXFound)
        {
            Debug.Log("找不到该文件： " + name);
        }
    }

    /// <summary>
    /// 指定位置播放
    /// </summary>
    /// <param name="name"></param>
    /// <param name="worldPosition"></param>
    /// <param name="parent"></param>
    public void playSFXAtPosition(string name, Vector3 worldPosition, Transform parent)
    {
        bool SFXFound = false;
        foreach (AudioItem s in AudioList)
        {
            if (s.name == name)
            {

                //check the time threshold
                if (Time.time - s.lastTimePlayed < s.MinTimeBetweenCall)
                {
                    return;
                }
                else
                {
                    s.lastTimePlayed = Time.time;
                }

                //pick a random number
                int rand = Random.Range(0, s.clip.Length);

                //create gameobject for the audioSource
                GameObject audioObj = new GameObject();
                audioObj.transform.parent = parent;
                audioObj.name = name;
                audioObj.transform.position = worldPosition;
                AudioSource audiosource = audioObj.AddComponent<AudioSource>();

                //audio source settings
                audiosource.clip = s.clip[rand];
                audiosource.spatialBlend = 1.0f;
                audiosource.minDistance = 4f;
                audiosource.volume = s.volume * sfxVolume;
                audiosource.outputAudioMixerGroup = source.outputAudioMixerGroup;
                audiosource.loop = s.loop;
                audiosource.Play();

                //Destroy on finish
                if (!s.loop && audiosource.clip != null)
                {
                    TimeToLive TTL = audioObj.AddComponent<TimeToLive>();
                    TTL.LifeTime = audiosource.clip.length;
                }
                SFXFound = true;
            }
        }
        if (!SFXFound) Debug.Log("no sfx found with name: " + name);
    }

    public void playSFXAtPosition(string name, Vector3 worldPosition)
    {
        playSFXAtPosition(name, worldPosition, transform.root);
    }

    public void playMusic(string name)
    {
        GameObject music = new GameObject();
        music.name = "Music";
        AudioSource AS = music.AddComponent<AudioSource>();

        foreach (AudioItem s in AudioList)
        {
            if (s.name == name)
            {
                AS.clip = s.clip[0];
                AS.loop = true;
                AS.volume = s.volume * musicVolume;
                AS.Play();
            }
        }
    }
}
