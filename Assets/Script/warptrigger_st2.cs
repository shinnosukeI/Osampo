using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // ★ 新Input System用

[RequireComponent(typeof(AudioSource))]
public class warptrigger_st2 : MonoBehaviour
{
    [Header("ワープ設定")]
    public Transform player;           // プレイヤー
    public Vector3 teleportPosition;   // ワープ先（ワールド座標）

    [Header("別のドアを開く設定")]
    public Transform targetDoor;       // 開きたいドア
    public float openAngle = 90f;      // 開く角度
    public float doorOpenSpeed = 3f;   // 開閉の速さ

    [Header("ドア音")]
    [SerializeField] private AudioClip doorOpenSE;   // ★開く音
    [SerializeField] private float seVolume = 1f;    // ★音量

    [Header("ステージ2用ホラーイベント連携")]
    [SerializeField] private HorrorEventManager stage2EventManager; // ★ 型修正: 本来のマネージャを参照

    public static int teleportCount = 0;

    [HideInInspector] public bool isDoorOpen = false;

    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;
    private bool isMoving = false;

    private AudioSource audioSource;

    // ★ 全ての warptrigger_st2 を管理するリスト
    private static List<warptrigger_st2> allStage2Doors = new List<warptrigger_st2>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // シーンにある warptrigger_st2 をリストに登録
        if (!allStage2Doors.Contains(this))
            allStage2Doors.Add(this);
    }

    private void Start()
    {
        // ★ 自動取得ロジック追加: 設定し忘れ防止
        if (stage2EventManager == null)
        {
            stage2EventManager = FindFirstObjectByType<HorrorEventManager>();
            if (stage2EventManager != null)
            {
                Debug.Log("[warptrigger_st2] HorrorEventManager was automatically found and assigned.");
            }
            else
            {
                Debug.LogError("[warptrigger_st2] Critical: HorrorEventManager not found in scene!");
            }
        }

        if (targetDoor != null)
        {
            doorClosedRot = targetDoor.rotation;
            doorOpenRot = Quaternion.Euler(targetDoor.eulerAngles + new Vector3(0f, openAngle, 0f));
        }
        else
        {
            Debug.LogWarning("[warptrigger_st2] targetDoor が設定されていません。");
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
                    OnDoorClicked();
                }
            }
        }
    }

    // ★ クリック時の一連の処理
    private void OnDoorClicked()
    {
        // ★ 進行判定: イベントを見ていない(ログがない)場合は進めない
        if (stage2EventManager != null)
        {
            if (!stage2EventManager.CanProceedToNextLoop())
            {
                Debug.Log("🔒 [warptrigger_st2] イベント未確認のためドアを開けません。");
                // ここで「鍵がかかっている」音などを鳴らすと親切
                return;
            }
        }

        TeleportPlayer();
        OpenDoor();
        teleportCount++;

        if (stage2EventManager != null)
        {
            stage2EventManager.OnDoorClicked();
        }
        else
        {
            Debug.LogWarning("[warptrigger_st2] stage2EventManager がアサインされていません。");
        }
    }

    private void TeleportPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[warptrigger_st2] player が設定されていません。");
            return;
        }

        // CharacterController が付いている場合は一旦無効化してから動かす
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;
    }

    private void OpenDoor()
    {
        if (targetDoor == null)
        {
            Debug.LogWarning("[warptrigger_st2] targetDoor が設定されていません。");
            return;
        }

        if (isMoving) return;

        // ★ 既に開いてるなら二重再生しない
        if (isDoorOpen) return;

        isDoorOpen = true;

        // ★ 開き始めに音を鳴らす
        PlayOpenSE();

        StartCoroutine(RotateDoor(targetDoor, doorClosedRot, doorOpenRot));
    }

    private void PlayOpenSE()
    {
        if (doorOpenSE == null || audioSource == null) return;
        audioSource.PlayOneShot(doorOpenSE, seVolume);
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
    }
}
