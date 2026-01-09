using UnityEngine;

public class MirrorGhostEvent : MonoBehaviour, IFocusable
{
    [Header("設定")]
    [SerializeField]
    private GameObject ghostObject; // 鏡に映るゴーストのオブジェクト

    [SerializeField]
    private string mirrorLayerName = "MirrorOnly"; // 鏡専用レイヤーの名前

    private bool hasTriggered = false;
    private bool isReady = false; // トリガー済みで、フォーカス待機中か？

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

    // マネージャーから呼ばれる（準備完了）
    public void TriggerEvent()
    {
        Debug.Log("🌑 [MirrorGhostEvent] TriggerEvent called (Waiting for Focus)");
        isReady = true;
    }

    // プレイヤーが鏡を見たときに呼ばれる
    public void OnFocus()
    {
        Debug.Log($"👁 [MirrorGhostEvent] OnFocus called. isReady: {isReady}, hasTriggered: {hasTriggered}");

        if (!isReady)
        {
            Debug.LogWarning("⚠ [MirrorGhostEvent] OnFocus ignored because isReady is FALSE. (TriggerEvent was not called?)");
            return;
        }

        if (hasTriggered)
        {
            Debug.Log("ℹ [MirrorGhostEvent] OnFocus ignored because hasTriggered is TRUE.");
            return;
        }

        hasTriggered = true;
        Debug.Log("👁 [MirrorGhostEvent] Activating Ghost!");

        if (ghostObject == null)
        {
            Debug.LogError("❌ [MirrorGhostEvent] Ghost Object is not assigned!");
            return;
        }

        // ゴーストを表示
        ghostObject.SetActive(true);

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
