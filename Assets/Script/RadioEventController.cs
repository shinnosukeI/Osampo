using UnityEngine;
using System;  
using System.Collections;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class RadioEventController : MonoBehaviour
{
    public event Action OnRadioPlaybackStarted;

    [Header("オーディオクリップ")]
    [SerializeField] private AudioClip radioStoryClip;
    [SerializeField] private AudioClip noiseLoopClip;

    [Header("再生設定")]
    [SerializeField] private bool playNoiseOnStart = false;

    [Header("音量バランス")]
    [Range(0f, 1f)] [SerializeField] private float talkVolume = 1.0f;
    [Range(0f, 1f)] [SerializeField] private float noiseVolume = 0.3f;

    [Header("字幕設定")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [TextArea(2, 5)]
    [SerializeField] private string[] subtitleContent;

    
    private AudioSource talkSource;
    private AudioSource noiseSource;
    private Coroutine talkCoroutine;

    void Awake()
    {
        talkSource = GetComponent<AudioSource>();
        noiseSource = gameObject.AddComponent<AudioSource>();

        noiseSource.spatialBlend = talkSource.spatialBlend;
        noiseSource.minDistance = talkSource.minDistance;
        noiseSource.maxDistance = talkSource.maxDistance;
        noiseSource.rolloffMode = talkSource.rolloffMode;

        talkSource.playOnAwake = false;
        noiseSource.playOnAwake = false;
    }

    void Start()
    {
        if (playNoiseOnStart) PlayNoiseLoop();

        // ★ 起動時に参照チェック（ここで問題が出てると一発で分かる）
        if (subtitleText == null)
            Debug.LogError("[RadioEvent] subtitleText が未設定です（InspectorでTextMeshProUGUIを入れて）");
        else
            Debug.Log($"[RadioEvent] subtitleText OK: {subtitleText.name} / activeInHierarchy={subtitleText.gameObject.activeInHierarchy}");

        if (subtitleContent == null || subtitleContent.Length == 0)
            Debug.LogWarning("[RadioEvent] subtitleContent が空です（字幕が1行もありません）");
    }

    public void PlayNoiseLoop()
    {
        if (noiseSource.isPlaying) return;
        if (noiseLoopClip == null) return;

        noiseSource.clip = noiseLoopClip;
        noiseSource.loop = true;
        noiseSource.volume = noiseVolume;
        noiseSource.Play();
    }

    public void PlayRadioSequence()
    {
        PlayNoiseLoop();
        
        OnRadioPlaybackStarted?.Invoke(); 

        // ★ 二重再生防止（連打でコルーチンが重なると字幕が消える/競合する）
        if (talkCoroutine != null)
        {
            StopCoroutine(talkCoroutine);
            talkCoroutine = null;
        }

        // ★ 強制テスト表示（親が非表示だと activeInHierarchy が false のまま）
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(true);
            subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 1f);
            subtitleText.text = (subtitleContent != null && subtitleContent.Length > 0) ? subtitleContent[0] : "（字幕データが空）";

            Debug.Log($"[RadioEvent] 字幕TEST表示: activeInHierarchy={subtitleText.gameObject.activeInHierarchy} text='{subtitleText.text}'");
        }

        talkCoroutine = StartCoroutine(TalkSequenceCoroutine());
    }

    private IEnumerator TalkSequenceCoroutine()
    {
        if (radioStoryClip == null)
        {
            Debug.LogWarning("[RadioEvent] radioStoryClip が未設定です");
            yield break;
        }

        talkSource.clip = radioStoryClip;
        talkSource.loop = false;
        talkSource.volume = talkVolume;
        talkSource.Play();

        // 字幕あり
        if (subtitleText != null && subtitleContent != null && subtitleContent.Length > 0)
        {
            float totalDuration = radioStoryClip.length;
            float perLineDuration = totalDuration / subtitleContent.Length;

            subtitleText.gameObject.SetActive(true);

            for (int i = 0; i < subtitleContent.Length; i++)
            {
                subtitleText.text = subtitleContent[i];
                yield return new WaitForSeconds(perLineDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(radioStoryClip.length);
        }

        // 終了後
        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }

        talkCoroutine = null;
    }
}