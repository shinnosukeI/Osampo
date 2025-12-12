using UnityEngine;

public class MirrorGhostEvent : MonoBehaviour
{
    [Header("設定")]
    [SerializeField]
    private GameObject ghostObject; // 鏡に映るゴーストのオブジェクト

    [SerializeField]
    private string mirrorLayerName = "MirrorOnly"; // 鏡専用レイヤーの名前

    private bool hasTriggered = false;

    private void Start()
    {
        // レイヤーが存在するかチェック（警告用）
        int layer = LayerMask.NameToLayer(mirrorLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"⚠ [MirrorGhostEvent] Layer '{mirrorLayerName}' が見つかりません！Project Settings > Tags and Layers で作成してください。");
        }

        // 初期状態では非表示にしておく（イベント発生時に表示）
        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
    }

    public void TriggerEvent()
    {
        Debug.Log("🌑 [MirrorGhostEvent] TriggerEvent called");

        if (hasTriggered) return;

        if (ghostObject == null)
        {
            Debug.LogError("❌ [MirrorGhostEvent] Ghost Object is not assigned!");
            return;
        }

        hasTriggered = true;

        // ゴーストを表示
        ghostObject.SetActive(true);

        // レイヤー設定の確認と適用（念のため）
        // レイヤー設定の確認と適用（念のため）
        int layer = LayerMask.NameToLayer(mirrorLayerName);
        if (layer != -1)
        {
            Debug.Log($"👻 [MirrorGhostEvent] Layer found: {layer}. Setting object to this layer.");
            SetLayerRecursively(ghostObject, layer);
        }
        else
        {
            Debug.LogError($"❌ [MirrorGhostEvent] Layer '{mirrorLayerName}' not found during trigger!");
        }

        // ログ保存 (43)
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(43);
        }
    }

    // 子オブジェクト含めてレイヤーを変更する
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
