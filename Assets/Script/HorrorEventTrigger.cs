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

    void OnEnable()
    {
        Debug.Log($"✅ [HorrorEventTrigger] Enabled: {gameObject.name} (Event {eventType}) | Layer: {LayerMask.LayerToName(gameObject.layer)} | IsActive: {gameObject.activeInHierarchy}");
        var col = GetComponent<Collider>();
        if (col != null)
        {
             Debug.Log($"   -> Collider: {col.GetType().Name}, Enabled: {col.enabled}, IsTrigger: {col.isTrigger}");
        }
        else
        {
             Debug.LogError("❌ [HorrorEventTrigger] No Collider found on this object!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // デバッグログ：何かが触れた
        Debug.Log($"⚡ [HorrorEventTrigger] OnTriggerENTER: {other.name}, Tag: {other.tag}, EventType: {eventType}");

        if (hasTriggered && triggerOnce) return;

        // プレイヤーが接触したか判定 (タグ判定またはコンポーネント判定)
        if (other.CompareTag("Player") || other.GetComponent<CharacterController>() != null || other.name.Contains("Player"))
        {
            Debug.Log($"🎯 [HorrorEventTrigger] Player Detected! EventID: {eventType}");
            
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

    void Update()
    {
        if (hasTriggered && triggerOnce) return;

        // 物理演算(OnTrigger)が効かない場合のための距離ベースのバックアップ判定
        // プレイヤーを探す（キャッシュ推奨だが、ひとまずFindで）
        // 頻繁な検索を避けるため、本来はStartでPlayerキャッシュすべきだが、
        // Playerが非アクティブ化→再生成されるケースがないならStartでよい。
        // ここでは安全に毎回チェック（重ければTime.frameCount判定を入れる）
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            
            // トリガーのスケールも考慮（一番大きい軸を基準にするなど）
            // BoxColliderの場合はそのサイズを見るのがベストだが、簡易的に「3.0m以内」とする
            // 必要に応じてInspectorで調整できるようにすると良い
            if (dist < 3.0f) 
            {
                 // 物理判定よりも前に距離で検知してしまった場合のログ
                 // Debug.Log($"📏 [HorrorEventTrigger] Distance Check Trigger: {dist}m (Event {eventType})");
                 
                 if (eventManager != null)
                 {
                    Debug.Log($"🎯 [HorrorEventTrigger] Force Trigger by Distance ({dist:F2}m). EventID: {eventType}");
                    eventManager.TriggerHorrorEvent(eventType);
                    hasTriggered = true;
                 }
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log($"🔄 [HorrorEventTrigger] Trigger Reset: {gameObject.name} (Event {eventType})");
    }
}
