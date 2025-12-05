using UnityEngine;

public class FallingCorpseTrigger : MonoBehaviour
{
    [SerializeField] private st1_HorrorEventManager eventManager;
    [SerializeField] private FallingCorpse corpse;

    [Header("○周目以降で落下させる")]
    [SerializeField] private int requiredCycle = 2;   // 例：2周目から有効

    private bool used = false;

    private void Awake()
    {
        if (eventManager == null)
        {
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (used) return;

        if (eventManager == null)
        {
            Debug.LogError("[CorpseTrigger] EventManager が見つかりません");
            return;
        }

        int current = eventManager.CycleCount;
        Debug.Log($"[CorpseTrigger] 周回 {current} でトリガーに侵入");

        // ★ 周回条件チェック（requiredCycle 未満なら何もしない）
        if (current < requiredCycle)
        {
            Debug.Log($"[CorpseTrigger] まだ {requiredCycle} 周目に達していないので落下させない");
            return;
        }

        // ★ 条件を満たしたので、このトリガー専用の落下イベントだけ実行
        if (corpse != null)
        {
            Debug.Log("[CorpseTrigger] 周回条件OK → 死体落下開始");
            corpse.StartFalling();
            used = true;   // 1回だけ落下
        }
        else
        {
            Debug.LogError("[CorpseTrigger] FallingCorpse が設定されていません");
        }
    }
}