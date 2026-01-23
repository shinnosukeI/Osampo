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
    [Tooltip("自動取得がうまくいかない場合、ここで音の長さ（秒）を指定してください。0なら自動取得を試みます。")]
    [SerializeField] private float overrideSoundDuration = 0f; 

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
        
        // ★ 音声設定の強制適用
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0.0f; // 2Dサウンド（どこにいても聞こえる）
            audioSource.volume = 1.0f;       // 最大音量
        }

        if (useSpeedControl && animator != null)
        {
            animator.speed = 0f;
        }

        eventManager = FindFirstObjectByType<HorrorEventManager>();

        // ★ 判定を残しつつ（Raycast用）、プレイヤーとの衝突だけ無視する（浮き防止）
        Collider myCol = GetComponent<Collider>();
        if (myCol != null)
        {
            // Triggerにはしない (TriggerだとFocusControllerが無視してしまうため)
            myCol.isTrigger = false; 

            // プレイヤーを探して衝突無視を設定
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Collider pCol = player.GetComponent<Collider>();
                if (pCol != null) Physics.IgnoreCollision(pCol, myCol, true);

                CharacterController pChar = player.GetComponent<CharacterController>();
                if (pChar != null) Physics.IgnoreCollision(pChar, myCol, true);
            }
            else
            {
                // タグで見つからない場合、PlayerFocusController経由で探す
                var pCtrl = FindFirstObjectByType<PlayerFocusController>();
                if (pCtrl != null)
                {
                    Collider pCol = pCtrl.GetComponent<Collider>();
                    if (pCol != null) Physics.IgnoreCollision(pCol, myCol, true);

                    CharacterController pChar = pCtrl.GetComponent<CharacterController>();
                    if (pChar != null) Physics.IgnoreCollision(pChar, myCol, true);
                }
            }
        }
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
        this.enabled = true; // ★ リセット時にコンポーネントも有効化（フォーカス可能に）
        hasTriggered = false; // リセット
        
        // もし即座に腐敗させるなら TriggerDecay() だが、
        // 「注視したら」という条件なら、ここはSetActiveだけで良い。
        Debug.Log("🧟 [DecayingCorpseEvent] アクティブ化されました（注視待ち）");
    }

    private void TriggerDecay()
    {
        hasTriggered = true;
        this.enabled = false; // ★ 一度発動したら無効化してフォーカス不可にする
        // Debug.Log("🧟 [DecayingCorpseEvent] 腐敗イベント開始");

        // スクリプトを有効化して再生開始
        if (scriptToEnable != null)
        {
            scriptToEnable.enabled = true;
            // Debug.Log($"🧟 [DecayingCorpseEvent] {scriptToEnable.GetType().Name} を有効化しました");
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

        // 音再生（アニメーション終了に合わせて止める）
        if (decaySound != null && audioSource != null)
        {
            // Debug.Log($"🔊 [DecayingCorpseEvent] Play: {decaySound.name} on {gameObject.name}");
            
            audioSource.clip = decaySound;
            audioSource.loop = false; // ★ ループしないように明示的に設定
            audioSource.Play(); 

            // 停止用コルーチン開始
            StartCoroutine(StopSoundAfterAnimation(audioSource));
        }
        else
        {
             if (decaySound == null) Debug.LogError($"❌ [DecayingCorpseEvent] decaySound is NULL on {gameObject.name}!");
             if (audioSource == null) Debug.LogError($"❌ [DecayingCorpseEvent] AudioSource is NULL on {gameObject.name}!");
        }

        // ScriptToEnableの状態もログに出して、インスタンスの不一致を確認しやすくする
        if (scriptToEnable == null)
        {
             Debug.Log($"ℹ [DecayingCorpseEvent] ScriptToEnable is NULL on {gameObject.name}");
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

    private System.Collections.IEnumerator StopSoundAfterAnimation(AudioSource source)
    {
        // 1フレーム待ってステート遷移を開始させる
        yield return null;

        float length = 0f;

        // 1. オーバーライド設定を確認
        if (overrideSoundDuration > 0)
        {
            length = overrideSoundDuration;
            Debug.Log($"⏳ [DecayingCorpseEvent] Sound Duration Overridden to: {length}s");
        }
        else if (animator != null) // 2. Animatorから取得
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            length = stateInfo.length;

            if (animator.speed > 0)
            {
                 length /= animator.speed;
            }
            // Debug.Log($"⏳ [DecayingCorpseEvent] Auto-Detected Animation Length: {length}s");
        }
        else
        {
            Debug.LogWarning("⚠ [DecayingCorpseEvent] No Animator and No Override Duration. Sound will play until end (default).");
            // クリップの長さを採用する手もある
            if (source.clip != null) length = source.clip.length;
        }

        if (length > 0)
        {
            // 時間分待つ
            yield return new WaitForSeconds(length);
            
            source.Stop();
            Debug.Log("🔇 [DecayingCorpseEvent] Sound Stopped.");
        }
    }
}
