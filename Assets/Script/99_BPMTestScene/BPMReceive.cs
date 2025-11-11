using UnityEngine;
using UnityEngine.UI; // Button
using TMPro;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic; // List
using System.Linq; // Average

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
/// <summary>
/// メインクラス (メリハリのある心拍アニメーション版)
/// </summary>
//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
public sealed class BPMReceive : MonoBehaviour
{
    //================================================================================
    // Fields - Inspector (UI設定)
    //================================================================================

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI Label = default;
    [SerializeField] private Button StartButton = default;
    [SerializeField] private Button StopButton = default;
    [SerializeField] private Button ResetButton = default;
    [SerializeField] private Button AvarageButton = default;
    [SerializeField] private Button NextButton = default;

    [Header("Heart Animation")]
    [SerializeField] private RectTransform heartIcon = default;
    [SerializeField] private float minScale = 1.0f; // 通常時のサイズ
    [SerializeField] private float maxScale = 1.3f; // ドクッといった瞬間のサイズ(少し大きくしました)
    [SerializeField] private float decaySpeed = 10.0f; // ★追加: 縮む速さ（大きいほどキレが良い）

    //================================================================================
    // Fields - Animation
    //================================================================================

    private float beatTimer = 0f;      // ビートのタイミングを計るタイマー
    private float currentPulse = 0f;   // 現在の膨らみ具合 (0.0 ~ 1.0)
    private float targetBpm = 60f;     // 目標のBPM
    private float displayBpm = 60f;    // アニメーション用の滑らかなBPM
    private int lastValidBpm = 60;     // 最後の正常値

    //================================================================================
    // Fields - CSV & Logging
    //================================================================================

    private StreamWriter heartRateLogWriter;
    private string logFilePath;

    private enum AppStatus 
    { 
        Idle, Checking_Start, Logging, Checking_Avarage,
        Measuring_Avarage, Result_Avarage, Failed
    }
    private AppStatus currentStatus = AppStatus.Idle;

    private enum ConnectionCheckStatus { NotChecked, Connected, Failed_NoData, Failed_LowBPM }
    private ConnectionCheckStatus connectionCheckStatus = ConnectionCheckStatus.NotChecked;
    
    private List<int> avarageDataList = new List<int>();

    //================================================================================
    // Unity Lifecycle Methods
    //================================================================================

    void Start()
    {
        PrepareLogFile();

        // ボタンのリスナー設定
        StartButton.onClick.AddListener(OnStartButtonPressed);
        StopButton.onClick.AddListener(OnStopButtonPressed);
        ResetButton.onClick.AddListener(OnResetButtonPressed);
        AvarageButton.onClick.AddListener(OnAvarageButtonPressed);
        NextButton.onClick.AddListener(OnNextButtonPressed);

        // UIの初期状態を設定
        Label.SetText("待機中");
        UpdateButtons(AppStatus.Idle);
    }

    void Update()
    {
        // アプリの状態に関わらず、心臓のアニメーションを常に実行
        AnimateHeart();
    }

    void OnDestroy()
    {
        CloseLogFile();
    }

    //================================================================================
    // Event methods (EventReceiverから呼び出される)
    //================================================================================

    public void OnIntEvent(int value)
    {
        // --- 1. BPMとUI Textの更新 ---
        if (value >= 30)
        {
            targetBpm = (float)value;
            lastValidBpm = value;
            
            if (currentStatus == AppStatus.Idle || currentStatus == AppStatus.Logging)
            {
                Label.SetText($"{value}");
            }
        }
        else
        {
            targetBpm = (float)lastValidBpm; // 異常時は最後の正常値で動かす
            
            if (currentStatus == AppStatus.Idle)
            {
                Label.SetText("心拍が正常に確認できません");
            }
        }

        // --- 2. アプリの状態に応じたロジック ---
        switch (currentStatus)
        {
            case AppStatus.Checking_Start:
            case AppStatus.Checking_Avarage:
                if (value >= 30) { connectionCheckStatus = ConnectionCheckStatus.Connected; }
                else if (connectionCheckStatus != ConnectionCheckStatus.Connected)
                { connectionCheckStatus = ConnectionCheckStatus.Failed_LowBPM; }
                break;
            
            case AppStatus.Measuring_Avarage:
                if (value >= 30)
                {
                    avarageDataList.Add(value);
                    Label.SetText($"測定中... (BPM: {value})");
                }
                else { Label.SetText("測定中... (心拍異常)"); }
                break;
            
            case AppStatus.Logging:
                if (value < 30)
                {
                    Label.SetText("心拍異常のため停止");
                    OnStopButtonPressed();
                }
                else { WriteToLog(value); }
                break;

            case AppStatus.Idle:
            case AppStatus.Result_Avarage:
            case AppStatus.Failed:
                break;
        }
    }

