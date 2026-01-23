using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class warptrigger_st2 : MonoBehaviour, IFocusable
{
    [Header("ワープ設定")]
    public Transform player;
    public Vector3 teleportPosition;

    [Header("開くドア（このドアから音を鳴らす）")]
    public Transform targetDoor;

    [Header("ドアを180度へ緩やかに回す設定")]
    public float targetY = 180f;
    public float rotateSmoothSpeed = 2f;
    public float angleTolerance = 0.5f;

    [Header("ドア音")]
    [SerializeField] private AudioClip doorOpenSE;
    [SerializeField] private AudioClip lockedSE;
    [SerializeField] private float seVolume = 1f;

    [Header("ステージ2用ホラーイベント連携")]
    [SerializeField] private HorrorEventManager stage2EventManager;

    public static int teleportCount = 0;

    private bool isMoving = false;

    // ★ 必ず「targetDoor」側の AudioSource だけを使う
    private AudioSource doorAudioSource;

    private void Awake()
    {
        // stage2EventManager 自動取得
        if (stage2EventManager == null)
            stage2EventManager = FindFirstObjectByType<HorrorEventManager>();
    }

    private void Start()
    {
        SetupDoorAudioSource();
    }

    private void SetupDoorAudioSource()
    {
        if (targetDoor == null)
        {
            Debug.LogWarning("[warptrigger_st2] targetDoor が設定されていません。");
            return;
        }

        // ★ AudioSource は「開くドア」に必ず付ける
        doorAudioSource = targetDoor.GetComponent<AudioSource>();
        if (doorAudioSource == null)
            doorAudioSource = targetDoor.gameObject.AddComponent<AudioSource>();

        doorAudioSource.playOnAwake = false;
        doorAudioSource.loop = false;

        // 好みで：ドアから鳴ってる感が欲しいなら 1f（3D）
        // 確実に聞かせたいなら 0f（2D）
        // doorAudioSource.spatialBlend = 1f;

        // 3Dにする場合、聞こえない対策（必要なら）
        // doorAudioSource.minDistance = 1f;
        // doorAudioSource.maxDistance = 12f;
    }

    private void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform)
                TryOpenDoor();
        }
    }

    public void OnFocus()
    {
        TryOpenDoor();
    }

    private void TryOpenDoor()
    {
        // イベント未確認なら進めない
        if (stage2EventManager != null && !stage2EventManager.CanProceedToNextLoop())
        {
            ShowLockedMessage();
            PlaySEFromDoor(lockedSE);
            return;
        }

        TeleportAndOpenDoor();
    }

    public void TeleportAndOpenDoor()
    {
        if (player == null)
        {
            Debug.LogWarning("[warptrigger_st2] player が設定されていません。");
            return;
        }

        // ワープ
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;

        teleportCount++;

        // ステージ2のカウント（元仕様）
        if (stage2EventManager != null)
            stage2EventManager.OnDoorClicked();

        // ワープ後：開くドアを180へ緩やかに回す
        SmoothOpenDoorTo180();
    }

    private void SmoothOpenDoorTo180()
    {
        if (targetDoor == null) return;
        if (isMoving) return;

        // もし Start 前に呼ばれても確実にドア側AudioSourceを準備
        if (doorAudioSource == null) SetupDoorAudioSource();

        // 動く時だけ開く音
        float currentY = targetDoor.eulerAngles.y;
        if (Mathf.Abs(Mathf.DeltaAngle(currentY, targetY)) > angleTolerance)
            PlaySEFromDoor(doorOpenSE);

        StopAllCoroutines();
        StartCoroutine(RotateDoorYSmooth(targetDoor, targetY));
    }

    private IEnumerator RotateDoorYSmooth(Transform door, float toY)
    {
        isMoving = true;

        while (true)
        {
            Vector3 e = door.eulerAngles;
            float currentY = e.y;

            float delta = Mathf.Abs(Mathf.DeltaAngle(currentY, toY));
            if (delta <= angleTolerance)
            {
                door.eulerAngles = new Vector3(e.x, toY, e.z);
                break;
            }

            float newY = Mathf.LerpAngle(currentY, toY, Time.deltaTime * rotateSmoothSpeed);
            door.eulerAngles = new Vector3(e.x, newY, e.z);

            yield return null;
        }

        isMoving = false;
    }

    // ★ 音は「開くドア（targetDoor）」の AudioSource だけで鳴らす
    private void PlaySEFromDoor(AudioClip clip)
    {
        if (clip == null) return;

        if (targetDoor == null)
        {
            Debug.LogWarning("[warptrigger_st2] targetDoor が無いので音を鳴らせません。");
            return;
        }

        if (doorAudioSource == null) SetupDoorAudioSource();
        if (doorAudioSource == null) return;

        doorAudioSource.PlayOneShot(clip, Mathf.Clamp01(seVolume));
    }

    private void ShowLockedMessage()
    {
        var canvas = GameObject.Find("CrosshairCanvas");
        if (canvas != null)
        {
            var textComp = canvas.GetComponentInChildren<Text>();
            if (textComp != null)
                StartCoroutine(ShowMessageCoroutine(textComp, "開かない・・・"));
        }
    }

    private IEnumerator ShowMessageCoroutine(Text textUI, string message)
    {
        string originalText = textUI.text;
        Color originalColor = textUI.color;

        textUI.text = message;
        textUI.color = Color.red;

        yield return new WaitForSeconds(2.0f);

        textUI.text = originalText;
        textUI.color = originalColor;
    }
}
