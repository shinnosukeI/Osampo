using UnityEngine;
using UnityEngine.InputSystem; // ← 追加

public class DoorController : MonoBehaviour
{
    public float openAngle = 90f;
    public float speed = 2f;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

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
                isOpen = !isOpen;
            }
        }

        // スムーズ回転
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * speed
        );
    }
}