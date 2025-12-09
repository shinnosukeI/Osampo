using UnityEngine;

public class CockroachSwarmCycleTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("ゴキブリ群")]
    [SerializeField] private CockroachSwarm cockroachSwarm;

    [Header("何周目で発動させるか")]
    [SerializeField] private int targetCycle = 3;

    [Header("プレイヤータグ名")]
    [SerializeField] private string playerTag = "Player";

    [Header("一度きりにするか")]
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤー以外は無視
        if (!other.CompareTag(playerTag)) return;

        // 一度きりなら、既に発動してたら何もしない
        if (onlyOnce && hasTriggered) return;

        if (eventManager == null)
        {
            Debug.LogError("[CockroachSwarmCycleTrigger] eventManager が設定されていません。");
            return;
        }

        if (cockroachSwarm == null)
        {
            Debug.LogError("[CockroachSwarmCycleTrigger] cockroachSwarm が設定されていません。");
            return;
        }

        int currentCycle = eventManager.CycleCount;
        Debug.Log($"[CockroachSwarmCycleTrigger] プレイヤーがトリガーに侵入。現在の周回 = {currentCycle}");

        // ★ ぴったり targetCycle 周目のときだけ発動
        if (currentCycle == targetCycle)
        {
            Debug.Log($"🪳 周回 {currentCycle} → ゴキブリイベント発動！");
            cockroachSwarm.StartSwarm();
            hasTriggered = true;
        }
        else
        {
            Debug.Log($"[CockroachSwarmCycleTrigger] 周回 {currentCycle} はゴキブリ発動対象外（ターゲット {targetCycle}）");
        }
    }
}