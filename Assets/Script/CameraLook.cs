using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLookInputSystem : MonoBehaviour
{
    public Transform playerBody;                 // Player を割り当て
    public InputActionReference lookAction;      // Controls.Player.Look
    public float mouseSensitivity = 0.1f;        // Mouse deltaは大きいので係数小さめ
    public float gamepadSensitivity = 120f;      // 右スティック用（deg/sec）
    public float minPitch = -80f, maxPitch = 80f;

    float pitch;

    void Start()
    {
        if (!playerBody) playerBody = transform.parent;
        lookAction?.action.Enable();
    }
    void OnDisable() => lookAction?.action.Disable();

    void Update()
    {
        Vector2 look = lookAction.action.ReadValue<Vector2>();

        // 入力デバイスで感度を分ける
        float dx, dy;
        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            dx = look.x * mouseSensitivity;
            dy = look.y * mouseSensitivity;
        }
        else
        {
            dx = look.x * gamepadSensitivity * Time.deltaTime;
            dy = look.y * gamepadSensitivity * Time.deltaTime;
        }

        playerBody.Rotate(Vector3.up * dx);   // yaw
        pitch = Mathf.Clamp(pitch - dy, minPitch, maxPitch);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}