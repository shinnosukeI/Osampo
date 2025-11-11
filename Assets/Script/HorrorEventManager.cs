using UnityEngine;
using System;
using System.Collections.Generic;

public class HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    [SerializeField]
    private FallingObjectAudio objectToFallTarget; // 54：物が落ちるイベント用

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
        TriggerHorrorEvent(54);
    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {

        eventActionMap[54] = TriggerFallEvent; // 54:物が落ちる

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

////////ここに関数を追加////////

}
