using UnityEngine;
using System.Collections;
using TMPro; // TextMeshProを使うために必要

[RequireComponent(typeof(AudioSource))]
public class RadioEventController : MonoBehaviour
{
    [Header("オーディオクリップ")]
    [SerializeField] private AudioClip radioStoryClip; // 最初に流すラジオ音声
    [SerializeField] private AudioClip noiseLoopClip;  // 後に流すノイズ音声

    [Header("字幕設定")]
    [SerializeField] private TextMeshProUGUI subtitleText; // 画面のテキストオブジェクト
    [TextArea(3, 10)] 
    [SerializeField] private string subtitleContent; // 表示する文章

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // 勝手に再生しない
    }

    // EventManagerから呼ばれる関数
    public void PlayRadioSequence()
    {
        StartCoroutine(SequenceCoroutine());
    }

    // 一連の流れを処理するコルーチン
    private IEnumerator SequenceCoroutine()
    {
        Debug.Log("📻 ラジオイベント開始");

        // --- 1. ラジオ音声（会話）の再生 ---
        if (radioStoryClip != null)
        {
            // 字幕を表示
            if (subtitleText != null)
            {
                subtitleText.text = subtitleContent; // 文章をセット
                subtitleText.gameObject.SetActive(true); // 表示ON
            }

            // 音声を再生
            audioSource.clip = radioStoryClip;
            audioSource.loop = false; // 会話はループしない
            audioSource.Play();

            // 音声が終わるまで待機 (秒数待つ)
            yield return new WaitForSeconds(radioStoryClip.length);
        }

        // --- 2. ノイズ音声への切り替え ---
        
        // 字幕を非表示
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false); // 表示OFF
        }

        if (noiseLoopClip != null)
        {
            audioSource.clip = noiseLoopClip;
            audioSource.loop = true; // ノイズはループする
            audioSource.Play();
            Debug.Log("📻 ノイズ再生中...");
        }
    }
}