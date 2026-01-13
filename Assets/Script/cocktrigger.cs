using UnityEngine;

public class CockroachSwarmCycleTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("何周目で発動させるか")]
    [SerializeField] private int targetCycle = 3;

    [Header("プレイヤータグ名")]
    [SerializeField] private string playerTag = "Player";

    [Header("一度きりにするか")]
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered = false;

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (onlyOnce && hasTriggered) return;

        if (eventManager == null)
        {
            Debug.LogError("[CockroachSwarmCycleTrigger] eventManager が見つかりません。");
            return;
        }

        int currentCycle = eventManager.CycleCount;
        Debug.Log($"[CockroachSwarmCycleTrigger] プレイヤーがトリガーに侵入。現在の周回 = {currentCycle}");

        if (currentCycle == targetCycle)
        {
            Debug.Log($"🪳 周回 {currentCycle} → ゴキブリイベント発動！（EventManager経由）");
            eventManager.TriggerEventFromTrigger(11); // ★ログ＆実行を統一
            hasTriggered = true;
        }
        else
        {
            Debug.Log($"[CockroachSwarmCycleTrigger] 周回 {currentCycle} は発動対象外（ターゲット {targetCycle}）");
        }
    }
}