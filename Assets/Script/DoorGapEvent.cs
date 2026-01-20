using UnityEngine;
using System.Collections;

public class DoorGapEvent : MonoBehaviour, IFocusable
{
    [Header("設定")]
    [SerializeField]
    private Transform targetDoor; // 操作するドアのTransform（DoorControllerがついているオブジェクト）
    
    [SerializeField]
    private float gapAngle = 20f; // 隙間の角度（度数）

    [SerializeField]
    private AudioClip womanVoiceSound; // ★ 追加: 女の声

    [SerializeField]
    private AudioSource audioSource; // ★ 音源再生用

    [SerializeField]
    private AudioClip slamSound; // バタンと閉まるときの音

    [SerializeField]
    private GameObject zombieObject; // ゾンビ本体（イベント終了時に消すため）

    [SerializeField]
    // 変数名を変更してInspectorの値をリセット（旧openOnStartがtrueのまま残っている可能性があるため）
    private bool startActive = false; 

    private bool isEventActive = false;
    private DoorController doorController;
    private Quaternion originalRotation; // ドアの初期回転

    private void Awake()
    {
        // ★ Awakeで初期回転（閉じた状態）を確保
        if (targetDoor != null)
        {
            originalRotation = targetDoor.rotation;
        }
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // ★ 開始時にドアを完全にロック（閉じて固定）
        if (targetDoor != null)
        {
             doorController = targetDoor.GetComponent<DoorController>();
             
             // 強制的に閉じた回転にする（少しでも開いていたら閉じる）
             if (originalRotation != Quaternion.identity)
             {
                targetDoor.rotation = originalRotation;
             }
             else
             {
                // originalRotationが取れていない場合、ローカル回転0を信じる
                targetDoor.localRotation = Quaternion.identity;
                originalRotation = targetDoor.rotation;
             }

             if (doorController != null)
             {
                 doorController.CloseDoor(); // 論理状態も閉じる
                 doorController.enabled = false; // 無効化
             }
        }

        // 必要ならイベント開始（指定がある場合のみ）
        if (startActive)
        {
            TriggerEvent();
        }
    }

    // イベント開始（Managerから呼ばれる）
    public void TriggerEvent()
    {
        Debug.Log("🌑 [DoorGapEvent] TriggerEvent called");

        if (targetDoor == null)
        {
            Debug.LogError("❌ [DoorGapEvent] Target Door is not assigned!");
            return;
        }

        // ★ Renderersを使って表示/非表示を切り替え
        SetVisualsActive(false);

        // DoorControllerを取得
        if (doorController == null)
            doorController = targetDoor.GetComponent<DoorController>();

        if (doorController != null)
        {
            // ★変更: イベント待機中はフォーカス可能にする（そうしないとイベント発動できない）
            doorController.enabled = true;
            doorController.FocusOverride = this;
        }

        // Awakeで取れなかった場合の保険
        if (originalRotation == Quaternion.identity)
        {
            originalRotation = targetDoor.rotation;
        }

        // ドアを少し開ける
        targetDoor.rotation = originalRotation * Quaternion.Euler(0, gapAngle, 0);

        isEventActive = true;
    }

    // ... (OnFocus methods unchanged) ...
    // Note: Since I am replacing Start and TriggerEvent, I have to be careful with line numbers.
    // I will target Start and TriggerEvent block.

    // ... 



    // プレイヤーがフォーカスした（PlayerFocusControllerから呼ばれる）
    public void OnFocus()
    {
        if (!isEventActive) return;

        Debug.Log("👁 [DoorGapEvent] Player Focused on DoorGap! Revealing Zombie...");
        isEventActive = false; // 二重発動防止

        // ★ ここでゾンビを表示
        SetVisualsActive(true);

        // コルーチンで「表示 -> 待機 -> ドア閉める」
        StartCoroutine(ShockAndSlamSequence());

        // ログ保存 (52)
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(52);
        }
    }

    private IEnumerator ShockAndSlamSequence()
    {
        // ★ 0. 女の声を再生（表示と同時）
        if (womanVoiceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(womanVoiceSound);
        }

        // ★ 1. ゾンビを見せる時間 (例: 0.5秒)
        yield return new WaitForSeconds(0.5f);

        // ★ 2. バタン音を鳴らす前に、声を止める（ピタッと止める演出）
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // ★ 3. ドアを閉める（ここでバタンと閉めるアニメーション開始）
        // 音は「閉まりきった瞬間」に鳴らすのが自然なので、コルーチン内で鳴らすか、タイミング合わせる
        // ここではSlamDoorCoroutine内で完了時に鳴らすように変更、または直後に鳴らす
        yield return SlamDoorCoroutine();
        
        // （SlamDoorCoroutine内で音を鳴らすよう変更するため、ここは削除）
    }

    private IEnumerator SlamDoorCoroutine()
    {
        // 「勢いよく閉まる」演出：加速して閉じる
        // 時間をさらに短くして「バタン！」感を出す
        float duration = 0.05f; 
        Quaternion startRot = targetDoor.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // t*t で加速させる（EaseIn）
            t = t * t; 
            
            targetDoor.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            yield return null;
        }
        // ループ後に確実に閉める（しっかり閉める）
        targetDoor.rotation = originalRotation;

        // ★ 閉じた瞬間に音を鳴らす（衝撃音）
        if (slamSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slamSound);
        }
        else if (slamSound != null)
        {
            AudioSource.PlayClipAtPoint(slamSound, targetDoor.position);
        }

        // ゾンビを消す
        SetVisualsActive(false);

        // DoorControllerの設定復帰
        if (doorController != null)
        {
            doorController.FocusOverride = null; 
            
            // ★変更: イベント終了後は無効化（ロック）してフォーカス不可にする
            doorController.enabled = false; 
            
            // 状態も「閉」にする
            doorController.CloseDoor(); 
        }
    }

    private void SetVisualsActive(bool isActive)
    {
        Debug.Log($"👁 [DoorGapEvent] SetVisualsActive({isActive}) called.");

        if (zombieObject != null && zombieObject != gameObject)
        {
            Debug.Log($"   -> Toggling zombieObject: {zombieObject.name}");
            zombieObject.SetActive(isActive);
            return;
        }

        // zombieObjectがない、または自分自身の場合、MeshRenderer等を切り替える
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"   -> Toggling {renderers.Length} renderers on {gameObject.name} (ZombieObject is null or self)");
        
        foreach (var r in renderers)
        {
            r.enabled = isActive;
        }
    }
}
