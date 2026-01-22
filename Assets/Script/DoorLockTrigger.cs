using UnityEngine;

// 通り過ぎたらドアが開かなくなる（ロックされる）トリガー
public class DoorLockTrigger : MonoBehaviour
{
    [Header("Target Door")]
    [SerializeField] private DoorController targetDoor;

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private string targetTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;

        if (other.CompareTag(targetTag))
        {
            Debug.Log($"🔒 [DoorLockTrigger] Player passed trigger. Locking door: {(targetDoor != null ? targetDoor.name : "None")}");
            
            if (targetDoor != null)
            {
                targetDoor.LockDoor();
                hasTriggered = true;
            }
            else
            {
                Debug.LogWarning("⚠ [DoorLockTrigger] Target Door is NOT assigned!");
            }
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        Debug.Log("🔄 [DoorLockTrigger] Trigger reset.");
    }
}
