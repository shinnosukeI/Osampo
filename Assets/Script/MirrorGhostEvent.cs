using UnityEngine;

public class MirrorGhostEvent : MonoBehaviour, IFocusable
{
    [Header("設定")]
    [SerializeField]
    private GameObject ghostObject; // 鏡に映るゴーストのオブジェクト

    [SerializeField]
    private string mirrorLayerName = "MirrorOnly"; // 鏡専用レイヤーの名前

    [Header("音効")]
    [SerializeField] private AudioClip zombieSound; // ★ 追加: ゾンビの声
    [SerializeField] private AudioSource audioSource; // ★ 追加

    private bool hasTriggered = false;
    private bool isReady = false; // トリガー済みで、フォーカス待機中か？

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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
            
        // ★ スタート時にレイヤーを適用しておく（表示された一瞬が見えないように）
            int startLayer = LayerMask.NameToLayer(mirrorLayerName);
            if (startLayer != -1)
            {
                SetLayerRecursively(ghostObject, startLayer);
            }
        }
    }

    // マネージャーから呼ばれる（準備完了）
    public void TriggerEvent()
    {
        Debug.Log("🌑 [MirrorGhostEvent] TriggerEvent called (Waiting for Focus)");
        isReady = true;

        // ★ 安全策: イベント開始時は確実に非表示にする
        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }
    }

    // プレイヤーが鏡を見たときに呼ばれる
    public void OnFocus()
    {
        // ★ 追加ガード: 本当に右クリックされたか確認
        // (他から呼ばれた場合や、Hoverだけで呼ばれてしまった場合の防止)
        if (UnityEngine.InputSystem.Mouse.current != null && !UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
        {
            return;
        }

        Debug.Log($"👁 [MirrorGhostEvent] OnFocus called. isReady: {isReady}, hasTriggered: {hasTriggered}");

        if (!isReady)
        {
            // まだイベントループに入っていない、または順番待ち
            return;
        }

        if (hasTriggered)
        {
             // 既に発動済みなら何度もしない
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

        // ★ 音を鳴らす
        if (audioSource != null && zombieSound != null)
        {
            audioSource.PlayOneShot(zombieSound);
            Debug.Log("🔊 [MirrorGhostEvent] Playing Zombie Sound.");
        }

        // レイヤー設定の確認と適用（念のため）
        int layer = LayerMask.NameToLayer(mirrorLayerName);
        if (layer != -1)
        {
            Debug.Log($"👻 [MirrorGhostEvent] Layer found: {layer}. Setting object to this layer.");
            SetLayerRecursively(ghostObject, layer);

            // ★ 追加: メインカメラがこのレイヤーを映さないように強制設定
            if (Camera.main != null)
            {
                // ビットを落とす（非表示）
                Camera.main.cullingMask &= ~(1 << layer);
                Debug.Log($"👁 [MirrorGhostEvent] Main Camera CullingMask updated to HIDE layer {layer}.");
            }
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