    //================================================================================
    // ★ アニメーション処理 (修正版)
    //================================================================================
    private void AnimateHeart()
    {
        if (heartIcon == null) return;

        // 1. BPMを滑らかに追従させる
        displayBpm = Mathf.Lerp(displayBpm, targetBpm, Time.deltaTime * 5.0f);

        // 2. ビートの間隔(秒)を計算 (例: 60BPM -> 1.0秒, 120BPM -> 0.5秒)
        float beatInterval = 60.0f / Mathf.Max(displayBpm, 1.0f); // 0除算防止

        // 3. タイマーを進める
        beatTimer += Time.deltaTime;

        // 4. タイマーが間隔を超えたら「ドクッ」とさせる
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval; // タイマーをリセット（余剰分は持ち越し）
            currentPulse = 1.0f;       // 膨らみ具合をMAXにする
        }

        // 5. 膨らみ具合(currentPulse)を、時間経過ですぐに0に戻す（減衰）
        //    Time.deltaTime * decaySpeed の速さで 0 に近づける
        currentPulse = Mathf.Lerp(currentPulse, 0f, Time.deltaTime * decaySpeed);

        // 6. サイズに反映 (minScale から maxScale の間で変化)
        float currentScale = Mathf.Lerp(minScale, maxScale, currentPulse);
        heartIcon.localScale = new Vector3(currentScale, currentScale, 1f);
    }

    //================================================================================
    // Button Click Handlers (変更なし)
    //================================================================================
    public void OnStartButtonPressed()
    {
        Debug.Log("Startボタン押下: 接続確認を開始");
        currentStatus = AppStatus.Checking_Start;
        UpdateButtons(AppStatus.Checking_Start);
        StartCoroutine(CheckConnection_Start());
    }
    public void OnStopButtonPressed()
    {
        Debug.Log("Stopボタン押下: ログ書き出しを停止");
        currentStatus = AppStatus.Idle;
        UpdateButtons(AppStatus.Idle);
        CloseLogFile();
    }
    public void OnResetButtonPressed()
    {
        if (currentStatus == AppStatus.Idle)
        {
            Debug.Log("Resetボタン押下: ログファイルをクリアします");
            ClearLogFile();
            Label.SetText("ログリセット完了");
        }
    }
    public void OnAvarageButtonPressed()
    {
        Debug.Log("Avarageボタン押下: 接続確認(3秒)を開始");
        currentStatus = AppStatus.Checking_Avarage;
        UpdateButtons(AppStatus.Checking_Avarage);
        StartCoroutine(CheckConnection_Avarage());
    }
    public void OnNextButtonPressed()
    {
        Debug.Log("Nextボタン押下: 待機状態に戻ります");
        currentStatus = AppStatus.Idle;
        UpdateButtons(AppStatus.Idle);
        Label.SetText("待機中");
    }

    //================================================================================
    // Coroutines (Timers) (変更なし)
    //================================================================================
    private IEnumerator CheckConnection_Start()
    {
        connectionCheckStatus = ConnectionCheckStatus.NotChecked;
        Label.SetText("接続確認中");
        yield return new WaitForSeconds(3f);
        if (connectionCheckStatus == ConnectionCheckStatus.Connected)
        {
            Debug.Log("接続成功。ログ書き出しを開始します。");
            currentStatus = AppStatus.Logging;
            UpdateButtons(AppStatus.Logging);
            OpenLogFile();
        }
        else
        {
            currentStatus = AppStatus.Failed;
            if (connectionCheckStatus == ConnectionCheckStatus.Failed_LowBPM)
            { Label.SetText("心拍が正常に確認できません"); }
            else { Label.SetText("接続できませんでした"); }
            currentStatus = AppStatus.Idle;
            UpdateButtons(AppStatus.Idle);
        }
    }
    private IEnumerator CheckConnection_Avarage()
    {
        connectionCheckStatus = ConnectionCheckStatus.NotChecked;
        Label.SetText("接続確認中");
        yield return new WaitForSeconds(3f);
        if (connectionCheckStatus == ConnectionCheckStatus.Connected)
        { StartCoroutine(Measure_Avarage()); }
        else
        {
            currentStatus = AppStatus.Failed;
            if (connectionCheckStatus == ConnectionCheckStatus.Failed_LowBPM)
            { Label.SetText("心拍が正常に確認できません"); }
            else { Label.SetText("接続できませんでした"); }
            currentStatus = AppStatus.Idle;
            UpdateButtons(AppStatus.Idle);
        }
    }
    private IEnumerator Measure_Avarage()
    {
        Debug.Log("12秒間の平均心拍数測定を開始");
        currentStatus = AppStatus.Measuring_Avarage;
        UpdateButtons(AppStatus.Measuring_Avarage);
        avarageDataList.Clear();
        Label.SetText("測定中...");
        yield return new WaitForSeconds(12f);
        currentStatus = AppStatus.Result_Avarage;
        if (avarageDataList.Count > 0)
        {
            double avg = avarageDataList.Average();
            Label.SetText($"平均心拍数: {avg:F1} BPM");
        }
        else { Label.SetText("測定データがありません"); }
        UpdateButtons(AppStatus.Result_Avarage);
    }

    //================================================================================
    // UI Helper (変更なし)
    //================================================================================
    private void UpdateButtons(AppStatus status)
    {
        StartButton.interactable = false;
        StopButton.interactable = false;
        ResetButton.interactable = false;
        AvarageButton.interactable = false;
        NextButton.gameObject.SetActive(false);
        if (status == AppStatus.Idle || status == AppStatus.Failed)
        {
            StartButton.interactable = true;
            ResetButton.interactable = true;
            AvarageButton.interactable = true;
        }
        else if (status == AppStatus.Logging)
        { StopButton.interactable = true; }
        else if (status == AppStatus.Result_Avarage)
        { NextButton.gameObject.SetActive(true); }
    }

    //================================================================================
    // CSV書き出しロジック (変更なし)
    //================================================================================
    private void PrepareLogFile()
    {
        try
        {
            string logDirectoryPath = Path.Combine(Application.dataPath, "CSV");
            logFilePath = Path.Combine(logDirectoryPath, "heart_rate_log.csv");
            if (!Directory.Exists(logDirectoryPath))
            { Directory.CreateDirectory(logDirectoryPath); }
        }
        catch (Exception e) { Debug.LogError($"ディレクトリ準備エラー: {e.Message}"); }
    }
    private void OpenLogFile()
    {
        if (heartRateLogWriter != null) return;
        try
        {
            heartRateLogWriter = new StreamWriter(logFilePath, true, System.Text.Encoding.UTF8);
            if (heartRateLogWriter.BaseStream.Length == 0)
            { heartRateLogWriter.WriteLine("Timestamp,BPM"); }
            heartRateLogWriter.Flush();
            Debug.Log($"CSVログファイルを開きました: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ログファイルを開けませんでした: {e.Message}");
            heartRateLogWriter = null;
        }
    }
    private void WriteToLog(int bpm)
    {
        if (heartRateLogWriter == null) return;
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            heartRateLogWriter.WriteLine($"{timestamp},{bpm}");
            heartRateLogWriter.Flush();
        }
        catch (Exception e) { Debug.LogError($"CSV書き込みエラー: {e.Message}"); }
    }
    private void CloseLogFile()
    {
        if (heartRateLogWriter != null)
        {
            Debug.Log("CSVログを閉じます。");
            heartRateLogWriter.Flush();
            heartRateLogWriter.Close();
            heartRateLogWriter = null;
        }
    }
    private void ClearLogFile()
    {
        CloseLogFile();
        try
        {
            File.WriteAllText(logFilePath, "Timestamp,BPM" + Environment.NewLine, System.Text.Encoding.UTF8);
            Debug.Log($"ログファイルをリセットしました: {logFilePath}");
        }
        catch (Exception e)
        { Debug.LogError($"ログファイルのリセットに失敗: {e.Message}"); }
    }
}