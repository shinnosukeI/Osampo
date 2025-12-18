using UnityEngine;
using System;
using System.Collections.Generic;

// ★ ステージ2専用ホラーイベントマネージャ
public class st2_HorrorEventManager : MonoBehaviour
{
    [Header("ステージ2用イベントデータベース")]
    [SerializeField]
    private HorrorEventDatabase stage2EventDatabase;

    [Header("14: ゾンビ落下イベント")]
    [SerializeField]
    private GameObject zombiePrefab; // FallingCorpseスクリプト付きのプレハブ
    [SerializeField]
    private Transform zombieSpawnPoint;

    // ★ ステージ2用イベントログ
    public List<(string Timestamp, int eventType)> stage2EventLog = new List<(string, int)>();

    // ★ ステージ2用：周回カウント（ドア/ワープを通った回数）
    [Header("ステージ2：周回カウント")]
    [SerializeField]
    private int stage2CycleCount = 1;
    public int Stage2CycleCount => stage2CycleCount;

    // ★ ステージ2用：周回ごとのイベント設定
    // 例: [0, 201, 202] → 1周目=なし / 2周目=201 / 3周目=202
    [Header("ステージ2：周回ごとのイベント設定")]
    [SerializeField]
    private List<int> stage2CycleEventTypes = new List<int>();

    // ★ イベントタイプ → 実処理（ステージ2用）
    private Dictionary<int, Action> stage2EventActionMap = new Dictionary<int, Action>();

    // ★ この周回ではもうイベントを発動したか？（ステージ2用）
    private int lastTriggeredStage2Cycle = 0;

    private void Start()
    {
        if (stage2EventDatabase != null)
        {
            stage2EventDatabase.Initialize();
        }

        Debug.Log($"[Stage2] cycleEventTypes 要素数 = {stage2CycleEventTypes.Count}");
        Debug.Log($"[Stage2] ゲーム開始時の周回 = {stage2CycleCount}");

        RegisterStage2EventActions();
    }

    /// <summary>
    /// ★ ステージ2用：イベントアクション登録
    /// ここにステージ2専用のイベントを追加していく
    /// 例） stage2EventActionMap[201] = SomeStage2EventMethod;
    /// </summary>
    private void RegisterStage2EventActions()
    {
        // まだステージ2イベントは未定義なので空にしておく
        // 必要になったらここに登録していく
    }

    /// <summary>
    /// ★ ステージ2用：ホラーイベント発動共通処理
    /// </summary>
    private void TriggerStage2HorrorEvent(int eventType)
    {
        HorrorEventData data = stage2EventDatabase?.GetEventData(eventType);

        if (data == null)
        {
            Debug.LogWarning($"[Stage2] イベントタイプ {eventType} のデータがありません。");
            return;
        }

        Debug.Log($"[Stage2] 🎃 イベント発生: {data.eventName} (Type: {eventType})");

        string currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        stage2EventLog.Add((currentTimestamp, eventType));

        if (stage2EventActionMap.TryGetValue(eventType, out Action action))
        {
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"[Stage2] ⚠ イベントタイプ {eventType} に対応するアクションが登録されていません。");
        }
    }

    /// <summary>
    /// ★ ステージ2：ドア/ワープから呼ぶ用
    /// 周回カウントを増やすだけのメソッド（ステージ1と混ざらない名前にした）
    /// </summary>
    public void OnStage2DoorPassed()
    {
        stage2CycleCount++;
        Debug.Log($"[Stage2] 🚪 ドア/ワープ通過で周回カウント: {stage2CycleCount}");
    }

    /// <summary>
    /// ★ ステージ2：トリガーから呼ぶ
    /// 今の周回に対応するイベントがあれば発動する
    /// </summary>
    public bool TryTriggerStage2CycleEvent()
    {
        if (stage2CycleCount <= 0)
        {
            Debug.Log("[Stage2][EventManager] まだ周回が始まっていないのでイベントなし");
            return false;
        }

        if (stage2CycleEventTypes == null || stage2CycleEventTypes.Count == 0)
        {
            Debug.LogWarning("[Stage2][EventManager] 周回ごとのイベントが設定されていません。");
            return false;
        }

        // この周回では既に発動済み
        if (lastTriggeredStage2Cycle == stage2CycleCount)
        {
            Debug.Log($"[Stage2][EventManager] 周回 {stage2CycleCount} は既にイベント発動済み");
            return false;
        }

        // 周回 → stage2CycleEventTypes の index（最後を繰り返す）
        int index = stage2CycleCount - 1;
        if (index >= stage2CycleEventTypes.Count)
        {
            index = stage2CycleEventTypes.Count - 1;
        }

        int eventType = stage2CycleEventTypes[index];

        // 0 を「何も起こさない」予約値にする
        if (eventType == 0)
        {
            Debug.Log($"[Stage2][EventManager] 周回 {stage2CycleCount} はイベントなし（eventType=0）");
            lastTriggeredStage2Cycle = stage2CycleCount;  // 二重発火防止
            return false;
        }

        lastTriggeredStage2Cycle = stage2CycleCount;
        Debug.Log($"[Stage2][EventManager] 周回 {stage2CycleCount} のトリガーからイベント {eventType} を発動");
        TriggerStage2HorrorEvent(eventType);
        return true;
    }
}