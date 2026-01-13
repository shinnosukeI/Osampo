using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerFocusController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float focusDistance = 3.0f; // フォーカス可能な距離
    [SerializeField] private LayerMask focusLayer = ~0;  // 対象レイヤー（すべて）
    [SerializeField] private GameObject actionGuideUI; // カステムアクションガイドUI

    private Camera playerCamera;
    private HorrorEventManager horrorEventManager;

    void Start()
    {
        horrorEventManager = FindFirstObjectByType<HorrorEventManager>();

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

        SetupCrosshair();
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
                 // コンポーネントがアタッチされているオブジェクトの名前を確認する
                string objName = (focusable as Component).name.ToLower().Trim();

                // 特定の名前のオブジェクトは除外
                if (objName == "doorpovit" || objName == "doorprovit")
                {
                    Debug.Log($"ℹ [PlayerFocusController] Ignored target: {objName}");
                    return;
                }

                // 特定のドア (doorpovit (3)) はCycleCountなどの条件で制御
                if (objName.Contains("doorpovit (3)") || objName.Contains("doorprovit (3)"))
                {
                    // HorrorEventManagerが見つからない場合は安全のため通常のワンタイム動作とするか、デフォルトの挙動にする
                    // CycleCount == 0 の時は反応しない
                    if (horrorEventManager != null && horrorEventManager.CycleCount == 0)
                    {
                        Debug.Log($"ℹ [PlayerFocusController] Target {objName} ignored (Cycle 0).");
                        return;
                    }

                    // CycleCount >= 1 または マネージャー不明時はワンタイム
                    DoorController door = hit.collider.GetComponentInParent<DoorController>();
                    if (door != null && door.HasBeenInteracted)
                    {
                         Debug.Log($"ℹ [PlayerFocusController] Target {hit.collider.name} has already been interacted with.");
                         return;
                    }
                }

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

    private void SetupCrosshair()
    {
        // シーン名による表示制限
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"👁 [SetupCrosshair] Current Scene: {sceneName}");

        // Stage1, Stage2, およびそれらのBPMテストシーンでのみ表示
        if (sceneName != "Stage1" && sceneName != "Stage2" && 
            sceneName != "99_BPMTestScene1" && sceneName != "99_BPMTestScene2")
        {
            Debug.Log($"ℹ [SetupCrosshair] Skipping crosshair creation for this scene.");
            return;
        }

        // 既にクロスヘアがある場合は作成しない（重複防止）
        if (GameObject.Find("CrosshairCanvas") != null)
        {
            Debug.Log("ℹ [SetupCrosshair] CrosshairCanvas already exists.");
            return;
        }

        Debug.Log("🔨 [SetupCrosshair] Creating CrosshairCanvas...");

        // Canvas作成
        GameObject canvasObj = new GameObject("CrosshairCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767; // 最前面に表示 (Max)
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 親としてプレイヤーのライフサイクルに合わせたい場合、別オブジェクトにするか検討するが、
        // ScreenSpaceOverlayはルートにおくのが一般的。
        // シーン遷移で破棄されるように、プレイヤーの子にするか、DontDestroyOnLoadしないでおく。
        // ここでは単純に生成し、シーン切り替えで破棄されるに任せる。
        
        // クロスヘアの親オブジェクト（画面中央）
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rect = crosshairObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero; // 中央

        // 画像の生成関数
        void CreateLine(string name, Vector2 size)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(crosshairObj.transform, false);
            UnityEngine.UI.Image img = lineObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.5f); // 半透明の白
            img.raycastTarget = false; // レイキャストをブロックしない
            
            // アウトラインを追加して視認性向上
            UnityEngine.UI.Outline outline = lineObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);

            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.sizeDelta = size;
        }

        // 横線 (幅20, 高さ2)
        CreateLine("H_Line", new Vector2(20f, 2f));
        // 縦線 (幅2, 高さ20)
        CreateLine("V_Line", new Vector2(2f, 20f));

        // キャッシュ
        crosshairRect = crosshairObj.GetComponent<RectTransform>();
        crosshairImages = crosshairObj.GetComponentsInChildren<UnityEngine.UI.Image>();

        // ガイドUIのセットアップ
        if (actionGuideUI != null)
        {
            activeActionUIObject = actionGuideUI;
            activeActionUIObject.SetActive(false);
        }
        else
        {
            // デフォルトのテキスト作成（フォールバック）
            GameObject textObj = new GameObject("ActionText");
            textObj.transform.SetParent(crosshairObj.transform, false);
            UnityEngine.UI.Text actionText = textObj.AddComponent<UnityEngine.UI.Text>();
            actionText.text = "右クリックでアクション";
            actionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (actionText.font == null) actionText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            actionText.fontSize = 24;
            // テキストの色も少しマイルドな赤に
            actionText.color = Color.white; 
            actionText.alignment = TextAnchor.MiddleCenter;
            actionText.horizontalOverflow = HorizontalWrapMode.Overflow;
            actionText.verticalOverflow = VerticalWrapMode.Overflow;

            // テキストにもアウトラインを追加して視認性向上
            UnityEngine.UI.Outline textOutline = textObj.AddComponent<UnityEngine.UI.Outline>();
            textOutline.effectColor = Color.black;
            textOutline.effectDistance = new Vector2(1f, -1f);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0f, -40f);
            textRect.sizeDelta = new Vector2(300f, 50f);
            
            activeActionUIObject = textObj;
            activeActionUIObject.SetActive(false);
        }
    }

    // UI参照用キャッシュ
    private RectTransform crosshairRect;
    private UnityEngine.UI.Image[] crosshairImages;
    private GameObject activeActionUIObject; // 表示切り替え用オブジェクト
    private bool isHoveringFocusable = false;

    void Update()
    {
        // マウスの右クリック（Input System）
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("🖱 [PlayerFocusController] Right Click Detected");
            TryFocus();
        }

        CheckFocusableHover();
    }

    private void CheckFocusableHover()
    {
        if (playerCamera == null) return;
        if (crosshairRect == null) return; 

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool hitFocusable = false;

        if (Physics.Raycast(ray, out RaycastHit hit, focusDistance, focusLayer, QueryTriggerInteraction.Ignore))
        {
            IFocusable focusable = hit.collider.GetComponent<IFocusable>();
            if (focusable == null) focusable = hit.collider.GetComponentInParent<IFocusable>();

            if (focusable != null)
            {
                // コンポーネントがアタッチされているオブジェクトの名前を確認する
                // (Colliderが子要素にある場合、hit.collider.nameでは親の名前が取れないため)
                string objName = (focusable as Component).name.ToLower().Trim();

                // 特定の名前のオブジェクトは除外（完全一致のみ除外して、(1)などは許可する）
                if (objName == "doorpovit" || objName == "doorprovit")
                {
                    hitFocusable = false;
                }
                // 特定のドア (doorpovit (3)) はCycleCountなどの条件で制御
                else if (objName.Contains("doorpovit (3)") || objName.Contains("doorprovit (3)"))
                {
                     // CycleCount == 0 の時は反応しない
                     if (horrorEventManager != null && horrorEventManager.CycleCount == 0)
                     {
                         hitFocusable = false;
                     }
                     else
                     {
                         // CycleCount >= 1: ワンタイム
                         DoorController door = hit.collider.GetComponentInParent<DoorController>();
                         if (door != null && door.HasBeenInteracted)
                         {
                             hitFocusable = false;
                         }
                         else
                         {
                             hitFocusable = true;
                         }
                     }
                }
                else
                {
                    hitFocusable = true;
                }
            }
        }

        UpdateCrosshairState(hitFocusable);
    }

    private void UpdateCrosshairState(bool isHovering)
    {
        if (isHovering == isHoveringFocusable) return; // 状態変化なしなら何もしない
        isHoveringFocusable = isHovering;

        if (isHovering)
        {
            // ハイライト状態
            crosshairRect.localScale = Vector3.one * 1.5f; // 1.5倍に拡大
            if (activeActionUIObject) activeActionUIObject.SetActive(true);
            foreach (var img in crosshairImages)
            {
                // 明度を上げたマイルドな赤 (白に近い赤)
                img.color = new Color(1f, 0.6f, 0.6f, 0.9f); 
            }
        }
        else
        {
            // 通常状態
            crosshairRect.localScale = Vector3.one;
            if (activeActionUIObject) activeActionUIObject.SetActive(false);
            foreach (var img in crosshairImages)
            {
                img.color = new Color(1f, 1f, 1f, 0.5f); // 半透明の白
            }
        }
    }
}
