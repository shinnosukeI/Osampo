using UnityEngine;
using UnityEngine.Video; // VideoPlayer を使うために必要

public class VideoEndHandler : MonoBehaviour
{
    // インスペクタから設定する
    [SerializeField]
    private GameObject imageToShow; // 動画終了後に表示する画像

    // ▼▼▼【追加】▼▼▼
    [SerializeField]
    private GameObject buttonToShow; // 動画終了後に表示するボタン
    // ▲▲▲▲▲▲▲▲▲▲

    private VideoPlayer videoPlayer;

    void Awake()
    {
        // このスクリプトがアタッチされているのと同じGameObjectからVideoPlayerを取得
        videoPlayer = GetComponent<VideoPlayer>();

        // 再生終了イベント(loopPointReached)に関数を登録
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    // ▼ 動画の再生が終了したときに呼び出される関数 ▼
    void OnVideoFinished(VideoPlayer vp)
    {
        // 画像を表示
        if (imageToShow != null)
        {
            imageToShow.SetActive(true);
        }

        // ▼▼▼【追加】▼▼▼
        // ボタンを表示
        if (buttonToShow != null)
        {
            buttonToShow.SetActive(true);
        }
        // ▲▲▲▲▲▲▲▲▲▲
        
        // オプション：動画プレイヤーのオブジェクト自体を非表示にする
        // vp.gameObject.SetActive(false);
    }
}