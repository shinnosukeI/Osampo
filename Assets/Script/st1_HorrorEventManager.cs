using UnityEngine;
using System;
using System.Collections.Generic;

public class st1_HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

    //========参考=========
    //[Header("14: ゾンビ落下イベント")]
    //[SerializeField]
    //private GameObject zombiePrefab; // FallingCorpseスクリプト付きのプレハブ
    //[SerializeField]
    //private Transform zombieSpawnPoint;
    //=====================


    // ★ 周期カウント（ドア/ワープした回数）
    [Header("周回カウント")]
    [SerializeField] private int cycleCount = 0;
    public int CycleCount => cycleCount;

    // ★ 周回ごとに発生させるイベントタイプの一覧
    // 例: [54, 14, 31] → 1周目=54, 2周目=14, 3周目=31
    [Header("周回ごとのイベント設定")]
    [SerializeField] private List<int> cycleEventTypes = new List<int>();

    // イベントタイプ → 実行アクション のマップ
    private Dictionary<int, Action> eventActionMap = new Dictionary<int, Action>();

    void Start()
    {
        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        RegisterEventActions();

        // 起動時テスト。必要なら使う
        //TriggerHorrorEvent(54);
        //TriggerHorrorEvent(14);
        //TriggerHorrorEvent(31);
    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {
        //========参考=========
        //eventActionMap[14] = TriggerZombieFall;
        //=====================

    }

    /// <summary>
    /// イベントを発動
    /// </summary>
    public void TriggerHorrorEvent(int eventType)
    {
        HorrorEventData data = eventDatabase?.GetEventData(eventType);

        if (data == null)
        {
            Debug.LogWarning($"イベントタイプ {eventType} がデータベースに存在しません。");
            return;
        }

        Debug.Log($"🎃 イベント発生: {data.eventName} (Type: {eventType})");

        string currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        eventLog.Add((currentTimestamp, eventType));

        // イベント固有のアクションが登録されていれば実行
        if (eventActionMap.TryGetValue(eventType, out Action action))
        {
            action.Invoke();
        }
        else
        {
            Debug.Log($"⚠ イベントタイプ {eventType} に対応するアクションが登録されていません。");
        }
    }

    // ======== 各イベント処理 ========

    // 参考===14: ゾンビが降ってくる
    //public void TriggerZombieFall()
    //{
        //if (zombiePrefab == null || zombieSpawnPoint == null)
        //{
           // Debug.LogError("14: ゾンビのプレハブまたは出現位置が設定されていません。");
           //return;
        //}

       // Debug.Log("😱 ゾンビが降ってきます！");
       //Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
    //}


    // ============================
    // ★ ドア（ワープ含む）で呼び出す周期カウント
    // ============================
    public void OnDoorClicked()
    {
        // 周回カウントを増やす
        cycleCount++;
        Debug.Log($"🚪 ドア/ワープで周期カウント: {cycleCount}");

        if (cycleEventTypes == null || cycleEventTypes.Count == 0)
        {
            Debug.LogWarning("周回ごとのイベントが設定されていません。");
            return;
        }

        // --- パターンA: 最後の要素を以降も使い続ける ---
        int index = cycleCount - 1;
        if (index >= cycleEventTypes.Count)
        {
            index = cycleEventTypes.Count - 1; // 最後の要素
        }

        int eventType = cycleEventTypes[index];
        Debug.Log($"🎃 周回 {cycleCount} でイベント {eventType} を実行");
        TriggerHorrorEvent(eventType);

        /* --- パターンB: リストをループさせたい場合 ---
        // 例: [54,14,31] → 1周目=54, 2=14, 3=31, 4=54...
        int index = (cycleCount - 1) % cycleEventTypes.Count;
        int eventType = cycleEventTypes[index];
        TriggerHorrorEvent(eventType);
        ------------------------------------------------- */
    }
}
