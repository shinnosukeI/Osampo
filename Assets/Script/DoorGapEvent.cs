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
    private bool openOnStart = true; // ゲーム開始時にドアを開けて待機するか

    private bool isEventActive = false;
    private DoorController doorController;
    private Quaternion originalRotation; // ドアの初期回転

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

        // 開始時にすでにイベント状態にする場合
        if (openOnStart)
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

        // ★ 変更点: Renderersを使って表示/非表示を切り替える
        SetVisualsActive(false);

        // DoorControllerを取得して無効化
        doorController = targetDoor.GetComponent<DoorController>();
        if (doorController != null)
        {
            doorController.enabled = false;
            // ドアを見てもこのイベントが発動するようにする
            doorController.FocusOverride = this;
        }

        // 現在の回転を保存（閉まっている前提）
        originalRotation = targetDoor.rotation;

        // ドアを少し開ける
        targetDoor.rotation = originalRotation * Quaternion.Euler(0, gapAngle, 0);

        isEventActive = true;
    }

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

        // ★ 2. バタン音を鳴らす（ドア閉め開始時）
        if (slamSound != null && audioSource != null)
        {
            // PlayOneShotを使えば声と被って再生される（声が長ければミックスされる）
            // もし「声はここで止めたい」なら audioSource.Stop() を呼ぶが、
            // 悲鳴とバタン音が重なる方が自然な場合が多いのでStopなしにする。
            audioSource.PlayOneShot(slamSound);
        }
        else if (slamSound != null)
        {
             AudioSource.PlayClipAtPoint(slamSound, targetDoor.position);
        }

        // ★ 3. ドアを閉める
        yield return SlamDoorCoroutine();
    }

    private IEnumerator SlamDoorCoroutine()
    {
        // 少しだけ時間をかけて閉める演出（0.1秒など）
        float duration = 0.1f;
        Quaternion startRot = targetDoor.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            targetDoor.rotation = Quaternion.Lerp(startRot, originalRotation, t);
            yield return null;
        }
        targetDoor.rotation = originalRotation;

        // ゾンビを消す
        SetVisualsActive(false);

        // DoorControllerを有効に戻す（プレイヤーが後で通れるように）
        if (doorController != null)
        {
            doorController.FocusOverride = null; // 上書き解除
            doorController.enabled = true;
            // DoorControllerの状態整合性を取るためにCloseを呼んでおくのが無難
            doorController.CloseDoor(); 
        }
    }

    private void SetVisualsActive(bool isActive)
    {
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
