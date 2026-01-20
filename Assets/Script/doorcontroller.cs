using UnityEngine;

public class DoorController : MonoBehaviour, IFocusable
{
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Manual Control")]
    [SerializeField] private bool disableManualControl = false; // ★チェックで手動無効

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine autoCloseCoroutine;

    public bool HasBeenInteracted { get; private set; } = false;

    public void ResetInteraction() => HasBeenInteracted = false;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Update()
    {
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * speed
        );
    }

    public IFocusable FocusOverride { get; set; }

    public void OnFocus()
    {
        if (FocusOverride != null) { FocusOverride.OnFocus(); return; }
        if (!enabled) return;

        // ★手動操作禁止なら反応しない
        if (disableManualControl) return;

        ToggleDoor();
    }

    public void ToggleDoor()
    {
        // ★念のためここでもブロック（外部からToggle呼ばれても無効）
        if (disableManualControl) return;

        HasBeenInteracted = true;
        isOpen = !isOpen;

        StopAutoCloseIfNeeded();

        if (isOpen)
            autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void OpenDoor()
    {
        HasBeenInteracted = true;
        if (isOpen) return;
        isOpen = true;

        StopAutoCloseIfNeeded();
        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void CloseDoor()
    {
        HasBeenInteracted = true;
        if (!isOpen) return;
        isOpen = false;

        StopAutoCloseIfNeeded();
    }

    private void StopAutoCloseIfNeeded()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private System.Collections.IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3f);
        isOpen = false;
        autoCloseCoroutine = null;
    }

    public Quaternion ClosedRotation => closedRotation;
    public Quaternion OpenRotation => openRotation;
    public bool IsOpen => isOpen;

    // （任意）外部から切り替えたい時用
    public bool DisableManualControl
    {
        get => disableManualControl;
        set => disableManualControl = value;
    }
}