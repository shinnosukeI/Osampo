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
    private GameObject zombiePrefab; // ★ ステップ1で作成した物理演算ゾンビのプレハブ
    [SerializeField]
    private Transform zombieSpawnPoint;

    [Header("55: 窓ガラスが割れるイベント")]
    [SerializeField]
    private GameObject normalWindowObject; // 割れる前の窓（普段表示）
    [SerializeField]
    private GameObject brokenWindowObject;

    [Header("56: ボールが転がるイベント")] // ★ 追加
    [SerializeField]
    private GameObject ballPrefab;     // ボールのプレハブ
    [SerializeField]
    private Transform ballSpawnPoint;  // 出現位置

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

    // イベントタイプ → 実行アクション のマップ
    private Dictionary<int, Action> eventActionMap = new Dictionary<int, Action>();

    void Start()
    {
        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        RegisterEventActions();

        /////////// 🎬 起動時テスト（必要に応じてコメントアウト）//////////
        //TriggerHorrorEvent(54);
        //TriggerHorrorEvent(11);
        TriggerHorrorEvent(14);

    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {

        eventActionMap[54] = TriggerFallEvent; // 54:物が落ちる
        eventActionMap[11] = TriggerCockroachSwarm;
        eventActionMap[14] = TriggerZombieFall;
        eventActionMap[56] = TriggerBallRoll;
        /////////////////ここに追加/////////////////
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

        // 指定した出現位置(zombieSpawnPoint)に、プレハブ(zombiePrefab)を生成する
        Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
    }

    // 54:物が落ちる
    public void TriggerFallEvent()
    {
        // MakeObjectFall(objectToFallTarget); // ←古いコード

        if (objectToFallTarget != null)
        {
            objectToFallTarget.StartFall(); // ★ 落下オブジェクト自身の「StartFall」を呼び出す
        }
        else
        {
            Debug.LogError("落下対象(FallingObjectAudio)が設定されていません。");
        }
    }

    public void TriggerWindowBreak()
    {
        if (normalWindowObject != null && brokenWindowObject != null)
        {
            Debug.Log("💥 窓ガラスが割れます！");
            normalWindowObject.SetActive(false); // 通常の窓を非表示
            brokenWindowObject.SetActive(true);  // 割れるアニメーション付きの窓を表示
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
            
            // スポーン位置に、スポーン位置の向き(Rotation)でボールを生成
            Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("56: ボールのプレハブまたは出現位置が設定されていません。");
        }
    }



    ////////ここに関数を追加////////

}
