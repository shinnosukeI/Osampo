using System.Collections.Generic;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    [Header("ワープ設定")]
    public Transform player;           // プレイヤー
    public Vector3 teleportPosition;   // ワープ先（XYZ）

    [Header("別のドアを開く設定")]
    public Transform targetDoor;       // 開きたいドア
    public float openAngle = 90f;      // 開く角度
    public float doorOpenSpeed = 3f;   // 開閉の速さ

    public static int teleportCount = 0;

    [HideInInspector] public bool isDoorOpen = false;

    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;
    private bool isMoving = false;

    // ★ 全ての DoorTeleporter を管理するリスト
    private static List<DoorTeleporter> allDoors = new List<DoorTeleporter>();

    private void Awake()
    {
        // シーンにある DoorTeleporter をリストに登録
        if (!allDoors.Contains(this))
            allDoors.Add(this);
    }

    private void OnDestroy()
    {
        allDoors.Remove(this);
    }

    private void Start()
    {
        if (targetDoor != null)
        {
            doorClosedRot = targetDoor.rotation;
            doorOpenRot = Quaternion.Euler(
                targetDoor.eulerAngles.x,
                targetDoor.eulerAngles.y + openAngle,
                targetDoor.eulerAngles.z
            );
        }
    }

    // クリックされたときに呼ぶ用（Raycasterから呼び出し）
    public void TeleportAndOpenDoor()
    {
        if (player == null) return;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;

        // ★ カウンタを +1
        teleportCount++;
        Debug.Log("ワープ回数: " + teleportCount);

        // ★ カウンタが増えたタイミングで全ドア閉じる
        CloseAllDoors();

        // その後、このドアだけ開ける
        if (targetDoor != null && !isDoorOpen)
        {
            StopAllCoroutines();
            StartCoroutine(MoveDoor(doorOpenRot));
            isDoorOpen = true;
        }
    }

    // ★ 全ドアを閉じる static 関数
    public static void CloseAllDoors()
    {
        foreach (var door in allDoors)
        {
            if (door != null)
            {
                door.CloseDoor();
            }
        }
    }

    // 個別に閉める関数（他のスクリプトやトリガーからも呼べる）
    public void CloseDoor()
    {
        if (targetDoor == null || !isDoorOpen) return;

        StopAllCoroutines();                  // 開き途中でも一旦止める
        StartCoroutine(MoveDoor(doorClosedRot));
        isDoorOpen = false;
    }

    private System.Collections.IEnumerator MoveDoor(Quaternion targetRot)
    {
        isMoving = true;

        Quaternion startRot = targetDoor.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * doorOpenSpeed;
            targetDoor.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        targetDoor.rotation = targetRot;
        isMoving = false;
    }
}