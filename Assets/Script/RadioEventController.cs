using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class RadioEventController : MonoBehaviour
{
    [Header("オーディオクリップ")]
    [SerializeField] private AudioClip radioStoryClip; // 会話（英語）
    [SerializeField] private AudioClip noiseLoopClip;  // ノイズ（ループ）

    [Header("再生設定")]
    [SerializeField] private bool playNoiseOnStart = false; // チェックを入れると、ゲーム開始直後からノイズが流れます

    [Header("音量バランス")]
    [Range(0f, 1f)] [SerializeField] private float talkVolume = 1.0f; 
    [Range(0f, 1f)] [SerializeField] private float noiseVolume = 0.3f; 

    [Header("字幕設定")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [TextArea(3, 10)] 
    [SerializeField] private string subtitleContent;

    private AudioSource talkSource;  // 会話用
    private AudioSource noiseSource; // ノイズ用

    void Awake()
    {
        // スピーカーの準備
        talkSource = GetComponent<AudioSource>();
        noiseSource = gameObject.AddComponent<AudioSource>();

        // 設定コピー
        noiseSource.spatialBlend = talkSource.spatialBlend;
        noiseSource.minDistance = talkSource.minDistance;
        noiseSource.maxDistance = talkSource.maxDistance;
        noiseSource.rolloffMode = talkSource.rolloffMode;

        // 自動再生はオフ（スクリプトで制御するため）
        talkSource.playOnAwake = false;
        noiseSource.playOnAwake = false;
    }

    void Start()
    {
        // ★「最初からノイズを流す」設定の場合、ここで再生開始
        if (playNoiseOnStart)
        {
            PlayNoiseLoop();
        }
    }

    // ノイズ再生専用の関数（ずっとループ再生）
    public void PlayNoiseLoop()
    {
        if (noiseSource.isPlaying) return; // 既に鳴っていたら何もしない

        if (noiseLoopClip != null)
        {
            noiseSource.clip = noiseLoopClip;
            noiseSource.loop = true;          // ★重要：ループON
            noiseSource.volume = noiseVolume; 
            noiseSource.Play();
            Debug.Log("📻 ノイズ再生開始（ループ）");
        }
    }

    // イベント：会話を再生（ノイズがまだなら、ついでにノイズも開始）
    public void PlayRadioSequence()
    {
        // もしノイズがまだ鳴っていなければ、ここで開始（以降ずっと鳴りっぱなし）
        PlayNoiseLoop();

        // 会話のコルーチンを開始
        StartCoroutine(TalkSequenceCoroutine());
    }

    private IEnumerator TalkSequenceCoroutine()
    {
        Debug.Log("📻 会話イベント開始");

        if (radioStoryClip != null)
        {
            // 字幕表示
            if (subtitleText != null)
            {
                subtitleText.text = subtitleContent;
                subtitleText.gameObject.SetActive(true);
            }

            // 会話再生
            talkSource.clip = radioStoryClip;
            talkSource.loop = false;        // 会話は1回だけ
            talkSource.volume = talkVolume; 
            talkSource.Play();

            // 会話が終わるまで待つ
            yield return new WaitForSeconds(radioStoryClip.length);
        }

        // --- 会話終了後の処理 ---
        
        // 字幕だけ消す（ノイズは止めない！）
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
        
        Debug.Log("📻 会話終了（ノイズはそのまま継続）");
    }
}