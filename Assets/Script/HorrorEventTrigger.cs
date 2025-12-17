using UnityEngine;

public class HorrorEventTrigger : MonoBehaviour
{
    [Header("イベント設定")]
    [Tooltip("HorrorEventManagerで定義されたイベントIDを指定してください")]
    public int eventType;

    [Tooltip("一度だけ発動するかどうか")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;
    private HorrorEventManager eventManager;

    void Start()
    {
        // シーン内のHorrorEventManagerを探す
        eventManager = FindFirstObjectByType<HorrorEventManager>();
        if (eventManager == null)
        {
            Debug.LogError("HorrorEventTrigger: HorrorEventManagerが見つかりません。");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // デバッグログ：何かが触れた
        Debug.Log($"[HorrorEventTrigger] Hit: {other.name}, Tag: {other.tag}, EventType: {eventType}");

        if (hasTriggered && triggerOnce) return;

        // プレイヤーが接触したか判定 (タグ判定またはコンポーネント判定)
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            float dist = Vector3.Distance(other.transform.position, transform.position);
            Debug.Log($"[HorrorEventTrigger] Player detected! EventID: {eventType}");
            Debug.Log($"   Type: {eventType} | PlayerPos: {other.transform.position} | TriggerPos: {transform.position} | Dist: {dist}");
            
            if (eventManager != null)
            {
                eventManager.TriggerHorrorEvent(eventType);
                hasTriggered = true;
            }
            else
            {
                Debug.LogError("HorrorEventTrigger: EventManager is null!");
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        // Debug.Log($"[HorrorEventTrigger] Reset state for Event {eventType}");
    }
}
