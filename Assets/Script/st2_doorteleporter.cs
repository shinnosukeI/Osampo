using UnityEngine;
using UnityEngine.InputSystem;

public class St2DoorTeleporter : MonoBehaviour
{
    [Header("ワープ設定")]
    public Transform player;
    public Vector3 teleportPosition;

    [Header("ドア設定")]
    public float openAngle = 90f;
    public float speed = 2f;
    public Transform doorPivot;   // 回転させるPivot

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    [Header("ホラーイベント連携 (Stage2)")]
    [SerializeField] private HorrorEventManager eventManager;

    void Start()
    {
        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.rotation;
        openRotation = Quaternion.Euler(
            doorPivot.eulerAngles.x,
            doorPivot.eulerAngles.y + openAngle,
            doorPivot.eulerAngles.z
        );
    }

    void Update()
    {
        // 新 Input System でクリック検知
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    OnDoorClicked();
                }
            }
        }

        // ドアのスムーズ開閉
        doorPivot.rotation = Quaternion.Slerp(
            doorPivot.rotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * speed
        );
    }

    private void OnDoorClicked()
    {
        Debug.Log("Stage2: ドアをクリック → ワープ＆開閉");

        WarpPlayer();
        TriggerCycleCount();
        ToggleDoor();
    }

    // -------------------------
    // ★ ワープ
    // -------------------------
    private void WarpPlayer()
    {
        if (player == null)
        {
            Debug.LogError("Player が設定されていません");
            return;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = teleportPosition;

        if (cc != null) cc.enabled = true;
    }

    // -------------------------
    // ★ 周回カウント
    // -------------------------
    private void TriggerCycleCount()
    {
        if (eventManager != null)
        {
            eventManager.OnDoorClicked();
        }
    }

    // -------------------------
    // ★ ドア開閉（自動閉じ無し）
    // -------------------------
    private void ToggleDoor()
    {
        isOpen = !isOpen;
    }

    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }
}