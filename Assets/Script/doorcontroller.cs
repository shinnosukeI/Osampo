using UnityEngine;
using UnityEngine.InputSystem; // ← そのまま使用

public class DoorController : MonoBehaviour, IFocusable
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine autoCloseCoroutine;  // ★ 自動閉鎖用

    [Header("Event System Link")]
    [SerializeField] private HorrorEventManager eventManager;

    [Header("Restriction Settings")]
    [Tooltip("Check this if this door should be locked until the horror event is logged.")]
    [SerializeField] private bool requiresEventCompletion = false;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));

        if (eventManager == null)
        {
            eventManager = FindFirstObjectByType<HorrorEventManager>();
        }

        if (eventManager != null)
        {
            Debug.Log($"🚪 [DoorController] Linked to EventManager: {eventManager.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ [DoorController] HorrorEventManager NOT found. Restrictions may not work!");
        }

        Debug.Log($"🚪 [DoorController] Initialized on {gameObject.name}. RequiresCompletion: {requiresEventCompletion}");
    }

    void Update()
    {
        // 新Input Systemでのクリック検知
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                ToggleDoor();
            }
        }

        // スムーズ回転
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * speed
        );
    }

    // イベントなどでフォーカス先を上書きしたい場合の設定
    public IFocusable FocusOverride { get; set; }

    // IFocusableの実装 (右クリックなどで呼ばれる)
    public void OnFocus()
    {
        // 上書き設定があればそちらを呼ぶ
        if (FocusOverride != null)
        {
            FocusOverride.OnFocus();
            return;
        }

        // イベントなどで無効化されている場合は反応しない
        if (!this.enabled) return;

        ToggleDoor();
    }

    public void ToggleDoor()
    {
        Debug.Log($"🖱 [DoorController] ToggleDoor called on {gameObject.name}. IsOpen: {isOpen}, Requires: {requiresEventCompletion}");

        // ★ 開けようとするときに制限チェック
        if (!isOpen)
        {
            // フラグが立っている場合のみチェック
            if (requiresEventCompletion)
            {
                if (eventManager == null)
                {
                    Debug.LogError("🔒 [DoorController] Locked (Safety). EventManager missing.");
                    return;
                }

                bool canProceed = eventManager.CanProceedToNextLoop();
                Debug.Log($"🧐 [DoorController] Checking Permission... Result: {canProceed}");

                if (!canProceed)
                {
                    Debug.Log("🔒 [DoorController] Locked. You must witness the horror event first.");
                    return;
                }
            }
        }

        isOpen = !isOpen;

        if (isOpen)
        {
            // ★すでに自動閉じカウントがあれば止める
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            // ★3秒後に閉じる
            autoCloseCoroutine = StartCoroutine(AutoClose());
        }
        else
        {
            // ★閉じた瞬間は自動閉じ処理を止める
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);
        }
    }

    public void OpenDoor()
    {
        isOpen = true;

        // ★OpenDoor() で開いたときも自動閉じ開始
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void CloseDoor()
    {
        isOpen = false;

        // ★閉じたときは自動閉鎖 coroutine を停止
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);
    }

    // ★ 3秒後に自動で閉まる処理
    private System.Collections.IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3f);
        isOpen = false;
        autoCloseCoroutine = null;
    }
}