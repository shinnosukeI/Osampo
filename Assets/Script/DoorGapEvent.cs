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

    private Rigidbody doorRigidbody; 
    private HorrorEventTrigger myTrigger; 
    
    private bool hasTriggered = false; // ★ 追加: 二重発動防止用フラグ

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

        // ★Triggerコンポーネント取得
        myTrigger = GetComponent<HorrorEventTrigger>();
        if (myTrigger == null && targetDoor != null)
        {
             // 親や子にあるかもしれないので探す
             myTrigger = targetDoor.GetComponent<HorrorEventTrigger>();
             if (myTrigger == null) myTrigger = GetComponentInChildren<HorrorEventTrigger>();
        }

        // ★ 開始時にドアを完全にロック（閉じて固定）
        if (targetDoor != null)
        {
             doorController = targetDoor.GetComponent<DoorController>();
             doorRigidbody = targetDoor.GetComponent<Rigidbody>(); // ★RB取得
             
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

             // ★物理挙動もロック（プレイヤーが押して開かないようにする）
             if (doorRigidbody != null)
             {
                 doorRigidbody.isKinematic = true;
             }
        }

        // 必要ならイベント開始（指定がある場合のみ）
        if (startActive)
        {
            TriggerEvent();
        }
        else
        {
            // イベント開始しないなら、自分自身(DoorGapEvent)も無効化してフォーカス対象外にする
            this.enabled = false;
        }
    }

    // イベント開始（Managerから呼ばれる）
    public void TriggerEvent()
    {
        Debug.Log("🌑 [DoorGapEvent] TriggerEvent called");
        
        // ★ 自分自身を有効化
        this.enabled = true;

        // ★ トラブル防止のため、トリガーコンポーネントがあれば最初は有効化し、
        //    重複発動を防ぐためにResetする（またはTriggerOnceに任せる）が、
        //    ここでは「イベント開始時は有効」にしておく
        if (myTrigger != null)
        {
            myTrigger.enabled = true;
        }

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

        if (doorRigidbody == null)
            doorRigidbody = targetDoor.GetComponent<Rigidbody>();

        if (doorController != null)
        {
            // ★変更: イベント待機中はフォーカス可能にする（そうしないとイベント発動できない）
            doorController.enabled = true;
            doorController.SkipUpdate = true; // ★追加: DoorControllerの自動回転を停止（隙間を維持するため）
            doorController.FocusOverride = this;
        }

        // ★イベント中は物理挙動をKinematicにして固定（変な動きを防ぐ）
        // ただしSlamアニメーションはTransform直接操作なのでKinematicでOK
        if (doorRigidbody != null)
        {
            doorRigidbody.isKinematic = true; 
        }

        // Awakeで取れなかった場合の保険
        if (originalRotation == Quaternion.identity)
        {
            originalRotation = targetDoor.rotation;
        }

        // ドアを少し開ける
        targetDoor.rotation = originalRotation * Quaternion.Euler(0, gapAngle, 0);

        isEventActive = true;
        hasTriggered = false; // ★ リセット
    }

    // プレイヤーがフォーカスした（PlayerFocusControllerから呼ばれる）
    public void OnFocus()
    {
        if (!isEventActive) return;
        if (hasTriggered) return; // ★ 既に発動済みなら無視

        Debug.Log("👁 [DoorGapEvent] Player Focused on DoorGap! Revealing Zombie...");
        isEventActive = false; 
        hasTriggered = true; // ★ 即座にフラグを立てる

        // ★ 即座に無効化して、二度押しや連打を完全に防ぐ
        if (doorController != null)
        {
            doorController.FocusOverride = null;
            doorController.enabled = false;
        }
        this.enabled = false; // 自分自身も無効化（PlayerFocusControllerから無視される）

        // ★ フォーカスされた時点で、接近トリガーも無効化して再発動（再オープン）を即座に防ぐ
        if (myTrigger != null)
        {
            myTrigger.enabled = false;
        }

        // ★ ここでゾンビを表示
        SetVisualsActive(true);

        // コルーチンで「表示 -> 待機 -> ドア閉める」
        // コンポーネントが無効でもGameObjectがActiveならコルーチンは走る
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
        yield return SlamDoorCoroutine();
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

         // もしDoorControllerがあれば、そちらのClosedRotationを使うのが確実
        if (doorController != null)
        {
            targetDoor.localRotation = doorController.ClosedRotation; // ★ 確実な閉鎖位置
        }
        else
        {
             targetDoor.rotation = originalRotation;
        }

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

        // DoorControllerの設定復帰（念のためCloseだけ確認）
        if (doorController != null)
        {
             // 既に無効化されているはずだが、状態整合性のためCloseDoorは呼んでおく
            doorController.CloseDoor(); 
        }

        // ★ 物理挙動も完全に固定（念押し）
        if (doorRigidbody != null)
        {
            doorRigidbody.isKinematic = true;
        }
        
        // ★ 自分自身も無効化して、PlayerFocusControllerに見つからないようにする
        this.enabled = false;
    }

    private void SetVisualsActive(bool isActive)
    {
        // Debug.Log($"👁 [DoorGapEvent] SetVisualsActive({isActive}) called.");
        if (zombieObject != null && zombieObject != gameObject)
        {
            zombieObject.SetActive(isActive);
            return;
        }

        // zombieObjectがない、または自分自身の場合、MeshRenderer等を切り替える
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            r.enabled = isActive;
        }
    }
}
