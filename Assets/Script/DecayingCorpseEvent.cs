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

    // イベントマネージャーから呼び出す用（強制実行、あるいは準備など）
    public void ActivateEvent()
    {
        // 外部から強制的に起こす場合
        // 今回の仕様では「見たら起きる」なので、ここでは
        // 「オブジェクトを表示する」または「リセットする」などの処理が必要かもしれないが
        // シンプルに TriggerDecay を呼ぶか、あるいは何もしない（見るのを待つ）か。
        
        // ユーザーの意図としては「このイベント発生タイミングになったよ」という合図なので、
        // もしオブジェクトが非表示なら表示する、などの初期化を行うのが適切。
        this.gameObject.SetActive(true);
        hasTriggered = false; // リセット
        
        // もし即座に腐敗させるなら TriggerDecay() だが、
        // 「注視したら」という条件なら、ここはSetActiveだけで良い。
        Debug.Log("🧟 [DecayingCorpseEvent] アクティブ化されました（注視待ち）");
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
        // ここで無限ループしないように注意（Managerから呼ばれた場合）
        // Manager -> ActivateEvent -> (Wait) -> OnFocus -> TriggerDecay -> Manager.TriggerHorrorEvent -> Log
        if (eventManager != null)
        {
           // eventManager.TriggerHorrorEvent(eventID); // ログ保存したいだけならOKだが、再帰呼び出しに注意
           eventManager.LogEvent(eventID);
        }
    }
}
