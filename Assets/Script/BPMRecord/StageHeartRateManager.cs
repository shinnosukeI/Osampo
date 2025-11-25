using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StageHeartRateManager : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private DataRecorder dataRecorder;
    [SerializeField] private GameManager gameManager;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform heartIcon; // 拍動させるハートの画像
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float decaySpeed = 5.0f;

    // 内部変数
    private int currentBPM = 60;
    private bool isLogging = false;
    
    // アニメーション用
    private float beatTimer = 0f;
    private float currentPulse = 0f;
    private float displayBpm = 60f;

    void Start()
    {
        if (dataRecorder == null) dataRecorder = GetComponent<DataRecorder>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();

        // ログ開始
        StartLogging();
    }

    void Update()
    {
        // ハートのアニメーション処理
        UpdateHeartAnimation();
    }

    void OnDestroy()
    {
        // シーン移動時などに確実にログを閉じる
        StopLogging();
    }

    // ▼▼▼ EventReceiverから呼ばれる関数 ▼▼▼
    public void OnIntEvent(int value)
    {
        if (value >= 30)
        {
            currentBPM = value;
        }
    }

    // -------- ログ機能 --------

    private void StartLogging()
    {
        if (isLogging) return;
        isLogging = true;

        Debug.Log("ステージ心拍ログ記録開始");
        // ファイル名: ステージごとの名前にする
        dataRecorder.OpenLogFile("99_test_stage1_bpm_log");
        
        // 1秒ごとに記録するコルーチン開始
        StartCoroutine(LoggingCoroutine());
    }

    private void StopLogging()
    {
        isLogging = false;
        dataRecorder.CloseLogFiles();
        Debug.Log("ステージ心拍ログ記録終了");
    }

    private IEnumerator LoggingCoroutine()
    {
        while (isLogging)
        {
            // 現在のBPMを記録
            dataRecorder.RecordHeartRate(currentBPM);
            
            // 1秒待つ
            yield return new WaitForSeconds(1.0f);
        }
    }

    // -------- アニメーション機能 --------

    private void UpdateHeartAnimation()
    {
        if (heartIcon == null) return;

        // BPMに合わせてスケールを計算
        displayBpm = Mathf.Lerp(displayBpm, (float)currentBPM, Time.deltaTime * 5.0f);
        
        // 拍動間隔 (60秒 / BPM)
        float beatInterval = 60.0f / Mathf.Max(displayBpm, 1.0f);
        
        beatTimer += Time.deltaTime;
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
            currentPulse = 1.0f; // ドクン！
        }

        // 減衰
        currentPulse = Mathf.Lerp(currentPulse, 0f, Time.deltaTime * decaySpeed);
        
        // スケール適用
        float currentScale = Mathf.Lerp(minScale, maxScale, currentPulse);
        heartIcon.localScale = new Vector3(currentScale, currentScale, 1f);
    }

    // -------- 画面遷移ボタンから呼ぶ --------
    public void OnNextButtonClicked()
    {
        StopLogging(); // 明示的に停止
        
        // RestScene2へ遷移
        if (gameManager != null)
        {
            gameManager.LoadRestScene2();
        }
    }
}