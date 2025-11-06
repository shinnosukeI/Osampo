using UnityEngine;
using UnityEngine.UI; // Button
using TMPro;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic; // ★ List のために追加
using System.Linq; // ★ 平均計算(Average)のために追加

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
/// <summary>
/// メインクラス (Avarage機能付き)
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
    [SerializeField] private Button AvarageButton = default; // ★ 追加
    [SerializeField] private Button NextButton = default;    // ★ 追加

    //================================================================================
    // Fields - CSV & Logging
    //================================================================================

    private StreamWriter heartRateLogWriter;
    private string logFilePath;

    /// <summary>
    /// ログ書き出しの状態管理
    /// </summary>
    private enum AppStatus 
    { 
        Idle,           // 待機中 (Start/Reset/Avarage が押せる)
        Checking_Start, // Start後の接続確認中
        Logging,        // CSV書き出し中 (Stopが押せる)
        Checking_Avarage, // Avarage後の接続確認中
        Measuring_Avarage, // Avarageの12秒測定中
        Result_Avarage, // Avarageの結果表示中 (Nextが押せる)
        Failed          // 失敗状態 (Idleに戻すトリガー)
    }
    private AppStatus currentStatus = AppStatus.Idle;

    /// <summary>
    /// 接続確認用の内部状態 (3秒タイマー用)
    /// </summary>
    private enum ConnectionCheckStatus { NotChecked, Connected, Failed_NoData, Failed_LowBPM }
    private ConnectionCheckStatus connectionCheckStatus = ConnectionCheckStatus.NotChecked;

    /// <summary>
    /// Avarage測定用の心拍数データリスト
    /// </summary>
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
        AvarageButton.onClick.AddListener(OnAvarageButtonPressed); // ★ 追加
        NextButton.onClick.AddListener(OnNextButtonPressed);       // ★ 追加

        // UIの初期状態を設定
        Label.SetText("待機中");
        UpdateButtons(AppStatus.Idle);
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
        // ----- 1. 接続確認中の処理 (Start/Avarage共通) -----
        if (currentStatus == AppStatus.Checking_Start || currentStatus == AppStatus.Checking_Avarage)
        {
            if (value >= 30)
            {
                connectionCheckStatus = ConnectionCheckStatus.Connected;
            }
            else if (connectionCheckStatus != ConnectionCheckStatus.Connected) // 正常値を一度も受信してない場合
            {
                connectionCheckStatus = ConnectionCheckStatus.Failed_LowBPM;
            }
        }

        // ----- 2. Avarage測定中の処理 -----
        if (currentStatus == AppStatus.Measuring_Avarage)
        {
            if (value >= 30)
            {
                avarageDataList.Add(value); // CSV書き出しはせず、リストにのみ追加
                Label.SetText($"測定中... (BPM: {value})");
            }
            else
            {
                Label.SetText("測定中... (心拍異常)");
            }
            return; // 測定中は他の処理をしない
        }
        
        // ----- 3. ログ書き出し中の処理 -----
        if (currentStatus == AppStatus.Logging)
        {
            if (value < 30)
            {
                Label.SetText("心拍異常のため停止");
                OnStopButtonPressed();
                return;
            }
            else
            {
                Label.SetText($"{value}"); // 正常な心拍数を表示
                WriteToLog(value); // CSVに書き込む
            }
            return;
        }

        // ----- 4. 待機中(Idle) または 結果表示中(Result) のリアルタイム表示 -----
        if (currentStatus == AppStatus.Idle)
        {
             if (value < 30)
            {
                Label.SetText("心拍が正常に確認できません");
            }
            else
            {
                Label.SetText($"{value}"); // 正常な心拍数を表示
            }
        }
        // (Checking中は "接続確認中"、Result中は "平均: XX" が表示され続ける)
    }

    //================================================================================
    // Button Click Handlers
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

    // ★ Avarageボタンの処理
    public void OnAvarageButtonPressed()
    {
        Debug.Log("Avarageボタン押下: 接続確認(3秒)を開始");
        currentStatus = AppStatus.Checking_Avarage;
        UpdateButtons(AppStatus.Checking_Avarage);
        StartCoroutine(CheckConnection_Avarage());
    }

    // ★ Nextボタンの処理
    public void OnNextButtonPressed()
    {
        Debug.Log("Nextボタン押下: 待機状態に戻ります");
        currentStatus = AppStatus.Idle;
        UpdateButtons(AppStatus.Idle);
        Label.SetText("待機中"); // 常時表示に戻す
    }


    //================================================================================
    // Coroutines (Timers)
    //================================================================================

    /// <summary>
    /// (Startボタン用) 接続確認のタイマー処理
    /// </summary>
    private IEnumerator CheckConnection_Start()
    {
        connectionCheckStatus = ConnectionCheckStatus.NotChecked;
        Label.SetText("接続確認中");
        
        yield return new WaitForSeconds(3f); // 3秒待機

        if (connectionCheckStatus == ConnectionCheckStatus.Connected)
        {
            Debug.Log("接続成功。ログ書き出しを開始します。");
            currentStatus = AppStatus.Logging;
            UpdateButtons(AppStatus.Logging);
            OpenLogFile();
        }
        else
        {
            // 接続失敗
            currentStatus = AppStatus.Failed; // 失敗状態
            if (connectionCheckStatus == ConnectionCheckStatus.Failed_LowBPM)
            {
                Label.SetText("心拍が正常に確認できません");
            }
            else // NotChecked
            {
                Label.SetText("接続できませんでした");
            }
            // 失敗したらIdleに戻す
            currentStatus = AppStatus.Idle;
            UpdateButtons(AppStatus.Idle);
        }
    }

    /// <summary>
    /// (Avarageボタン用) 接続確認のタイマー処理
    /// </summary>
    private IEnumerator CheckConnection_Avarage()
    {
        connectionCheckStatus = ConnectionCheckStatus.NotChecked;
        Label.SetText("接続確認中");

        yield return new WaitForSeconds(3f); // 3秒待機

        if (connectionCheckStatus == ConnectionCheckStatus.Connected)
        {
            // 接続成功 -> 12秒測定フェーズに移行
            StartCoroutine(Measure_Avarage());
        }
        else
        {
            // 接続失敗
            currentStatus = AppStatus.Failed;
            if (connectionCheckStatus == ConnectionCheckStatus.Failed_LowBPM)
            {
                Label.SetText("心拍が正常に確認できません");
            }
            else // NotChecked
            {
                Label.SetText("接続できませんでした");
            }
            // 失敗したらIdleに戻す
            currentStatus = AppStatus.Idle;
            UpdateButtons(AppStatus.Idle);
        }
    }

    /// <summary>
    /// (Avarageボタン用) 12秒間の測定処理
    /// </summary>
    private IEnumerator Measure_Avarage()
    {
        Debug.Log("12秒間の平均心拍数測定を開始");
        currentStatus = AppStatus.Measuring_Avarage;
        UpdateButtons(AppStatus.Measuring_Avarage);
        avarageDataList.Clear(); // 測定リストを初期化
        Label.SetText("測定中...");

        yield return new WaitForSeconds(12f); // 12秒待機

        // --- 12秒後の結果発表 ---
        currentStatus = AppStatus.Result_Avarage;
        
        if (avarageDataList.Count > 0)
        {
            double avg = avarageDataList.Average();
            Label.SetText($"平均心拍数: {avg:F1} BPM");
        }
        else
        {
            Label.SetText("測定データがありません");
        }
        
        UpdateButtons(AppStatus.Result_Avarage); // Nextボタンを表示
    }

    //================================================================================
    // UI Helper
    //================================================================================

    private void UpdateButtons(AppStatus status)
    {
        // デフォルトは全ボタン無効
        StartButton.interactable = false;
        StopButton.interactable = false;
        ResetButton.interactable = false;
        AvarageButton.interactable = false;
        NextButton.gameObject.SetActive(false); // Nextは非表示がデフォルト

        // 状態に応じて有効化
        if (status == AppStatus.Idle || status == AppStatus.Failed)
        {
            StartButton.interactable = true;
            ResetButton.interactable = true;
            AvarageButton.interactable = true;
        }
        else if (status == AppStatus.Logging)
        {
            StopButton.interactable = true;
        }
        else if (status == AppStatus.Result_Avarage)
        {
            NextButton.gameObject.SetActive(true); // 表示＆有効
        }
        // (Checking系、Measuring中は全ボタン無効)
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
            {
                Directory.CreateDirectory(logDirectoryPath);
            }
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
            {
                heartRateLogWriter.WriteLine("Timestamp,BPM");
            }
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
        {
            Debug.LogError($"ログファイルのリセットに失敗: {e.Message}");
        }
    }
}