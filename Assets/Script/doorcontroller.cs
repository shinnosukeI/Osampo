using UnityEngine;
using UnityEngine.InputSystem; // ← そのまま使用

public class DoorController : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine autoCloseCoroutine;  // ★ 自動閉鎖用

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, openAngle, 0f));
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

    public void ToggleDoor()
    {
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