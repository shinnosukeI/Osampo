using UnityEngine;

public class FocusRelay : MonoBehaviour, IFocusable
{
    [Header("転送先")]
    [SerializeField]
    private DoorGapEvent targetEvent; // 転送先のイベントスクリプト(既存)

    [SerializeField]
    private MirrorGhostEvent mirrorEvent; // 転送先のイベントスクリプト(鏡用)

    public void OnFocus()
    {
        bool handled = false;

        if (targetEvent != null)
        {
            Debug.Log($"🔗 [FocusRelay] Relaying focus from {name} to DoorGapEvent");
            targetEvent.OnFocus();
            handled = true;
        }

        if (mirrorEvent != null)
        {
            Debug.Log($"🔗 [FocusRelay] Relaying focus from {name} to MirrorGhostEvent");
            mirrorEvent.OnFocus();
            handled = true;
        }

        if (!handled)
        {
            Debug.LogWarning("⚠ [FocusRelay] Target Event is not assigned!");
        }
    }
}
