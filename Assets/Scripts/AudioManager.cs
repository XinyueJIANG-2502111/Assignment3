using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("专门用来播放背景音乐的喇叭 / Dedicated for BGM")]
    private AudioSource bgmSource;
    [Tooltip("专门用来并发播放音效的喇叭 / Dedicated for SFX loops/shots")]
    private AudioSource sfxSource;

    //[Header("Audio Clips Database")]
    // 用字典或者直观的列表在面板里注册音效
    // Audio library struct to link a string Key to an AudioClip file
    [System.Serializable]
    public struct SoundEffect
    {
        public string soundName; // 音效的名字，比如 "Click", "Explode", "BGM_Title"
        public AudioClip clip;   // 对应的音频资源文件
    }

    public List<SoundEffect> soundLibrary = new List<SoundEffect>();
    private Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();

    void Awake()
    {
        // 单例不灭守卫 / Singleton lifecycle preservation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. 运行时动态动态给自己挂载两个喇叭，彻底免去手动摆放的麻烦
        // Auto-initialize separate hardware channels for background music and effects
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // 配置背景音乐喇叭：必须默认循环，且不随 3D 距离衰减（2D 纯平播放）
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // 0 代表绝对的 2D 纯平音效

        // 配置音效喇叭
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        // 2. 将面板里的列表转化为快速查表的字典
        // Bake the list data into a high-performance hash dictionary
        foreach (var sound in soundLibrary)
        {
            if (!string.IsNullOrEmpty(sound.soundName) && sound.clip != null)
            {
                audioDict[sound.soundName] = sound.clip;
            }
        }
    }

    /// <summary>
    /// 【全局接口 1】播放背景音乐（会自动平滑切歌）
    /// Global API to play background music on a permanent loop
    /// </summary>
    public void PlayBGM(string bgmName, float volume = 0.5f)
    {
        if (audioDict.TryGetValue(bgmName, out AudioClip clip))
        {
            // 如果已经在放这首歌了，就不要打断重播
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"【AudioManager】未找到指定的背景音乐: {bgmName}");
        }
    }

    /// <summary>
    /// 【全局接口 2】停止播放背景音乐
    /// Stop background music instantly
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 【全局接口 3】播放突发重叠音效（比如一万个方块同时连环殉爆）
    /// Global API to fire rapid, overlapping sound effects via PlayOneShot
    /// </summary>
    public void PlaySFX(string sfxName, float volume = 0.8f)
    {
        if (audioDict.TryGetValue(sfxName, out AudioClip clip))
        {
            // PlayOneShot 是独立游戏音频的灵魂！
            // 它允许同一个喇叭在同一帧重叠播放几十个声音，而不会互相掐断掐死，非常适合殉爆效果！
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"【AudioManager】未找到指定的音效: {sfxName}");
        }
    }
}