using UnityEngine;
using System.Collections;
using TMPro;
using System; // Added for Action

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
    [TextArea(2, 5)] 
    [SerializeField] private string[] subtitleContent;

    // ★ 追加: 再生開始イベント
    public event Action OnRadioPlaybackStarted;

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

        // ★ 強制テスト：配列の先頭を一回だけ表示してみる
        if (subtitleText != null && subtitleContent != null && subtitleContent.Length > 0)
        {
            subtitleText.gameObject.SetActive(true);
            subtitleText.text = subtitleContent[0];
            Debug.Log("🎬 テスト表示: " + subtitleContent[0]);
        }
        else
        {
            Debug.Log("⚠ 字幕テスト失敗: subtitleText か subtitleContent が設定されていない");
        }

        // 会話のコルーチンを開始
        StartCoroutine(TalkSequenceCoroutine());
    }

    private IEnumerator TalkSequenceCoroutine()
    {
        Debug.Log("📻 会話イベント開始");

        if (radioStoryClip != null)
        {
            // 会話再生開始
            talkSource.clip = radioStoryClip;
            talkSource.loop = false;
            talkSource.volume = talkVolume; 
            talkSource.Play();
            OnRadioPlaybackStarted?.Invoke(); // ★ 追加: イベント発火
            Debug.Log("▶ ラジオ音声再生開始。長さ: " + radioStoryClip.length);

            // ★ 字幕が設定されている場合は、クリップの長さを行数で割って表示時間を決める
            if (subtitleText != null && subtitleContent != null && subtitleContent.Length > 0)
            {
                Debug.Log("📝 字幕行数: " + subtitleContent.Length);

                float totalDuration = radioStoryClip.length;
                float perLineDuration = totalDuration / subtitleContent.Length;

                subtitleText.gameObject.SetActive(true);

                for (int i = 0; i < subtitleContent.Length; i++)
                {
                    Debug.Log($"➡ {i} 行目表示: {subtitleContent[i]} / 表示時間: {perLineDuration}");
                    subtitleText.text = subtitleContent[i]; // この行の字幕を表示
                    yield return new WaitForSeconds(perLineDuration);
                }
            }
            else
            {
                Debug.Log("⚠ 字幕なし分岐に入りました");
                // 字幕がない場合は、音声の長さだけ待機
                yield return new WaitForSeconds(radioStoryClip.length);
            }
        }

        // --- 会話終了後の処理 ---
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
            subtitleText.text = ""; // 念のため消しておく
        }

        Debug.Log("📻 会話終了（ノイズはそのまま継続）");
    }
}