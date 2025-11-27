using UnityEngine;
using UnityEngine.InputSystem;   
public class ClickDoorRaycaster : MonoBehaviour
{
    public float maxDistance = 5f; // クリックが届く距離
void Update()
{
    // マウスが存在するかチェック
    if (Mouse.current == null) return;

    // 左クリックが押された瞬間
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            DoorTeleporter door = hit.collider.GetComponent<DoorTeleporter>();
            if (door != null)
            {
                door.TeleportAndOpenDoor();
            }
        }
    }
}

}