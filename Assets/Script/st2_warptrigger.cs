using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // ★ 新Input System用

public class st2_warptrigger : MonoBehaviour
{
    [Header("ワープ設定")]
    public Transform player;           // プレイヤー
    public Vector3 teleportPosition;   // ワープ先（ワールド座標）

    [Header("別のドアを開く設定")]
    public Transform targetDoor;       // 開きたいドア
    public float openAngle = 90f;      // 開く角度
    public float doorOpenSpeed = 3f;   // 開閉の速さ

    [Header("ステージ2用ホラーイベント連携")]
    [SerializeField] private st2_HorrorEventManager stage2EventManager;

    public static int teleportCount = 0;

    [HideInInspector] public bool isDoorOpen = false;

    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;
    private bool isMoving = false;

    // ★ 全ての st2_warptrigger を管理するリスト
    private static List<st2_warptrigger> allStage2Doors = new List<st2_warptrigger>();

    private void Awake()
    {
        // シーンにある st2_warptrigger をリストに登録
        if (!allStage2Doors.Contains(this))
            allStage2Doors.Add(this);
    }

    private void Start()
    {
        if (targetDoor != null)
        {
            doorClosedRot = targetDoor.rotation;
            doorOpenRot = Quaternion.Euler(targetDoor.eulerAngles + new Vector3(0f, openAngle, 0f));
        }
        else
        {
            Debug.LogWarning("[Stage2][WarpTrigger] targetDoor が設定されていません。");
        }
    }

    private void Update()
    {
        // マウスがない環境なら何もしない
        if (Mouse.current == null) return;

        // 左クリックが押された瞬間
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 画面上のマウス位置からレイを飛ばす
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // このドア自身がクリックされたか？
                if (hit.transform == transform)
                {
                    Debug.Log("[Stage2][WarpTrigger] ドアがクリックされました。");
                    OnDoorClicked();
                }
            }
        }
    }

    // ★ クリック時の一連の処理
    private void OnDoorClicked()
    {
        TeleportPlayer();
        OpenDoor();
        teleportCount++;

        if (stage2EventManager != null)
        {
            stage2EventManager.OnStage2DoorPassed();       // 周回+1
            stage2EventManager.TryTriggerStage2CycleEvent(); // 必要ならイベント発火
        }
        else
        {
            Debug.LogWarning("[Stage2][WarpTrigger] stage2EventManager がアサインされていません。");
        }
    }

    private void TeleportPlayer()
{
    if (player == null)
    {
        Debug.LogWarning("[Stage2][WarpTrigger] player が設定されていません。");
        return;
    }

    Vector3 before = player.position;

    // CharacterController が付いている場合は一旦無効化してから動かす
    var cc = player.GetComponent<CharacterController>();
    if (cc != null)
    {
        cc.enabled = false;
    }

    player.position = teleportPosition;

    if (cc != null)
    {
        cc.enabled = true;
    }

    Debug.Log($"[Stage2][WarpTrigger] プレイヤーを {before} → {player.position} にワープしました。");
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
        StartCoroutine(RotateDoor(targetDoor, doorClosedRot, doorOpenRot));
    }

    private System.Collections.IEnumerator RotateDoor(Transform door, Quaternion from, Quaternion to)
    {
        isMoving = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;
            door.rotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        isMoving = false;
    }

    // ★ 開いているドアを全部閉じたいとき用（必要なら他スクリプトから呼ぶ）
    public void CloseAllOpenedDoors()
    {
        foreach (var door in allStage2Doors)
        {
            if (door != null && door.isDoorOpen && door.targetDoor != null)
            {
                door.StartCoroutine(door.RotateDoor(door.targetDoor, door.doorOpenRot, door.doorClosedRot));
                door.isDoorOpen = false;
            }
        }

        Debug.Log("[Stage2][WarpTrigger] 開いているドアをすべて閉じました。");
    }
}