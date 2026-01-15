using UnityEngine;

public class BoneTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("何周目で発動させるか")]
    [SerializeField] private int targetCycle = 4;

    [Header("ログ用イベントID（骸骨）")]
    [SerializeField] private int boneEventID = 23; // ★ ID changed to 23

    [Header("天井アンカー")]
    [SerializeField] private Transform hangAnchor;

    [Header("骸骨（Prefab）")]
    [SerializeField] private HangingSkull skullPrefab;

    [Header("一度きり")]
    [SerializeField] private bool onlyOnce = true;

    private bool triggered = false;

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (onlyOnce && triggered) return;

        if (eventManager == null || hangAnchor == null || skullPrefab == null)
        {
            Debug.LogError("[BoneTrigger] 参照が不足しています");
            return;
        }

        int current = eventManager.CycleCount;
        if (current != targetCycle) return;

        triggered = true;

        // ★ 先にログだけ取る（仮）
        eventManager.LogOnly(boneEventID);

        // ★ 骸骨生成（最初は非表示）
        HangingSkull skull = Instantiate(skullPrefab);
        skull.gameObject.SetActive(true);

        // ★ 吊って揺らす
        skull.HangAndSwing(hangAnchor);

        Debug.Log($"☠️ [BoneTrigger] Cycle={current} 骸骨イベント発生（ID:{boneEventID}）");
    }
}

//test