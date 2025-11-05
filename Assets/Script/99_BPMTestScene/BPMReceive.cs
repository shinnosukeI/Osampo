using UnityEngine;
using TMPro;
using System.IO; // ファイル書き出し(I/O)のために追加
using System;     // 日時(DateTime)のために追加
using System.Collections; // ★ コルーチン(Coroutine)のために追加

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
/// <summary>
/// メインクラス (接続確認機能付き)
/// </summary>
//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
public sealed class BPMReceive : MonoBehaviour
{
    //================================================================================
    // 状態定義
    //================================================================================

    /// <summary>
    /// 現在の動作ステータス
    /// </summary>
    private enum Status
    {
        Checking,   // 接続確認中
        Connected,  // 接続成功（ログ記録中）
        Failed      // 接続失敗
    }
    private Status currentStatus = Status.Checking;

    /// <summary>
    /// 接続確認フェーズ中にデータを受信したか
    /// </summary>
    private bool hasReceivedDataInCheckPhase = false;

    //================================================================================
    // Fields.
    //================================================================================

    /// <summary>
    /// ラベル.
    /// </summary>
    [SerializeField] private TextMeshProUGUI Label = default;

    /// <summary>
    /// CSVファイルに書き込むためのライター
    /// </summary>
    private StreamWriter heartRateLogWriter;

    /// <summary>
    /// ログファイルのフルパス
    /// </summary>
    private string logFilePath;

    //================================================================================
    // Unity Lifecycle Methods
    //================================================================================

    /// <summary>
    /// ゲーム開始時に1回呼ばれる初期化処理
    /// </summary>
    void Start()
    {
        // 1. UIを「接続確認中」に設定
        if (Label != null)
        {
            Label.SetText("接続確認中...");
        }

        // 2. 5秒後に接続状態を判定するコルーチンを開始
        StartCoroutine(CheckConnectionProcess(5.0f));
    }

    /// <summary>
    /// 接続確認のプロセス（5秒タイマー）
    /// </summary>
    private IEnumerator CheckConnectionProcess(float duration)
    {
        // 指定された秒数だけ待機
        yield return new WaitForSeconds(duration);

        // --- 5秒経過後 ---

        if (hasReceivedDataInCheckPhase)
        {
            // 接続成功
            currentStatus = Status.Connected;
            Debug.Log("接続成功。CSVログ記録を開始します。");
            
            // ★ このタイミングで初めてCSVファイルを開く
            InitializeCsvWriter();
        }
        else
        {
            // 接続失敗
            currentStatus = Status.Failed;
            Debug.LogWarning("接続確認できませんでした。CSV書き出しは行いません。");

            if (Label != null)
            {
                Label.SetText("接続確認できませんでした");
            }
        }
    }

    /// <summary>
    /// ゲーム（アプリ）終了時やオブジェクト破棄時に呼ばれる処理
    /// </summary>
    void OnDestroy()
    {
        // もしファイルが開かれていれば（＝接続成功していれば）閉じる
        if (heartRateLogWriter != null)
        {
            Debug.Log("CSVログを閉じます。");
            heartRateLogWriter.Flush();
            heartRateLogWriter.Close();
            heartRateLogWriter = null;
        }
    }

    //================================================================================
    // Event methods.
    //================================================================================

    /// <summary>
    /// Intの値が返るイベント (心拍計データ受信時に呼ばれる)
    /// </summary>
    /// <param name="value">値 (心拍数).</param>
    public void OnIntEvent(int value)
    {
        // 現在のステータスによって処理を分岐
        switch (currentStatus)
        {
            case Status.Checking:
                // 接続確認フェーズ中にデータを受信
                hasReceivedDataInCheckPhase = true; // フラグを立てる
                // UIは「接続確認中...」のまま変更しない
                break;

            case Status.Connected:
                // 接続成功（ログ記録中）フェーズ
                
                // 1. UIのテキストを更新
                if (Label != null)
                {
                    Label.SetText($"{value}");
                }
                // 2. CSVファイルへの書き込み処理
                WriteToCsv(value);
                break;

            case Status.Failed:
                // 接続失敗フェーズ
                // UIもCSVも処理しない
                break;
        }
    }

    //================================================================================
    // Helper Methods
    //================================================================================

    /// <summary>
    /// CSVファイルを開く（接続成功時に呼ばれる）
    /// </summary>
    private void InitializeCsvWriter()
    {
        string logDirectoryPath = Path.Combine(Application.dataPath, "CSV");
        logFilePath = Path.Combine(logDirectoryPath, "heart_rate_log.csv");

        try
        {
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }

            heartRateLogWriter = new StreamWriter(logFilePath, true, System.Text.Encoding.UTF8);

            if (heartRateLogWriter.BaseStream.Length == 0)
            {
                heartRateLogWriter.WriteLine("Timestamp,BPM");
            }
            
            heartRateLogWriter.Flush();
            Debug.Log($"CSVログの書き出しを初期化しました: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ログファイルを開けませんでした: {e.Message}");
            heartRateLogWriter = null;
            // 万が一CSVオープンに失敗したら、ステータスをFailedに戻す
            currentStatus = Status.Failed; 
            if (Label != null) Label.SetText("CSVファイルエラー");
        }
    }

    /// <summary>
    /// CSVに1行書き込む（接続成功時に呼ばれる）
    /// </summary>
    private void WriteToCsv(int value)
    {
        if (heartRateLogWriter == null) return; // 念のため

        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            heartRateLogWriter.WriteLine($"{timestamp},{value}");
            heartRateLogWriter.Flush(); // 即時書き込み
        }
        catch (Exception e)
        {
            Debug.LogError($"CSVへの書き込みエラー: {e.Message}");
        }
    }
}