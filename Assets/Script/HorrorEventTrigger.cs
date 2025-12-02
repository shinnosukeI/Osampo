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
        if (hasTriggered && triggerOnce) return;

        // プレイヤーが接触したか判定 (タグ判定またはコンポーネント判定)
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null)
        {
            if (eventManager != null)
            {
                eventManager.TriggerHorrorEvent(eventType);
                hasTriggered = true;
            }
        }
    }
}
