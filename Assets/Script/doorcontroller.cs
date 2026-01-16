using UnityEngine;
using UnityEngine.InputSystem;

public class DoorController : MonoBehaviour, IFocusable
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine autoCloseCoroutine;

    // ★一度でもインタラクトされたかを記録
    public bool HasBeenInteracted { get; private set; } = false;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Update()
    {


        // スムーズ回転
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
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        if (isOpen)
            autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void OpenDoor()
    {
        isOpen = true;

        if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void CloseDoor()
    {
        isOpen = false;

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
}
