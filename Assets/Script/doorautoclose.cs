using UnityEngine;

public class DoorAutoCloseTrigger : MonoBehaviour
{
    public DoorController targetDoor;   // 閉めたいドアをここに入れる

    private void OnTriggerEnter(Collider other)
    {
        // Player というタグのオブジェクトが入ってきたら閉める
        if (other.CompareTag("Player"))
        {
            if (targetDoor != null)
            {
                targetDoor.CloseDoor();
            }
        }
    }
}