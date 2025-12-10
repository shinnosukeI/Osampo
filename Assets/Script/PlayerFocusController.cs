using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFocusController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float focusDistance = 3.0f; // フォーカス可能な距離
    [SerializeField] private LayerMask focusLayer = ~0;  // 対象レイヤー（すべて）

    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            // 自分の子要素にカメラがある場合も考慮
            playerCamera = GetComponentInChildren<Camera>();
        }

        if (playerCamera == null)
        {
            Debug.LogError("❌ [PlayerFocusController] Camera not found!");
        }
        else
        {
            Debug.Log($"👁 [PlayerFocusController] Camera found: {playerCamera.name}");
        }
    }

    void Update()
    {
        // マウスの右クリック（Input System）
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("🖱 [PlayerFocusController] Right Click Detected");
            TryFocus();
        }
    }

    private void TryFocus()
    {
        if (playerCamera == null) return;

        // 画面中央からレイを飛ばす（Triggerは無視する）
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, focusDistance, focusLayer, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"🎯 [PlayerFocusController] Hit Raycast: {hit.collider.name} (Tag: {hit.collider.tag})");

            // IFocusableを持つオブジェクトか確認
            IFocusable focusable = hit.collider.GetComponent<IFocusable>();
            
            // 親オブジェクトにある場合も探す
            if (focusable == null)
            {
                focusable = hit.collider.GetComponentInParent<IFocusable>();
            }

            if (focusable != null)
            {
                Debug.Log($"👁 [PlayerFocusController] Focused on target: {hit.collider.name}");
                focusable.OnFocus();
            }
            else
            {
                Debug.Log("ℹ [PlayerFocusController] Target is not IFocusable");
            }
        }
        else
        {
            Debug.Log("💨 [PlayerFocusController] Raycast hit nothing");
        }
    }

    // デバッグ用にRayを表示
    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * focusDistance);
        }
    }
}
