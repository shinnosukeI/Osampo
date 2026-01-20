using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorTeleporter : MonoBehaviour, IFocusable
{
    private void OnMouseDown()
    {
        Debug.Log("ドアをクリックした！");
        TeleportAndOpenDoor();
    }

    [Header("ワープ設定")]
    public Transform player;           // プレイヤー
    public Vector3 teleportPosition;   // ワープ先（XYZ）

    [Header("別のドアを開く設定")]
    public Transform targetDoor;       // 開きたいドア
    public float doorOpenSpeed = 3f;   // 開閉の速さ

    [Header("角度の段階開き")]
    public float firstOpenAngle = 90f;     // 1段階目（閉→90）
    public float secondOpenAngle = 180f;   // 2段階目（90→180）
    public float angleTolerance = 5f;      // 角度判定の許容誤差（±）

    [Header("ドア音")]
    [SerializeField] private AudioClip doorOpenSE;   // ★開く音
    [SerializeField] private float seVolume = 1f;    // ★音量

    [Header("ホラーイベント連携")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    public static int teleportCount = 0;

    [HideInInspector] public bool isDoorOpen = false;

    private Quaternion doorClosedRot;
    private bool isMoving = false;

    private AudioSource audioSource;

    // ★ 全ての DoorTeleporter を管理するリスト
    private static List<DoorTeleporter> allDoors = new List<DoorTeleporter>();

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // シーンにある DoorTeleporter をリストに登録
        if (!allDoors.Contains(this))
            allDoors.Add(this);

        // ★ もし Inspector で設定されていなければ自動で探す
        if (eventManager == null)
        {
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
            if (eventManager == null)
            {
                Debug.LogWarning("st1_HorrorEventManager がシーンに見つかりませんでした。");
            }
            else
            {
                Debug.Log($"DoorTeleporter が EventManager を補完: {eventManager.gameObject.name}");
            }
        }
    }

    private void OnDestroy()
    {
        allDoors.Remove(this);
    }

    private void Start()
    {
        if (targetDoor != null)
        {
            // 閉じ位置は「開始時点」で記憶（CloseDoor用）
            doorClosedRot = targetDoor.rotation;
        }
    }

    // クリックされたときに呼ぶ用（Raycasterから呼び出し）
    public void TeleportAndOpenDoor()
    {
        if (player == null) return;
        Debug.Log("ドアクリックされた");

        // ワープ
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;

        // ★ ワープ回数カウント
        teleportCount++;
        Debug.Log("ワープ回数: " + teleportCount);

        // ★ 周期カウント（EventManager 側）
        if (eventManager != null)
        {
            eventManager.OnDoorClicked();   // ← ここで cycleCount++（あなたの実装前提）
        }
        else
        {
            Debug.LogWarning("EventManager が設定されていないため、周期カウントできません。");
        }

        // ★ カウンタが増えたタイミングで全ドア閉じる（このドアは除外）
        CloseAllDoors(targetDoor);

        // ★ このドアだけ「段階開き」
        if (targetDoor != null)
        {
            float currentY = NormalizeAngle(targetDoor.eulerAngles.y);

            float targetY;

            if (IsNearAngle(currentY, firstOpenAngle, angleTolerance))
            {
                targetY = secondOpenAngle; // 90 → 180
            }
            else if (IsNearAngle(currentY, secondOpenAngle, angleTolerance))
            {
                targetY = secondOpenAngle;
            }
            else
            {
                targetY = firstOpenAngle;  // 0(閉) → 90
            }

            Quaternion targetRot = Quaternion.Euler(
                targetDoor.eulerAngles.x,
                targetY,
                targetDoor.eulerAngles.z
            );

            // ★「開く動作」になる時だけ音を鳴らす
            // 0→90, 90→180 の時は開く音。180のままなら鳴らさない。
            bool willOpenMove = !IsNearAngle(currentY, targetY, angleTolerance);
            if (willOpenMove)
            {
                PlayOpenSE();
            }

            StopAllCoroutines();
            StartCoroutine(MoveDoor(targetRot));

            isDoorOpen = !IsNearAngle(targetY, 0f, angleTolerance);
        }
    }

    private void PlayOpenSE()
    {
        if (doorOpenSE == null || audioSource == null) return;
        audioSource.PlayOneShot(doorOpenSE, seVolume);
    }

    // ★ 全ドアを閉じる static 関数
    public static void CloseAllDoors(Transform excludeTargetDoor)
    {
        foreach (var door in allDoors)
        {
            if (door == null) continue;
            if (door.targetDoor == excludeTargetDoor) continue;
            door.CloseDoor();
        }
    }

    // 個別に閉める関数（他のスクリプトやトリガーからも呼べる）
    public void CloseDoor()
    {
        if (targetDoor == null) return;

        StopAllCoroutines();
        StartCoroutine(MoveDoor(doorClosedRot));
        isDoorOpen = false;
    }

    // IFocusableの実装
    public void OnFocus()
    {
        TeleportAndOpenDoor();
    }

    private IEnumerator MoveDoor(Quaternion targetRot)
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

    // ------------------------------
    // 補助関数
    // ------------------------------

    // 角度を 0〜360 に正規化
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    // 角度が target±tol に近いか（0/360またぎにも対応）
    private bool IsNearAngle(float angle, float target, float tol)
    {
        angle = NormalizeAngle(angle);
        target = NormalizeAngle(target);

        float diff = Mathf.Abs(angle - target);
        diff = Mathf.Min(diff, 360f - diff);
        return diff <= tol;
    }
}