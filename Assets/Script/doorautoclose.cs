using UnityEngine;

public class DoorAutoCloseTrigger : MonoBehaviour
{
    [Header("閉じたいドア（ドア本体を指定）")]
    public GameObject targetDoor;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (targetDoor == null) return;

        // ドアについている CloseDoor() を呼ぶ
        targetDoor.SendMessage(
            "CloseDoor",
            SendMessageOptions.DontRequireReceiver
        );
    }
}