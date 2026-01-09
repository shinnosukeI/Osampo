using UnityEngine;

/// <summary>
/// トリガー（Collider）を使わずに、
/// プレイヤーが一定距離に近づいただけでログを記録するためのコンポーネント。
/// イベントの演出（音やアニメーション）は再生せず、ログのみを残したい場合に使用します。
/// </summary>
public class ProximityLogTrigger : MonoBehaviour
{
    [Header("Log Settings")]
    [Tooltip("ログに記録するイベントID")]
    public int eventType;

    [Tooltip("ログを記録する検知半径（メートル）")]
    public float detectionRadius = 3.0f;

    [Tooltip("検知対象のタグ（通常はPlayer）")]
    public string targetTag = "Player";

    [Tooltip("trueの場合、一度ログを記録したら以降は反応しません")]
    public bool logOnce = true;

    private bool hasLogged = false;
    private Transform targetTransform;
    private HorrorEventManager eventManager;

    void Start()
    {
        // イベントマネージャーの取得
        eventManager = FindFirstObjectByType<HorrorEventManager>();
        if (eventManager == null)
        {
            Debug.LogWarning("⚠️ [ProximityLogTrigger] HorrorEventManager not found in the scene.");
        }

        // ターゲット（プレイヤー）の検索
        // 開始時にいない場合もあるため、Updateでも探すガードを入れるか、
        // ここで見つからなければコルーチンで探す等の対策が考えられるが、
        // 今回はシンプルにUpdateでnullチェックを行う方式をとる。
        GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
        if (targetObj != null)
        {
            targetTransform = targetObj.transform;
        }
        else
        {
            // タグで見つからない場合、念のためCharacterControllerなども探してみる（HorrorEventTrigger準拠）
            var controller = FindFirstObjectByType<CharacterController>();
            if (controller != null)
            {
                targetTransform = controller.transform;
            }
        }
    }

    void Update()
    {
        // ログ記録済みなら何もしない
        if (hasLogged && logOnce) return;

        // ターゲットが見つかっていない場合は再検索を試みる
        if (targetTransform == null)
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
            }
            return;
        }

        // 距離チェック
        float distance = Vector3.Distance(transform.position, targetTransform.position);

        if (distance <= detectionRadius)
        {
            RecordLog();
        }
    }

    private void RecordLog()
    {
        if (eventManager != null)
        {
            Debug.Log($"📝 [ProximityLogTrigger] Proximity Detected! Logging Event {eventType}. Dist: {detectionRadius}");
            
            // 直接ログ出力メソッドを呼ぶ（イベント演出はトリガーしない）
            eventManager.LogEvent(eventType);
            
            hasLogged = true;
        }
    }

    // エディタ上で検知範囲を可視化
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.0f, 1.0f, 1.0f, 0.3f); // シアン（半透明）
        Gizmos.DrawSphere(transform.position, detectionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
