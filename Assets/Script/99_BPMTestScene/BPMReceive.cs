using UnityEngine;
using TMPro;
using System.IO; // ★ ファイル書き出し(I/O)のために追加
using System;     // ★ 日時(DateTime)のために追加

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
/// <summary>
/// メインクラス (心拍数受信＆CSV書き出しテスト用)
/// </summary>
//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
public sealed class BPMReceive : MonoBehaviour
{
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
        // 1. 保存先ディレクトリのパスを決定 (Assets/CSV/)
        // Application.dataPath は "Assets" フォルダを指します
        string logDirectoryPath = Path.Combine(Application.dataPath, "CSV");

        // 2. ログファイルのフルパスを決定
        logFilePath = Path.Combine(logDirectoryPath, "heart_rate_log.csv");

        try
        {
            // 3. ディレクトリが存在しない場合は、まずディレクトリを作成する
            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
                Debug.Log($"作成されたディレクトリ: {logDirectoryPath}");
            }

            // 4. ファイルを「追記モード(true)」で開く
            heartRateLogWriter = new StreamWriter(logFilePath, true, System.Text.Encoding.UTF8);

            // 5. ファイルが新規作成（空）の場合のみ、ヘッダーを書き込む
            if (heartRateLogWriter.BaseStream.Length == 0)
            {
                heartRateLogWriter.WriteLine("Timestamp,BPM");
            }
            
            // 6. ヘッダーを確実に書き込むために一度フラッシュ（バッファを書き出し）
            heartRateLogWriter.Flush();

            Debug.Log($"CSVログの書き出しを開始します: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ログファイルを開けませんでした: {e.Message}");
            heartRateLogWriter = null; // 開けなかったらnullにしておく
        }
    }

    /// <summary>
    /// ゲーム（アプリ）終了時やオブジェクト破棄時に呼ばれる処理
    /// </summary>
    void OnDestroy()
    {
        if (heartRateLogWriter != null)
        {
            Debug.Log("CSVログを閉じます。");
            heartRateLogWriter.Flush(); // ★ 停止前に、残っているデータをすべて書き出す
            heartRateLogWriter.Close(); // ★ ファイルを閉じる
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
        // 1. UIのテキストを更新 (既存の処理)
        if (Label != null)
        {
            Label.SetText($"{value}");
        }

        // 2. CSVファイルへの書き込み処理
        if (heartRateLogWriter != null)
        {
            try
            {
                // 3. 現在時刻を「年-月-日 時:分:秒」の形式で取得
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 4. CSVに1行書き込む (例: "2025-11-05 11:45:01,80")
                heartRateLogWriter.WriteLine($"{timestamp},{value}");
                
                // 5. (★重要★) テストのため、書き込むたびに即時フラッシュ
                // これにより、Unityが停止してもデータが失われるのを防ぎます
                heartRateLogWriter.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"CSVへの書き込みエラー: {e.Message}");
            }
        }
        else
        {
            // Start()で失敗していた場合に備える
            Debug.LogWarning("heartRateLogWriterが初期化されていません。ログは書き込まれません。");
        }
    }
}