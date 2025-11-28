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

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName = "Stage1"; // 次に遷移するシーン名
    [SerializeField] private bool doMeasurement = true;       // 平均心拍計測を行うか？
    [SerializeField] private FadeType transitionFadeType = FadeType.Noise; // 次へのフェード種類

    [Header("UI Objects")]
    [SerializeField] private GameObject nextButtonObj; // NextボタンのGameObject
    [SerializeField] private GameObject testButtonObj; // テスト用、Stage1影武者シーンへ遷移するボタン
    [SerializeField] private GameObject reloadButtonObj;
    [SerializeField] private GameObject exitButtonObj; // ExitボタンのGameObject
    [SerializeField] private TextMeshProUGUI statusText; // メッセージ表示用

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
        reloadButtonObj.SetActive(false);
        testButtonObj.SetActive(false);

        statusText.text = "Loading...";

        // HeartRateManagerのイベント監視を開始
        if (heartRateManager != null)
        {
            heartRateManager.OnConnectionCheckFinished += OnConnectionResult;
            heartRateManager.OnMeasurementFinished += OnMeasurementResult;

            // ▼▼▼ 修正: 設定に基づいて計測するかどうかを伝える ▼▼▼
            heartRateManager.StartRestSceneSequence(doMeasurement);
        }
        if (SoundManager.Instance != null) SoundManager.Instance.StopBGM();
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

        // 計測モードの場合、計測完了(OnMeasurementFinished)を待つ必要があるが、
        // 今回のHeartRateManagerのロジックでは「接続成功通知」のあとに「計測」が走る。
        // UI表示タイミングを制御するため、
        // 「計測あり」の場合はここではまだNextを出さない、という制御も可能だが、
        // HeartRateManagerが接続成功を返した時点で「接続はOK」なので、
        // RestScene2(計測なし)の場合は即座にNextを出す。

        if (statusText != null) statusText.gameObject.SetActive(true);

        if (isConnectionSuccess == true)
        {
            // 成功パターン
            if (doMeasurement)
            {
                // 計測あり(RestScene1): 計測完了コールバックでNextを出す方が自然だが、
                // 既存ロジック(接続OKならNext)を踏襲するなら、ここでテキストだけ変える
                statusText.text = "接続成功"; 
                // ※計測完了イベントでボタンを出すようにしても良い
            }
            else
            {
                // 計測なし(RestScene2): 即座に完了
                statusText.text = "接続成功";
            }

            // ボタン表示
            if (nextButtonObj != null) nextButtonObj.SetActive(true);
            if (testButtonObj != null) testButtonObj.SetActive(true);
        }
        else
        {
            // 失敗パターン (共通)
            statusText.text = "接続確認できませんでした。\n再接続してください。";
            if (exitButtonObj != null) exitButtonObj.SetActive(true);
            if (reloadButtonObj != null) reloadButtonObj.SetActive(true);
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