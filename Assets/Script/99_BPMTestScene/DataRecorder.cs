using UnityEngine;
using System.IO; // ファイル書き込みに必要
using System;     // 日時取得に必要

//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
/// <summary>
/// クラス図に基づくデータ記録クラス.
/// </summary>
//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
public class DataRecorder : MonoBehaviour
{
    // --- クラス図で定義されたフィールド (ファイルパス) ---
    [Header("ログファイル名")]
    public string writeHeartRateLog = "heart_rate_log.csv";
    public string writeEventLog = "event_log.csv";

    // --- 内部フィールド ---
    private StreamWriter heartRateLogWriter;
    private StreamWriter eventLogWriter;
    private string logDirectoryPath;

    //================================================================================
    // クラス図で定義されたメソッド
    //================================================================================

    /// <summary>
    /// ログファイルを開く (または新規作成する)
    /// </summary>
    public void OpenLogFiles()
    {
        // 1. 保存先ディレクトリのパスを決定 (Assets/CSV/)
        // Application.dataPath は "Assets" フォルダを指します
        logDirectoryPath = Path.Combine(Application.dataPath, "CSV");

        // 2. ディレクトリが存在しない場合は作成する
        if (!Directory.Exists(logDirectoryPath))
        {
            Directory.CreateDirectory(logDirectoryPath);
        }

        // 3. 各ログファイルのフルパスを決定
        string hrLogPath = Path.Combine(logDirectoryPath, writeHeartRateLog);
        string evtLogPath = Path.Combine(logDirectoryPath, writeEventLog);

        try
        {
            // 4. 心拍数ログを「追記モード(true)」で開く
            heartRateLogWriter = new StreamWriter(hrLogPath, true, System.Text.Encoding.UTF8);
            // 5. ファイルが新規作成の場合のみ、ヘッダーを書き込む
            if (heartRateLogWriter.BaseStream.Length == 0)
            {
                heartRateLogWriter.WriteLine("Timestamp,BPM");
            }

            // 6. イベントログも同様に開く
            eventLogWriter = new StreamWriter(evtLogPath, true, System.Text.Encoding.UTF8);
            if (eventLogWriter.BaseStream.Length == 0)
            {
                eventLogWriter.WriteLine("Timestamp,eventType");
            }
            
            Debug.Log($"ログ書き出し開始: {logDirectoryPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ログファイルを開けませんでした: {e.Message}");
        }
    }

    /// <summary>
    /// ログファイルを閉じる
    /// </summary>
    public void CloseLogFiles()
    {
        if (heartRateLogWriter != null)
        {
            heartRateLogWriter.Flush(); // バッファを書き出し
            heartRateLogWriter.Close();
            heartRateLogWriter = null;
        }
        if (eventLogWriter != null)
        {
            eventLogWriter.Flush();
            eventLogWriter.Close();
            eventLogWriter = null;
        }
        Debug.Log("ログファイルを閉じました。");
    }

    /// <summary>
    /// 心拍数を記録する
    /// </summary>
    /// <param name="bpm">心拍数</param>
    public void RecordHeartRate(int bpm)
    {
        if (heartRateLogWriter == null) return; // ファイルが開かれていない

        try
        {
            // 年月日時分秒のタイムスタンプ
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // CSVに書き込み (例: "2025-11-05 11:30:00,80")
            heartRateLogWriter.WriteLine($"{timestamp},{bpm}");
        }
        catch (Exception e)
        {
            Debug.LogError($"心拍数ログ書き込みエラー: {e.Message}");
        }
    }

    /// <summary>
    /// 恐怖イベントを記録する
    /// </summary>
    /// <param name="eventType">イベントの種類やID</param>
    public void RecordEvent(string eventType)
    {
        if (eventLogWriter == null) return; // ファイルが開かれていない

        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // CSVに書き込み (例: "2025-11-05 11:30:15,JumpScare_01")
            eventLogWriter.WriteLine($"{timestamp},{eventType}");
        }
        catch (Exception e)
        {
            Debug.LogError($"イベントログ書き込みエラー: {e.Message}");
        }
    }
}