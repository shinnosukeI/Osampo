using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HeartRateManager : MonoBehaviour
{
    [SerializeField] private DataRecorder dataRecorder;

    // 受信データ
    public int CurrentBPM { get; private set; } = 0;
    public bool IsSensorActive { get; private set; } = false;

    // RestSceneManagerに結果を知らせるためのイベント
    public event Action<bool> OnConnectionCheckFinished; // true=成功
    public event Action<float> OnMeasurementFinished;    // 計測された平均BPM

    void Start()
    {
        // アタッチし忘れ防止
        if (dataRecorder == null) dataRecorder = GetComponent<DataRecorder>();
    }

    // ★★★ EventReceiverから呼ばれる関数（デバッグログ追加版） ★★★
    public void OnIntEvent(int value)
    {
        // ▼ この行を追加：どんな値でも受信したらコンソールに表示する
        Debug.Log($"【受信テスト】生の受信データ: {value}"); 

        // 30未満はノイズまたは未装着とみなすフィルタ
        if (value >= 30)
        {
            CurrentBPM = value;
            IsSensorActive = true;
            // ▼ 成功時もログを出す
            Debug.Log($"【受信成功】有効なBPMとして認識: {value}");
        }
        else
        {
            IsSensorActive = false;
            // ▼ 無視した場合もログを出す
            Debug.Log($"【受信無視】値が低すぎます: {value}");
        }
    }

    // RestScene1開始時に呼ばれる
    public void StartRestSceneSequence()
    {
        StartCoroutine(ConnectionAndMeasureFlow());
    }

    private IEnumerator ConnectionAndMeasureFlow()
    {
        // --- 1. 接続確認 (最大3秒) ---
        Debug.Log("【HeartRateManager】接続確認中...");
        float timer = 0f;
        bool connected = false;

        while (timer < 3.0f)
        {
            if (IsSensorActive && CurrentBPM > 0)
            {
                connected = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (!connected)
        {
            // ▼▼▼ 修正箇所: LogError を LogWarning に変更 ▼▼▼
            // LogErrorだとUnityの設定によっては実行が一時停止してしまうため、
            // 「想定内の失敗」としてWarning（警告）にします。
            Debug.LogWarning("【HeartRateManager】接続失敗 (タイムアウト)"); 
            
            OnConnectionCheckFinished?.Invoke(false); // 失敗を通知
            yield break; // 計測処理には進まず、ここでコルーチンを抜ける
        }

        // 成功を通知
        OnConnectionCheckFinished?.Invoke(true);

        // --- 2. 計測開始 (10秒間) ---
        Debug.Log("【HeartRateManager】平常時計測開始...");
        
        if (dataRecorder != null)
        {
            dataRecorder.OpenLogFile("01_default_bpm_log");
            dataRecorder.RecordMemo("RestScene1 Default Measurement");
        }

        List<int> bpmList = new List<int>();
        int durationSeconds = 10;
        
        for (int i = 0; i < durationSeconds; i++)
        {
            if (IsSensorActive)
            {
                bpmList.Add(CurrentBPM);
                dataRecorder?.RecordHeartRate(CurrentBPM);
                Debug.Log($"計測: {CurrentBPM} BPM"); // 確認用ログ
            }
            
            // 1秒待機 (これで重複を防ぐ)
            yield return new WaitForSeconds(1.0f);
        }

        dataRecorder?.CloseLogFiles();

        // 平均算出
        float averageBpm = 0f;
        if (bpmList.Count > 0)
        {
            double avg = bpmList.Average();
            averageBpm = (float)Math.Round(avg, 1, MidpointRounding.AwayFromZero);
        }

        Debug.Log($"【HeartRateManager】計測完了: 平均 {averageBpm}");
        OnMeasurementFinished?.Invoke(averageBpm); // 結果を通知
    }
}