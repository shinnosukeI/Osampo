using UnityEngine;
using System;
using System.Collections.Generic;

public class st1_HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

    [Header("45: ラジオイベント")]
    [SerializeField] private RadioEventController radioController;

    [Header("54: 落下イベント")]
    [SerializeField] private FallingCorpse fallingCorpse; 

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


    void Start()
    {
        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        Debug.Log("cycleEventTypes の要素数 = " + cycleEventTypes.Count);
        Debug.Log($"ゲーム開始時の周回 = {cycleCount}");
        RegisterEventActions();
    }

    // ★ 各イベントアクション登録
    private void RegisterEventActions()
    {
        eventActionMap[45] = TriggerRadioEvent;
        eventActionMap[54] = TriggerCorpseFall;    // ラジオイベント
        // eventActionMap[14] = TriggerZombieFall; など追加していく
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
        // ここではイベントを発動しない
    }

    // ★ トリガーから呼ぶ：今の周回のイベントを発動していいなら発動
    public bool TryTriggerCurrentCycleEvent()
    {
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
}