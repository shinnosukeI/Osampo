using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class st1_HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

    [Header("45: ラジオイベント")]
    [SerializeField] private RadioEventController radioController;

    [Header("血の表現")]
    [SerializeField] private GameObject bloodOnFloor; // 床用

    [Header("雨の音")]
    [SerializeField] private AudioSource rainAudio;

    [Header("54: 落下イベント")]
    [SerializeField] private FallingCorpse fallingCorpse;

    [Header("55: 窓ガラスが割れるイベント")]
    [SerializeField]
    private GameObject normalWindowObject; // 割れる前の窓
    [SerializeField]
    private GameObject brokenWindowObject;

    [Header("11: ゴキブリイベント")]
    [SerializeField] private CockroachSwarm cockroachSwarmTarget;

    [Header("23: 吊り骸骨イベント (BoneTriggerで制御)")]
    // PrefabなどはBoneTrigger側で管理

    // ★ 周回カウント（ドア/ワープを通った回数）
    [Header("周回カウント")]
    [SerializeField] private int cycleCount = 1;
    public int CycleCount => cycleCount;
    

    // ★ 周回ごとのイベント設定
    // 例: [0, 45, 14] → 1周目=0(なし) / 2周目=45 / 3周目=14
    [Header("周回ごとのイベント設定")]
    [SerializeField] private List<int> cycleEventTypes = new List<int>();

    // イベントタイプ → アクション
    private Dictionary<int, Action> eventActionMap = new Dictionary<int, Action>();

    // この周回ではもうイベントを発動したか？
    private int lastTriggeredCycle = 0;

    // ログ保存用
    private StreamWriter eventLogWriter;

    void Start()
    {
        Debug.Log($"[EventManager START] name={name} id={GetInstanceID()} path={GetHierarchyPath(transform)}");
        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        // ログ保存の初期化
        InitializeEventLogger();

        Debug.Log("cycleEventTypes の要素数 = " + cycleEventTypes.Count);
        Debug.Log($"ゲーム開始時の周回 = {cycleCount}");
        RegisterEventActions();

        // ★ ラジオ再生イベントの購読
        if (radioController != null)
        {
            radioController.OnRadioPlaybackStarted += () => LogEvent(45);
        }

        // ★ 血のオブジェクトは最初は非表示
        if (bloodOnFloor != null) bloodOnFloor.SetActive(false);

        // ★ 1周目だけ雨の音を止める
        if (cycleCount == 1 && rainAudio != null)
        {
            rainAudio.Stop();
            Debug.Log("🌧️ 1周目なので雨の音を停止しました");
        }
    }

    // ★ 各イベントアクション登録
    private void RegisterEventActions()
    {
        eventActionMap[45] = TriggerRadioEvent;
        eventActionMap[54] = TriggerCorpseFall;     // 落下イベント
        eventActionMap[55] = TriggerWindowBreak;    // 55.ガラスが割れる
        eventActionMap[11] = TriggerCockroachSwarm; // 11.ゴキブリイベント
    }

    // ★ ラジオイベント実行
    private void TriggerRadioEvent()
    {
        if (radioController == null)
        {
            Debug.LogError("45: RadioEventController が設定されていません。");
            return;
        }

        Debug.Log("📻 ラジオイベント発動");
        radioController.PlayRadioSequence();
    }

    private void TriggerCockroachSwarm()
    {
        if (cockroachSwarmTarget == null)
        {
            Debug.LogError("11: CockroachSwarm が設定されていません。");
            return;
        }

        Debug.Log("🪳 11: ゴキブリイベント発動！");
        cockroachSwarmTarget.StartSwarm();
    }

    private void TriggerCorpseFall()
    {
        if (fallingCorpse == null)
        {
            Debug.LogError("54: FallingCorpse が設定されていません。");
            return;
        }

        Debug.Log("💀 54: 落下イベント発動");
        fallingCorpse.StartFalling();   // FallingCorpse 側にこのメソッドが必要
    }

    public bool TriggerEventFromTrigger(int eventType)
{
    // 45はラジオ再生開始時に別経路でログする設計なので必要なら弾く
    if (eventType == 45)
    {
        Debug.LogWarning("[EventManager] 45はOnRadioPlaybackStartedでログします。");
        return false;
    }

    Debug.Log($"[EventManager] 外部トリガーからイベント {eventType} を発火");
    TriggerHorrorEvent(eventType);
    return true;
}

    // 55: 窓ガラスが割れる
    public void TriggerWindowBreak()
    {
        if (normalWindowObject != null && brokenWindowObject != null)
        {
            Debug.Log("💥 窓ガラスが割れます！");
            normalWindowObject.SetActive(false);
            brokenWindowObject.SetActive(true);
        }
        else
        {
            Debug.LogError("55: 窓ガラスのGameObjectが設定されていません。");
        }
    }

    public void LogOnly(int eventType)
{
    LogEvent(eventType);
}

    // ★ 指定イベントを発動
    private void TriggerHorrorEvent(int eventType)
    {
        HorrorEventData data = eventDatabase?.GetEventData(eventType);

        if (data == null)
        {
            Debug.LogWarning($"イベントタイプ {eventType} のデータがありません。");
            return;
        }

        Debug.Log($"🎃 イベント発生: {data.eventName} (Type: {eventType})");

        string currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        eventLog.Add((currentTimestamp, eventType));

        // ログ保存 (45番はラジオ再生時にコールバックで保存するので除外)
        if (eventType != 45)
        {
            LogEvent(eventType);
        }

        if (eventActionMap.TryGetValue(eventType, out Action action))
        {
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"⚠ イベントタイプ {eventType} に対応するアクションがありません。");
        }
    }

    // ★ ドア（ワープ含む）で呼び出す：周回数を増やすだけ
    public void OnDoorClicked()
    {
        cycleCount++;
        Debug.Log($"🚪 ドア/ワープで周期カウント: {cycleCount}");

        // 6周目に入った瞬間だけ血を出す
        if (cycleCount == 6)
        {
            if (bloodOnFloor != null) bloodOnFloor.SetActive(true);
            Debug.Log("🩸 6周目：床に血を出しました");
        }

        // 2周目に入ったら雨を開始
        if (cycleCount == 2 && rainAudio != null)
        {
            rainAudio.Play();
            Debug.Log("🌧️ 2周目に入ったので雨を開始しました！");
        }
    }

    // ★ トリガーから呼ぶ：今の周回のイベントを発動していいなら発動
    public bool TryTriggerCurrentCycleEvent()
    {
        Debug.Log($"[TryTrigger] id={GetInstanceID()} cycle={cycleCount} last={lastTriggeredCycle}");
        // まだ1周目に入っていない
        if (cycleCount <= 0)
        {
            Debug.Log("[EventManager] まだ周回が始まっていないのでイベントなし");
            return false;
        }

        if (cycleEventTypes == null || cycleEventTypes.Count == 0)
        {
            Debug.LogWarning("[EventManager] 周回ごとのイベントが設定されていません。");
            return false;
        }

        // この周回では既に発動済み
        if (lastTriggeredCycle == cycleCount)
        {
            Debug.Log($"[EventManager] 周回 {cycleCount} は既にイベント発動済み");
            return false;
        }

        // 周回 → cycleEventTypes の index（最後を繰り返す）
        int index = cycleCount - 1;
        if (index >= cycleEventTypes.Count)
        {
            index = cycleEventTypes.Count - 1;
        }

        int eventType = cycleEventTypes[index];

        // 0 を「何も起こさない」予約値にする
        if (eventType == 0)
        {
            Debug.Log($"[EventManager] 周回 {cycleCount} はイベントなし（eventType=0）");
            lastTriggeredCycle = cycleCount;  // 二重発火防止
            return false;
        }

        lastTriggeredCycle = cycleCount;
        Debug.Log($"[EventManager] 周回 {cycleCount} のトリガーからイベント {eventType} を発動");
        TriggerHorrorEvent(eventType);
        return true;
    }
    // ========================================================================
    // ▼▼▼ ログ保存機能 ▼▼▼
    // ========================================================================

    private void InitializeEventLogger()
    {
        string subjectID = GameManager.SubjectID;
        if (string.IsNullOrEmpty(subjectID))
        {
            subjectID = "TestUser"; // IDがない場合のフォールバック
        }

        // ファイル名: ユーザー名_02_HorrorEvent_log.csv
        string fileName = $"{subjectID}_02_stage1_HorrorEvent_log.csv";
        string directoryPath = Path.Combine(Application.persistentDataPath, "CSV");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fullPath = Path.Combine(directoryPath, fileName);

        try
        {
            // 上書きモード(false)で作成
            eventLogWriter = new StreamWriter(fullPath, false, Encoding.UTF8);
            eventLogWriter.WriteLine("Timestamp,EventID"); // ヘッダー
            eventLogWriter.Flush();
            Debug.Log($"📄 [st1_HorrorEventManager] ログファイルを作成しました: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"⚠ [st1_HorrorEventManager] ログファイル作成エラー: {e.Message}");
        }
    }

    private void LogEvent(int eventType)
    {
        Debug.Log($"[LogEvent] id={GetInstanceID()} eventType={eventType} writerNull={(eventLogWriter == null)}");
        if (eventLogWriter != null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            eventLogWriter.WriteLine($"{timestamp},{eventType}");
            eventLogWriter.Flush(); // 即時書き込み
            Debug.Log($"📝 [st1_HorrorEventManager] イベントログ保存: {timestamp}, ID: {eventType}");
        }
    }

    private void CloseEventLogger()
    {
        if (eventLogWriter != null)
        {
            eventLogWriter.Flush();
            eventLogWriter.Close();
            eventLogWriter = null;
            Debug.Log("📄 [st1_HorrorEventManager] ログファイルを閉じました。");
        }
    }

    void OnDestroy()
    {
        CloseEventLogger();
    }
    
    private string GetHierarchyPath(Transform t)
{
    var sb = new StringBuilder(t.name);
    while (t.parent != null)
    {
        t = t.parent;
        sb.Insert(0, t.name + "/");
    }
    return sb.ToString();
}
}

