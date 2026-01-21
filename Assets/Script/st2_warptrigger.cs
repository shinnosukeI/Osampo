using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // ★ 新Input System用

public class st2_warptrigger : MonoBehaviour, IFocusable
{
    [Header("ワープ設定")]
    public Transform player;           // プレイヤー
    public Vector3 teleportPosition;   // ワープ先（ワールド座標）

    [Header("別のドアを開く設定")]
    public Transform targetDoor;       // 開きたいドア
    public float doorOpenSpeed = 3f;   // 開閉の速さ

    [Header("ドア角度（Y固定）")]
    public float closedY = 90f;        // 閉じ状態のY
    public float openedY = 180f;       // 開き状態のY

    [Header("ドアSE")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;

    [Header("ステージ2用ホラーイベント連携")]
    [SerializeField] private HorrorEventManager stage2EventManager;

    public static int teleportCount = 0;

    [HideInInspector] public bool isDoorOpen = false;

    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;
    private bool isMoving = false;

    // ★ 全ての st2_warptrigger を管理するリスト
    private static List<st2_warptrigger> allStage2Doors = new List<st2_warptrigger>();

    private void Awake()
    {
        if (!allStage2Doors.Contains(this))
            allStage2Doors.Add(this);
    }

    private void Start()
    {
        // ★ 自動取得（イベントマネージャ）
        if (stage2EventManager == null)
        {
            stage2EventManager = FindFirstObjectByType<HorrorEventManager>();
            if (stage2EventManager != null)
                Debug.Log("[st2_warptrigger] HorrorEventManager was automatically found and assigned.");
            else
                Debug.LogError("[st2_warptrigger] Critical: HorrorEventManager not found in scene!");
        }

        // ★ AudioSource 自動取得（なければこのオブジェクトから取る）
        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
            if (doorAudioSource == null)
            {
                doorAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (targetDoor != null)
        {
            // ★ 絶対角（Y=90/180）で固定
            Vector3 e = targetDoor.eulerAngles;
            doorClosedRot = Quaternion.Euler(e.x, closedY, e.z);
            doorOpenRot   = Quaternion.Euler(e.x, openedY, e.z);

            // もし初期状態を「閉じ(90)」に揃えたいなら↓をON
            // targetDoor.rotation = doorClosedRot;
        }
        else
        {
            Debug.LogWarning("[Stage2][WarpTrigger] targetDoor が設定されていません。");
        }
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    OnDoorClicked();
                }
            }
        }
    }

    // IFocusableの実装
    public void OnFocus()
    {
        OnDoorClicked();
    }

    private void OnDoorClicked()
    {
        TeleportPlayer();
        OpenDoor();
        teleportCount++;

        if (stage2EventManager != null)
            stage2EventManager.OnDoorClicked();
        else
            Debug.LogWarning("[Stage2][WarpTrigger] stage2EventManager がアサインされていません。");
    }

    private void TeleportPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[Stage2][WarpTrigger] player が設定されていません。");
            return;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;
    }

    private void OpenDoor()
    {
        if (targetDoor == null)
        {
            Debug.LogWarning("[Stage2][WarpTrigger] targetDoor が設定されていません。");
            return;
        }

        if (isMoving) return;

        isDoorOpen = true;
        StartCoroutine(RotateDoor(targetDoor, doorClosedRot, doorOpenRot, true));
    }

    // ★ 閉じる処理（必要なときに呼ぶ）
    private void CloseDoor()
    {
        if (targetDoor == null) return;
        if (isMoving) return;

        isDoorOpen = false;
        StartCoroutine(RotateDoor(targetDoor, doorOpenRot, doorClosedRot, false));
    }

    private System.Collections.IEnumerator RotateDoor(Transform door, Quaternion from, Quaternion to, bool opening)
    {
        isMoving = true;

        // ★ 回転開始時にSE
        PlayDoorSE(opening);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;
            door.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        door.rotation = to; // 誤差止め
        isMoving = false;
    }

    private void PlayDoorSE(bool opening)
    {
        if (doorAudioSource == null) return;

        AudioClip clip = opening ? openSE : closeSE;
        if (clip == null) return;

        doorAudioSource.PlayOneShot(clip);
    }

    // ★ 開いているドアを全部閉じたいとき用
    public void CloseAllOpenedDoors()
    {
        foreach (var door in allStage2Doors)
        {
            if (door == null) continue;

            if (door.isDoorOpen && door.targetDoor != null)
            {
                // ★ 閉まる音も鳴る
                door.StartCoroutine(door.RotateDoor(door.targetDoor, door.doorOpenRot, door.doorClosedRot, false));
                door.isDoorOpen = false;
            }

            // ★ ドアのインタラクト状態もリセットする
            if (door.targetDoor != null)
            {
                var doorCtrl = door.targetDoor.GetComponent<DoorController>();
                if (doorCtrl == null) doorCtrl = door.targetDoor.GetComponentInParent<DoorController>();

                if (doorCtrl != null)
                {
                    doorCtrl.ResetInteraction();
                }
            }
        }
    }
}