using UnityEngine;
using System;
using System.Collections.Generic;

public class HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    [SerializeField]
    private FallingObjectAudio objectToFallTarget; // 54：物が落ちるイベント用

    [SerializeField]
    private CockroachSwarm cockroachSwarmTarget;// ★ 11：ゴキブリイベント用

    [Header("14: ゾンビ落下イベント")]
    [SerializeField]
    private GameObject zombiePrefab; // 物理演算ゾンビのプレハブ
    [SerializeField]
    private Transform zombieSpawnPoint;

    [SerializeField] private GameObject bloodSplashObject; // 32: 血痕

    [Header("31: 血が滴るイベント")]
    [SerializeField]
    private GameObject bloodDripObject;

    [Header("45: ラジオイベント")]
    [SerializeField]
    private RadioEventController radioController;

    [Header("55: 窓ガラスが割れるイベント")]
    [SerializeField]
    private GameObject normalWindowObject; // 割れる前の窓
    [SerializeField]
    private GameObject brokenWindowObject;

    [Header("56: ボールが転がるイベント")]
    [SerializeField]
    private GameObject ballPrefab;     // ボールのプレハブ
    [SerializeField]
    private Transform ballSpawnPoint;  // 出現位置

    [Header("24: 窓に手形イベント")]
    [SerializeField]
    private HandprintEvent handprintEventTarget;

    [Header("25: 壁に目イベント")]
    [SerializeField]
    private WallEyesEvent wallEyesEventTarget;

    [Header("21: 人形移動イベント")]
    [SerializeField]
    private BearMoveEvent bearMoveEventTarget;

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

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

        // 起動時テストは必要なら使う
        //TriggerHorrorEvent(54);
        //TriggerHorrorEvent(14);
        //TriggerHorrorEvent(31);
    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {
        eventActionMap[11] = TriggerCockroachSwarm;
        eventActionMap[14] = TriggerZombieFall;
        eventActionMap[31] = TriggerBloodDrip;
        eventActionMap[45] = TriggerRadio;
        eventActionMap[54] = TriggerFallEvent;
        eventActionMap[56] = TriggerBallRoll;

        eventActionMap[24] = TriggerHandprint;
        eventActionMap[25] = TriggerWallEyes;
        eventActionMap[21] = TriggerBearMove;
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

    // 11: 大量のゴキブリが出現する
    public void TriggerCockroachSwarm()
    {
        if (cockroachSwarmTarget != null)
        {
            cockroachSwarmTarget.StartSwarm();
        }
        else
        {
            Debug.LogError("ゴキブリ(CockroachSwarm)が設定されていません。");
        }
    }

    // 14: ゾンビが降ってくる
    public void TriggerZombieFall()
    {
        if (zombiePrefab == null || zombieSpawnPoint == null)
        {
            Debug.LogError("14: ゾンビのプレハブまたは出現位置が設定されていません。");
            return;
        }

        Debug.Log("😱 ゾンビが降ってきます！");
        Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
    }

    // 31: 血がしたたり落ちる
    public void TriggerBloodDrip()
    {
        if (bloodDripObject != null)
        {
            Debug.Log("🩸 血が滴り始めました...");
            bloodDripObject.SetActive(true);
        }
        else
        {
            Debug.LogError("31: 血のパーティクルが設定されていません。");
        }
    }

    // 45: ラジオから音がする
    public void TriggerRadio()
    {
        if (radioController != null)
        {
            radioController.PlayRadioSequence();
        }
        else
        {
            Debug.LogError("45: ラジオコントローラーが設定されていません。");
        }
    }

    // 54: 物が落ちる
    public void TriggerFallEvent()
    {
        if (objectToFallTarget != null)
        {
            objectToFallTarget.StartFall();
        }
        else
        {
            Debug.LogError("落下対象(FallingObjectAudio)が設定されていません。");
        }
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

    // 56: ボールが転がってくる
    public void TriggerBallRoll()
    {
        if (ballPrefab != null && ballSpawnPoint != null)
        {
            Debug.Log("⚽ ボールが転がってきます！");
            Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("56: ボールのプレハブまたは出現位置が設定されていません。");
        }
    }

    // 24: 窓に手形
    public void TriggerHandprint()
    {
        if (handprintEventTarget != null)
        {
            handprintEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError($"24: HandprintEventが設定されていません。(Object: {gameObject.name})");
        }
    }

    // 25: 壁に目
    public void TriggerWallEyes()
    {
        if (wallEyesEventTarget != null)
        {
            wallEyesEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError("25: WallEyesEventが設定されていません。");
        }
    }

    // 21: 人形移動
    public void TriggerBearMove()
    {
        if (bearMoveEventTarget != null)
        {
            bearMoveEventTarget.MoveToNextPosition();
        }
        else
        {
            Debug.LogError("21: BearMoveEventが設定されていません。");
        }
    }

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
