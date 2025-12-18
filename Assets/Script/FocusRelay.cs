using UnityEngine;

public class FocusRelay : MonoBehaviour, IFocusable
{
    [Header("転送先")]
    [SerializeField]
    private DoorGapEvent targetEvent; // 転送先のイベントスクリプト

    public void OnFocus()
    {
        if (targetEvent != null)
        {
            Debug.Log($"🔗 [FocusRelay] Relaying focus from {name} to {targetEvent.name}");
            targetEvent.OnFocus();
        }
        else
        {
            Debug.LogWarning("⚠ [FocusRelay] Target Event is not assigned!");
        }
    }
}
