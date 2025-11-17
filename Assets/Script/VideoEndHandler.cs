using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events; // ★追加: UnityEventを使うために必要

public class VideoEndHandler : MonoBehaviour
{
    // インスペクタから設定する
    [Header("UI Settings")]
    [SerializeField]
    private GameObject imageToShow; // 動画終了後に表示する画像 (RestSceneでは使わないなら空欄でOK)

    [SerializeField]
    private GameObject buttonToShow; // 動画終了後に表示するボタン (RestSceneでは使わないなら空欄でOK)

    // ▼▼▼【追加】他のスクリプトに動画終了を知らせるイベント ▼▼▼
    [Header("Events")]
    public UnityEvent onVideoEnd; 
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    private VideoPlayer videoPlayer;

    void Awake()
    {
        // このスクリプトがアタッチされているのと同じGameObjectからVideoPlayerを取得
        videoPlayer = GetComponent<VideoPlayer>();

        // 再生終了イベント(loopPointReached)に関数を登録
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    // ▼ 動画の再生が終了したときに呼び出される関数 ▼
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("Video finished.");

        // 1. 既存の機能（画像やボタンを表示）
        if (imageToShow != null)
        {
            imageToShow.SetActive(true);
        }

        if (buttonToShow != null)
        {
            buttonToShow.SetActive(true);
        }

        // ▼▼▼【追加】登録されたイベントを実行（RestSceneManagerへ通知）▼▼▼
        onVideoEnd?.Invoke();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }
}