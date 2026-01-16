using UnityEngine;

public class GlassAreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<GlassAreaFootstepPlayer>();
        if (player != null) player.SetInArea(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponentInParent<GlassAreaFootstepPlayer>();
        if (player != null) player.SetInArea(false);
    }
}