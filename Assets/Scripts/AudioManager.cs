using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Dedicated for BGM")]
    private AudioSource bgmSource;
    [Tooltip("Dedicated for SFX loops/shots")]
    private AudioSource sfxSource;

    //[Header("Audio Clips Database")]
    // サウンドライブラリ
    // Audio library struct to link a string Key to an AudioClip file
    [System.Serializable]
    public struct SoundEffect
    {
        public string soundName; // 音声の名前
        public AudioClip clip;   // 音声ファイル
    }

    public List<SoundEffect> soundLibrary = new List<SoundEffect>();
    private Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();

    // 音量設定
    [Header("Global Volume Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    void Awake()
    {
        // Singleton lifecycle preservation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初期化
        // Initialize audio sources for BGM and SFX
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // BGM 関連の設定
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;

        // SFX 関連の設定
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;

        // リストデータを高性能ハッシュディクショナリにベイク処理する
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
    /// BGMを再生する（ループ再生）
    /// Global API to play background music on a permanent loop
    /// </summary>
    public void PlayBGM(string bgmName)
    {
        if (audioDict.TryGetValue(bgmName, out AudioClip clip))
        {
            // 再生中かどうかをチェックして、同じ曲なら再生しない
            // Check if the same BGM is already playing, and skip if so
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"【AudioManager】Can't find the specified background music: {bgmName}");
        }
    }

    /// <summary>
    /// 再生停止（BGM）
    /// Stop background music instantly
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// SE再生
    /// Global API to fire rapid, overlapping sound effects via PlayOneShot
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (audioDict.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
        else
        {
            Debug.LogWarning($"【AudioManager】Can't find the specified sound effect: {sfxName}");
        }
    }

    /// <summary>
    /// BGM音量調整
    /// Set BGM volume and update the current playing audio source instantly
    /// </summary>
    public void SetBGMVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume; // リアルタイムで調整
        }
    }

    /// <summary>
    /// SFX音量調整
    /// Set SFX volume scale
    /// </summary>
    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }
}