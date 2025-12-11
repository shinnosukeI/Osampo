using UnityEngine;

public class DecayingCorpseEvent : MonoBehaviour, IFocusable
{
    [Header("スクリプト制御（RotAnimationなど）")]
    [Tooltip("イベント発生時に有効化したいスクリプトがあれば指定")]
    [SerializeField] private MonoBehaviour scriptToEnable;

    [Header("アニメーション設定（Animator使用時）")]
    [SerializeField] private Animator animator;
    [Tooltip("再生を開始するトリガー名（AnimatorのParameter）")]
    [SerializeField] private string triggerName = "StartDecay";
    [Tooltip("もしトリガーではなく、初期速度0→1で制御する場合はこちらをチェック")]
    [SerializeField] private bool useSpeedControl = false;

    [Header("音設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip decaySound;

    [Header("イベント連携")]
    [SerializeField] private int eventID = 12;

    private bool hasTriggered = false;
    private HorrorEventManager eventManager;

    void Start()
    {
        // 指定されたスクリプトがあれば、最初は無効化しておく（Startで勝手に動かないように）
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = false;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // ... (AudioSource setup) ...

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (useSpeedControl && animator != null)
        {
            animator.speed = 0f;
        }

        eventManager = FindFirstObjectByType<HorrorEventManager>();
    }

    // IFocusableの実装
    public void OnFocus()
    {
        if (hasTriggered) return;

        TriggerDecay();
    }

    private void TriggerDecay()
    {
        hasTriggered = true;
        Debug.Log("🧟 [DecayingCorpseEvent] 腐敗イベント開始");

        // スクリプトを有効化して再生開始
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = true;
            Debug.Log($"🧟 [DecayingCorpseEvent] {scriptToEnable.GetType().Name} を有効化しました");
        }

        // Animatorがあれば制御
        if (animator != null)
        {
            if (useSpeedControl)
            {
                animator.speed = 1f;
            }
            else
            {
                animator.SetTrigger(triggerName);
            }
        }

        // 音再生
        if (decaySound != null)
        {
            audioSource.PlayOneShot(decaySound);
        }

        // イベントマネージャーに通知（ログ保存など）
        if (eventManager != null)
        {
            eventManager.TriggerHorrorEvent(eventID);
        }
    }
}
