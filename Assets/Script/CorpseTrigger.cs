using UnityEngine;

public class FallingCorpseTrigger : MonoBehaviour
{
    [SerializeField] private st1_HorrorEventManager eventManager;
    [SerializeField] private FallingCorpse corpse;

    [Header("○周目以降で落下させる")]
    [SerializeField] private int requiredCycle = 2;

    private bool used = false;

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[CorpseTrigger] OnTriggerEnter hit: {other.name} tag={other.tag}");

        if (!other.CompareTag("Player")) return;
        if (used) return;

        if (eventManager == null)
        {
            Debug.LogError("[CorpseTrigger] st1_HorrorEventManager が見つかりません");
            return;
        }

        int current = eventManager.CycleCount;
        Debug.Log($"[CorpseTrigger] CycleCount={current}, required={requiredCycle}");

        if (current < requiredCycle)
        {
            Debug.Log("[CorpseTrigger] 周回不足でスキップ");
            return;
        }

        if (corpse == null)
        {
            Debug.LogError("[CorpseTrigger] FallingCorpse が未設定");
            return;
        }

        Debug.Log("[CorpseTrigger] 条件OK → StartFalling()");
        corpse.StartFalling();
        used = true;
    }
}