using UnityEngine;

public class WalkingPersonEvent : MonoBehaviour
{
    [Header("設定")]
    [SerializeField]
    private Animator targetAnimator; // 動かす対象のAnimator

    [SerializeField]
    private string animationTriggerName = "Walk"; // 再生するアニメーションのトリガー名

    [SerializeField]
    private Transform startPosition; // 開始位置（指定があればここに移動）

    [SerializeField]
    private GameObject targetObject; // 対象のオブジェクト（非表示から表示にする場合など）

    private bool hasTriggered = false;

    // ゲーム開始時に非表示にする
    private void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else if (targetAnimator != null)
        {
            targetAnimator.gameObject.SetActive(false);
        }
    }

    public void TriggerEvent()
    {
        Debug.Log("🌑 [WalkingPersonEvent] TriggerEvent called");

        if (hasTriggered)
        {
            Debug.Log("ℹ [WalkingPersonEvent] Already triggered");
            return;
        }

        if (targetAnimator == null && targetObject == null)
        {
            Debug.LogError("❌ [WalkingPersonEvent] Target is not assigned!");
            return;
        }

        hasTriggered = true;

        // オブジェクトの有効化
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
        else if (targetAnimator != null)
        {
            targetAnimator.gameObject.SetActive(true);
        }

        // 開始位置への移動
        if (startPosition != null)
        {
            if (targetObject != null)
            {
                targetObject.transform.position = startPosition.position;
                targetObject.transform.rotation = startPosition.rotation;
            }
            else if (targetAnimator != null)
            {
                targetAnimator.transform.position = startPosition.position;
                targetAnimator.transform.rotation = startPosition.rotation;
            }
        }

        // アニメーション再生
        if (targetAnimator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            targetAnimator.SetTrigger(animationTriggerName);
            Debug.Log($"🌑 [WalkingPersonEvent] Animation Triggered: {animationTriggerName}");
        }

        // イベントID 51 をログに記録
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(51);
        }
    }
}
