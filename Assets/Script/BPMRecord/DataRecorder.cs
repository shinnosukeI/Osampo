using UnityEngine;
using System.IO;
using System;
using System.Text;

public class DataRecorder : MonoBehaviour
{
    private StreamWriter currentStreamWriter;

    // ログを保存するフォルダを開く・作る
    public void OpenLogFile(string fileName)
    {
        string directoryPath = Path.Combine(Application.dataPath, "CSV");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fullPath = Path.Combine(directoryPath, fileName + ".csv");

        try
        {
            // false = 上書きモード (新規作成)
            currentStreamWriter = new StreamWriter(fullPath, false, Encoding.UTF8);
            currentStreamWriter.WriteLine("Timestamp,BPM"); // ヘッダー
            currentStreamWriter.Flush();
            Debug.Log($"CSV作成: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSVエラー: {e.Message}");
        }
    }

    public void RecordHeartRate(int bpm)
    {
        if (currentStreamWriter != null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            currentStreamWriter.WriteLine($"{timestamp},{bpm}");
        }
    }

    public void RecordMemo(string memo)
    {
        if (currentStreamWriter != null)
        {
            currentStreamWriter.WriteLine($"# {memo}");
        }
    }

    public void CloseLogFiles()
    {
        if (currentStreamWriter != null)
        {
            currentStreamWriter.Flush();
            currentStreamWriter.Close();
            currentStreamWriter = null;
        }
    }

    void OnDestroy()
    {
        CloseLogFiles();
    }
}