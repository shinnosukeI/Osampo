using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // シングルトン設定（どこからでも呼べるようにする）
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource seAudioSource;  // SE用スピーカー
    [SerializeField] private AudioSource bgmAudioSource; // BGM用スピーカー

    [Header("SE Clips")]
    [SerializeField] private AudioClip titleStartSE;    // タイトル画面のStartボタン専用
    [SerializeField] private AudioClip commonButtonSE;  // その他のボタン共通

    // Volume Properties
    private float bgmVolume = 1.0f;
    public float BGMVolume
    {
        get { return bgmVolume; }
        set
        {
            bgmVolume = Mathf.Clamp01(value);
            if (bgmAudioSource != null) bgmAudioSource.volume = bgmVolume;
        }
    }

    private float seVolume = 1.0f;
    public float SEVolume
    {
        get { return seVolume; }
        set
        {
            seVolume = Mathf.Clamp01(value);
            if (seAudioSource != null) seAudioSource.volume = seVolume;
        }
    }

    private const string BGM_VOLUME_KEY = "BGM_Volume";
    private const string SE_VOLUME_KEY = "SE_Volume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); // 重複して存在しないようにする
            return;
        }

        if (seAudioSource == null) seAudioSource = gameObject.AddComponent<AudioSource>();
        if (bgmAudioSource == null) bgmAudioSource = gameObject.AddComponent<AudioSource>();

        bgmAudioSource.loop = true;

        // Load saved volumes
        LoadVolume();
    }

    private void LoadVolume()
    {
        BGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        SEVolume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1.0f);
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, BGMVolume);
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, SEVolume);
        PlayerPrefs.Save();
    }

    // ========================================================================
    // ▼▼▼ SE再生機能 (ボタンから呼び出す用) ▼▼▼
    // ========================================================================

    /// <summary>
    /// 汎用的なSE再生メソッド
    /// </summary>
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            seAudioSource.PlayOneShot(clip, SEVolume);
        }
    }

    /// <summary>
    /// タイトル画面のStartボタン専用SEを再生
    /// </summary>
    public void PlayTitleStartSE()
    {
        PlaySE(titleStartSE);
    }

    /// <summary>
    /// その他の共通ボタンSEを再生（Next, Exitなど）
    /// </summary>
    public void PlayCommonButtonSE()
    {
        PlaySE(commonButtonSE);
    }

    // ========================================================================
    // ▼▼▼ BGM再生機能 (各Managerスクリプトから呼び出す用) ▼▼▼
    // ========================================================================

    /// <summary>
    /// 指定したBGMを再生する
    /// </summary>
    /// <param name="clip">再生したいBGMのクリップ</param>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // すでに同じ曲が流れている場合は何もしない（再ロード時の途切れ防止）
        if (bgmAudioSource.clip == clip && bgmAudioSource.isPlaying)
        {
            return;
        }

        bgmAudioSource.clip = clip;
        bgmAudioSource.volume = BGMVolume;
        bgmAudioSource.Play();
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        bgmAudioSource.Stop();
        bgmAudioSource.clip = null;
    }
}