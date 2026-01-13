using UnityEngine;
using UnityEngine.UI;

public class TitleFogController : MonoBehaviour
{
    [Header("Fog Settings")]
    [SerializeField] private Color fogColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
    [SerializeField] private Vector2 fogSpeed = new Vector2(0.05f, 0.02f);
    [SerializeField] private float fogScale = 3.0f;

    [Header("UI Sort Order")]
    [SerializeField] private int sortOrder = -1; // Default to behind other UI

    private void Start()
    {
        CreateFogLayer();
    }

    private void CreateFogLayer()
    {
        string fogObjName = "MistCanvas";
        
        // Prevent duplicate creation
        if (GameObject.Find(fogObjName) != null)
        {
            return;
        }

        // 1. Create Canvas GameObject
        GameObject canvasObj = new GameObject(fogObjName);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder; // Ensure it's behind the main UI

        // Add CanvasScaler (Optional, but good practice)
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 2. Create Image GameObject for the Fog
        GameObject imageObj = new GameObject("MistImage");
        imageObj.transform.SetParent(canvasObj.transform, false);

        Image img = imageObj.AddComponent<Image>();
        
        // Stretch to fill screen
        RectTransform rt = imageObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // 3. Create Material and assign Shader
        Shader mistShader = Shader.Find("Unlit/SimpleMist");
        if (mistShader != null)
        {
            Material mistMat = new Material(mistShader);
            mistMat.SetColor("_Color", fogColor);
            mistMat.SetVector("_Speed", new Vector4(fogSpeed.x, fogSpeed.y, 0, 0));
            mistMat.SetFloat("_Scale", fogScale);
            
            img.material = mistMat;
        }
        else
        {
            Debug.LogError("TitleFogController: 'Unlit/SimpleMist' shader not found!");
            // Fallback: Just set color semi-transparent
            img.color = fogColor;
        }

        // Raycast Target Off (so it doesn't block clicks)
        img.raycastTarget = false;
    }
}
