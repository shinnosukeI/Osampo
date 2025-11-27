using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class StageHeartRateManager : BaseHeartRateManager
{
    [Header("System")]
    // dataRecorderはBaseHeartRateManagerで定義済み
    [SerializeField] private GameManager gameManager;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform heartIcon; // 拍動させるハートの画像
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float decaySpeed = 5.0f;

    // 内部変数
    // currentBPMはBaseHeartRateManagerで定義済み
    private bool isLogging = false;
    private List<int> currentStageBpmList = new List<int>(); // メモリ保存用リスト
    
    // アニメーション用
    private float beatTimer = 0f;
    private float currentPulse = 0f;
    private float displayBpm = 60f;

    protected override void Start()
    {
        base.Start(); // dataRecorderの取得など

        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();

        // ログ開始
        StartLogging();
    }

    protected override void Update()
    {
        base.Update();
        // ハートのアニメーション処理
        UpdateHeartAnimation();
    }

    void OnDestroy()
    {
        // シーン移動時などに確実にログを閉じる
        StopLogging();
    }

    // OnIntEventはBaseHeartRateManagerで定義済み（BPM更新）

    // -------- ログ機能 --------

    private void StartLogging()
    {
        if (isLogging) return;
        isLogging = true;
        currentStageBpmList.Clear(); // リスト初期化

        Debug.Log("ステージ心拍ログ記録開始");
        
        // シーン名に応じてファイル名を切り替える
        string sceneName = SceneManager.GetActiveScene().name;
        string logFileName = "99_test_stage_unknown_log"; // デフォルト

        if (sceneName == "99_BPMTestScene1")
        {
            logFileName = "99_test_stage1_bpm_log";
        }
        else if (sceneName == "99_BPMTestScene2")
        {
            logFileName = "99_test_stage2_bpm_log";
        }
        else
        {
            // その他のシーンで使われた場合、シーン名を含めるなどしても良い
            logFileName = $"99_test_{sceneName}_bpm_log";
        }

        dataRecorder.OpenLogFile(logFileName);
        
        // 1秒ごとに記録するコルーチン開始
        StartCoroutine(LoggingCoroutine());
    }

    private void StopLogging()
    {
        if (!isLogging) return; // 既に停止済みなら何もしない

        isLogging = false;
        dataRecorder.CloseLogFiles();
        Debug.Log("ステージ心拍ログ記録終了");

        // ▼▼▼ GameManagerにリストを保存 ▼▼▼
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "99_BPMTestScene1")
        {
            GameManager.SavedStage1BPMList = new List<int>(currentStageBpmList);
            Debug.Log($"Stage1 BPM List Saved: Count={currentStageBpmList.Count}");
        }
        else if (sceneName == "99_BPMTestScene2")
        {
            GameManager.SavedStage2BPMList = new List<int>(currentStageBpmList);
            Debug.Log($"Stage2 BPM List Saved: Count={currentStageBpmList.Count}");
        }
    }

    private IEnumerator LoggingCoroutine()
    {
        while (isLogging)
        {
            // 現在のBPMを記録
            dataRecorder.RecordHeartRate(currentBPM);
            
            // リストにも追加 (ResultScene用)
            if (currentBPM > 0)
            {
                currentStageBpmList.Add(currentBPM);
            }
            
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
        
        // RestScene2へ遷移 (シーン名で分岐しても良いが、現状はGameManagerに任せる)
        // ※ 99_BPMTestScene2の場合は ResultScene に行く必要があるかもしれないが
        //    ユーザー要望では「ResultSceneはStage2の次に遷移」とあるので、
        //    BPMTestScene2のNextボタンがResultScene行きになるようにGameManager側かここで制御が必要。
        
        if (gameManager != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "99_BPMTestScene2")
            {
                gameManager.LoadResultScene();
            }
            else
            {
                gameManager.LoadRestScene2();
            }
        }
    }
}