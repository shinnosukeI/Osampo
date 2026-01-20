using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerFocusController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private float focusDistance = 5.0f; // フォーカス可能な距離
    [SerializeField] private float focusRadius = 0.3f;   // 判定の太さ（半径）
    [SerializeField] private LayerMask focusLayer = ~0;  // 対象レイヤー（すべて）
    [SerializeField] private GameObject actionGuideUI; // カステムアクションガイドUI

    private Camera playerCamera;
    private HorrorEventManager horrorEventManager;

    // ★ 現在ホバー中の（または直前までホバーしていた）ターゲットをキャッシュ
    private IFocusable cachedFocusTarget = null;
    private Collider cachedFocusCollider = null;

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

            // ★ カメラ位置の自動補正（ユーザーが手動で直せない場合用）
            // ローカル座標のYが適切でない場合（1.5m未満なら補正）、強制的に1.0mに設定する
            if (playerCamera.transform.localPosition.y < 1.5f)
            {
                Debug.LogWarning($"⚠ [PlayerFocusController] Camera height adjustment (Current: {playerCamera.transform.localPosition.y}). Auto-fixing to Y=1.0.");
                Vector3 newPos = playerCamera.transform.localPosition;
                newPos.y = 1.0f;
                playerCamera.transform.localPosition = newPos;
            }
        }

        SetupCrosshair();
    }

    private void TryFocus()
    {
        // ★ レティクルが表示されている（=キャッシュが有効）なら、物理判定を飛ばしてそれを使う
        // これにより「見た目」と「動作」の一致を保証する
        if (isHoveringFocusable && cachedFocusTarget != null)
        {
            Debug.Log($"🎯 [PlayerFocusController] Using Cached Target: {cachedFocusCollider.name}");
            cachedFocusTarget.OnFocus();
            return;
        }

        // キャッシュがない場合（一応フォールバックとして通常のRaycastも残すが、基本は上の分岐に入るはず）
        if (playerCamera == null) return;
        Debug.Log("ℹ [PlayerFocusController] No cached target, trying Raycast fallback...");

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit = new RaycastHit();
        bool foundValidTarget = false;

        // 1. Raycast (点)
        RaycastHit[] hitsPoint = Physics.RaycastAll(ray, focusDistance, focusLayer, QueryTriggerInteraction.Collide);
        foundValidTarget = TryFindFocusable(hitsPoint, out hit);

        // 2. SphereCast (円柱)
        if (!foundValidTarget)
        {
            RaycastHit[] hitsSphere = Physics.SphereCastAll(ray, focusRadius, focusDistance, focusLayer, QueryTriggerInteraction.Collide);
            foundValidTarget = TryFindFocusable(hitsSphere, out hit);
        }

        if (foundValidTarget)
        {
            Debug.Log($"🎯 [PlayerFocusController] Focused Target (Fallback): {hit.collider.name}");

            IFocusable focusable = hit.collider.GetComponent<IFocusable>();
            if (focusable == null) focusable = hit.collider.GetComponentInParent<IFocusable>();

            if (focusable != null)
            {
                focusable.OnFocus();
            }
        }
    }

    private void CheckFocusableHover()
    {
        if (playerCamera == null) return;
        if (crosshairRect == null) return; 
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        bool found = false;
        RaycastHit hit = new RaycastHit();
        
        // 1. Raycast
        RaycastHit[] hitsPoint = Physics.RaycastAll(ray, focusDistance, focusLayer, QueryTriggerInteraction.Collide);
        found = TryFindFocusable(hitsPoint, out hit);

        // 2. SphereCast
        if (!found)
        {
            RaycastHit[] hitsSphere = Physics.SphereCastAll(ray, focusRadius, focusDistance, focusLayer, QueryTriggerInteraction.Collide);
            found = TryFindFocusable(hitsSphere, out hit);
        }

        // ホバー状態の更新
        if (found)
        {
            // 有効なターゲットを発見 -> キャッシュ更新
            IFocusable focusable = hit.collider.GetComponent<IFocusable>();
            if (focusable == null) focusable = hit.collider.GetComponentInParent<IFocusable>();

            if (focusable != null)
            {
                cachedFocusTarget = focusable;
                cachedFocusCollider = hit.collider;
            }
        }

        UpdateCrosshairState(found);
    }

    // ★ ヘルパーメソッド
    private bool TryFindFocusable(RaycastHit[] hits, out RaycastHit resultHit)
    {
        resultHit = new RaycastHit();
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var h in hits)
        {
            if (h.collider.gameObject == gameObject || h.collider.transform.IsChildOf(transform))
                continue;

            IFocusable checkFocus = h.collider.GetComponent<IFocusable>();
            if (checkFocus == null) checkFocus = h.collider.GetComponentInParent<IFocusable>();

            if (checkFocus != null)
            {
                if (IsValidTarget(h.collider, checkFocus))
                {
                    resultHit = h;
                    return true; 
                }
                else
                {
                    Debug.Log($"ℹ [TryFindFocusable] Invalid target skipped: {h.collider.name}");
                }
            }
        }
        return false;
    }

    private bool IsValidTarget(Collider hitCollider, IFocusable focusable)
    {
         if (focusable == null || !(focusable is Component)) return false;

         // ★ 追加: MonoBehaviourの場合は enabled でなければ無視する
         if (focusable is MonoBehaviour mb && !mb.enabled)
         {
             return false;
         }

         string objName = (focusable as Component).name.ToLower().Trim();

         // 1. Name exclude
         if (objName == "doorpovit" || objName == "doorprovit")
         {
             // Debug.Log($"   -> Ignored: Name match ({objName})");
             return false;
         }

         // 2. Specific door control
         if (objName.Contains("doorpovit (3)") || objName.Contains("doorprovit (3)"))
         {
             if (horrorEventManager != null && horrorEventManager.CycleCount == 0)
             {
                 // Debug.Log($"   -> Ignored: CycleCount 0");
                 return false;
             }
             
             DoorController door = hitCollider.GetComponentInParent<DoorController>();
             if (door != null && door.HasBeenInteracted)
             {
                  // Debug.Log($"   -> Ignored: Already Interacted");
                  return false;
             }
         }

         return true;
    }


    // チラつき防止用のタイマー
    private float hoverLostTimer = 0f;
    private const float HOVER_LOST_DELAY = 0.15f; // 0.15秒猶予

    private void UpdateCrosshairState(bool isRealtimeHit)
    {
        if (isRealtimeHit)
        {
            // ヒットしたら即時オン、タイマーリセット
            hoverLostTimer = HOVER_LOST_DELAY;
            
            if (!isHoveringFocusable)
            {
                isHoveringFocusable = true;
                SetCrosshairVisuals(true);
            }
        }
        else
        {
            // ヒットか外れてもタイマー内ならオン維持
            if (hoverLostTimer > 0f)
            {
                hoverLostTimer -= Time.deltaTime;
                // 維持中は isHoveringFocusable を true のままにする
            }
            else
            {
               // 完全にロスト
               if (isHoveringFocusable)
               {
                   isHoveringFocusable = false;
                   SetCrosshairVisuals(false);
                   
                   // ★ キャッシュもここでクリア
                   cachedFocusTarget = null;
                   cachedFocusCollider = null;
               }
            }
        }
    }

    private void SetCrosshairVisuals(bool active)
    {
        if (active)
        {
            crosshairRect.localScale = Vector3.one * 1.5f;
            if (activeActionUIObject) activeActionUIObject.SetActive(true);
            foreach (var img in crosshairImages)
            {
                img.color = new Color(1f, 0.6f, 0.6f, 0.9f); 
            }
        }
        else
        {
            crosshairRect.localScale = Vector3.one;
            if (activeActionUIObject) activeActionUIObject.SetActive(false);
            foreach (var img in crosshairImages)
            {
                img.color = new Color(1f, 1f, 1f, 0.5f); 
            }
        }
    }

    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * focusDistance);
            Gizmos.DrawWireSphere(playerCamera.transform.position + playerCamera.transform.forward * focusDistance, focusRadius);
        }
    }

    // UI参照用キャッシュ
    private RectTransform crosshairRect;
    private UnityEngine.UI.Image[] crosshairImages;
    private GameObject activeActionUIObject; 
    private bool isHoveringFocusable = false;

    private void SetupCrosshair()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (sceneName != "Stage1" && sceneName != "Stage2" && 
            sceneName != "99_BPMTestScene1" && sceneName != "99_BPMTestScene2")
        {
            return;
        }

        if (GameObject.Find("CrosshairCanvas") != null) return;

        GameObject canvasObj = new GameObject("CrosshairCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject crosshairObj = new GameObject("Crosshair");
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        RectTransform rect = crosshairObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        void CreateLine(string name, Vector2 size)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(crosshairObj.transform, false);
            UnityEngine.UI.Image img = lineObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(1f, 1f, 1f, 0.5f);
            img.raycastTarget = false;
            
            UnityEngine.UI.Outline outline = lineObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1f, -1f);

            RectTransform lineRect = lineObj.GetComponent<RectTransform>();
            lineRect.sizeDelta = size;
        }

        CreateLine("H_Line", new Vector2(20f, 2f));
        CreateLine("V_Line", new Vector2(2f, 20f));

        crosshairRect = crosshairObj.GetComponent<RectTransform>();
        crosshairImages = crosshairObj.GetComponentsInChildren<UnityEngine.UI.Image>();

        if (actionGuideUI != null)
        {
            activeActionUIObject = actionGuideUI;
            activeActionUIObject.SetActive(false);
        }
        else
        {
            GameObject textObj = new GameObject("ActionText");
            textObj.transform.SetParent(crosshairObj.transform, false);
            UnityEngine.UI.Text actionText = textObj.AddComponent<UnityEngine.UI.Text>();
            actionText.text = "右クリックでアクション";
            actionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (actionText.font == null) actionText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            actionText.fontSize = 24;
            actionText.color = Color.white; 
            actionText.alignment = TextAnchor.MiddleCenter;
            actionText.horizontalOverflow = HorizontalWrapMode.Overflow;
            actionText.verticalOverflow = VerticalWrapMode.Overflow;

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

    void Update()
    {
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Debug.Log("🖱 [PlayerFocusController] Right Click Detected");
            TryFocus();
        }

        CheckFocusableHover();
    }
}
