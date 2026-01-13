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
        // ヒエラルキー上でゾンビが最初からいる場合、イベント発生まで非表示にしておく？
        // ユーザーの指示次第だが、通常はTriggerまでは非表示が安全。
        // ただし、もしユーザーがすでに非表示設定しているならそのままでOK。
        
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

        // ゾンビを表示
        if (zombieObject != null)
        {
            zombieObject.SetActive(true);
        }
        else
        {
            // 設定がなければ自分がゾンビとみなして表示
            this.gameObject.SetActive(true);
        }

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
        // ドアの軸に合わせて回転させる（Y軸回転と仮定）
        targetDoor.rotation = originalRotation * Quaternion.Euler(0, gapAngle, 0);

        isEventActive = true;
    }

    // プレイヤーがフォーカスした（PlayerFocusControllerから呼ばれる）
    public void OnFocus()
    {
        if (!isEventActive) return;

        Debug.Log("👁 [DoorGapEvent] Player Focused on Zombie! Slamming door.");
        isEventActive = false;

        // 音を鳴らす
        if (slamSound != null)
        {
            AudioSource.PlayClipAtPoint(slamSound, targetDoor.position);
        }

        // ドアを即座に閉める（またはアニメーションさせる）
        // "急にゾンビがドアを閉める" なので即座 または 高速回転
        StartCoroutine(SlamDoorCoroutine());

        // ログ保存 (52)
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(52);
        }
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
        if (zombieObject != null)
        {
            zombieObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(false);
        }

        // DoorControllerを有効に戻す（プレイヤーが後で通れるように）
        if (doorController != null)
        {
            doorController.FocusOverride = null; // 上書き解除
            doorController.enabled = true;
            // DoorControllerの状態整合性を取るためにCloseを呼んでおくのが無難
            doorController.CloseDoor(); 
        }
    }
}
