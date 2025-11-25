using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RestSceneManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private HeartRateManager heartRateManager;
    
    // VideoEndHandlerは既存のものを使用
    [SerializeField] private VideoEndHandler videoEndHandler;

    [Header("UI Objects")]
    [SerializeField] private GameObject nextButtonObj; // NextボタンのGameObject
    [SerializeField] private GameObject exitButtonObj; // ExitボタンのGameObject
    [SerializeField] private TextMeshProUGUI statusText; // メッセージ表示用

    [Header("UI Buttons")]
    [SerializeField] private Button nextButton; // Nextボタンコンポーネント
    [SerializeField] private Button exitButton; // Exitボタンコンポーネント

    // 状態フラグ
    private bool isVideoFinished = false;
    private bool? isConnectionSuccess = null; // null=未定, true=成功, false=失敗

    void Start()
    {

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
        }
        // マネージャーの自動検索
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (heartRateManager == null) heartRateManager = FindFirstObjectByType<HeartRateManager>();
        if (videoEndHandler == null) videoEndHandler = FindFirstObjectByType<VideoEndHandler>();

        // ボタンの初期化（非表示）
        nextButtonObj.SetActive(false);
        exitButtonObj.SetActive(false);
        statusText.text = "Loading...";

        // ボタンクリックイベント登録
        nextButton.onClick.AddListener(() => gameManager.LoadStage1());
        exitButton.onClick.AddListener(() => gameManager.LoadConfinementWalk());

        // HeartRateManagerのイベント監視を開始
        if (heartRateManager != null)
        {
            heartRateManager.OnConnectionCheckFinished += OnConnectionResult;
            heartRateManager.OnMeasurementFinished += OnMeasurementResult;
            
            // ★ シーン開始とともに処理スタート
            heartRateManager.StartRestSceneSequence();
        }
    }

    // VideoEndHandlerから呼び出してもらう関数
    public void OnVideoFinished()
    {
        Debug.Log("動画終了検知");
        isVideoFinished = true;
        CheckConditions();
    }

    // 接続確認の結果が来たら呼ばれる
    private void OnConnectionResult(bool success)
    {
        isConnectionSuccess = success;
        CheckConditions();
    }

    // 計測結果が来たら呼ばれる
    private void OnMeasurementResult(float avgBpm)
    {
        // GameManagerに保存
        if (gameManager != null)
        {
            gameManager.SetBaseHeartRate(avgBpm);
        }
    }

// 「動画終了」かつ「接続判定済み」ならUIを更新
    private void CheckConditions()
    {
        if (!isVideoFinished) return; // 動画が終わってないならまだ
        if (isConnectionSuccess == null) return; // 接続判定が出てないならまだ

        // ▼▼▼ 追加: 判定が出たら、まずはテキストオブジェクト自体を表示状態にする ▼▼▼
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        if (isConnectionSuccess == true)
        {
            // 成功パターン
            statusText.text = ""; // 文字を消す（または "接続成功" と出してもOK）
            nextButtonObj.SetActive(true);
        }
        else
        {
            // 失敗パターン
            statusText.text = "心拍計の接続を確認できませんでした。\n再接続してください。";
            exitButtonObj.SetActive(true);
        }
    }

    void OnDestroy()
    {
        // イベント解除（エラー防止）
        if (heartRateManager != null)
        {
            heartRateManager.OnConnectionCheckFinished -= OnConnectionResult;
            heartRateManager.OnMeasurementFinished -= OnMeasurementResult;
        }
    }
}