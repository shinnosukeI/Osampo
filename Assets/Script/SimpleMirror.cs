using UnityEngine;

public class MirrorCameraController : MonoBehaviour
{
    [Header("参照するカメラ（プレイヤー側）")]
    public Transform playerCamera;   // メインカメラ（プレイヤーの視点）

    [Header("鏡オブジェクト（Quad）")]
    public Transform mirror;        // 鏡のQuad

    [Header("鏡用カメラ")]
    public Camera mirrorCamera;     // MirrorCamera

    [Header("レイヤー設定")]
    [SerializeField] private string mirrorLayerName = "MirrorOnly";

    void Start()
    {
        // カメラが未設定ならMainCameraを取得
        if (playerCamera == null)
        {
            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
                Debug.Log("📸 [MirrorCameraController] playerCamera was null, auto-assigned from Camera.main.");
            }
            else
            {
                Debug.LogError("❌ [MirrorCameraController] Main Camera not found!");
                return;
            }
        }

        int layerIndex = LayerMask.NameToLayer(mirrorLayerName);
        if (layerIndex == -1)
        {
            Debug.LogError($"❌ [MirrorCameraController] Layer '{mirrorLayerName}' not found. Please create it in Project Settings.");
            return;
        }

        // Main Camera: MirrorOnlyレイヤーを非表示にする
        if (playerCamera != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
            {
                // ビット演算前の状態をログ
                // Debug.Log($"[MirrorInfo] Before Mask: {cam.cullingMask = Convert.ToString(cam.cullingMask, 2)}");
                
                cam.cullingMask &= ~(1 << layerIndex);
                
                Debug.Log($"✅ [MirrorCameraController] HIDDEN layer '{mirrorLayerName}' (Index {layerIndex}) from Main Camera.");
            }
        }

        // Mirror Camera: MirrorOnlyレイヤーを表示する
        if (mirrorCamera != null)
        {
            mirrorCamera.cullingMask |= (1 << layerIndex);
            Debug.Log($"✅ [MirrorCameraController] SHOWN layer '{mirrorLayerName}' (Index {layerIndex}) in Mirror Camera.");
        }
    }

    void Update()
    {
        // 1秒に1回など、定期的にチェックしてマスクが外れていたら強制適用する（他のスクリプトによる上書き対策）
        if (Time.frameCount % 60 == 0 && playerCamera != null)
        {
             Camera cam = playerCamera.GetComponent<Camera>();
             int layerIndex = LayerMask.NameToLayer(mirrorLayerName);
             if (cam != null && layerIndex != -1)
             {
                 // もしマスクに含まれてしまっていたら（ビットが立っていたら）
                 if ((cam.cullingMask & (1 << layerIndex)) != 0)
                 {
                      Debug.LogWarning($"⚠ [MirrorCameraController] Mask reset detected! Re-hiding layer '{mirrorLayerName}'");
                      cam.cullingMask &= ~(1 << layerIndex);
                 }
             }
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null || mirror == null || mirrorCamera == null)
            return;

        // 鏡の「表向き」法線（Quad が向いている方向）
        Vector3 mirrorNormal = mirror.forward; 
        Vector3 mirrorPos = mirror.position;

        // プレイヤーカメラから見た、鏡の中心へのベクトル
        Vector3 toCam = playerCamera.position - mirrorPos;

        // その位置を鏡平面で反転させて、鏡の向こう側に置く
        Vector3 reflectedPos = Vector3.Reflect(toCam, mirrorNormal);

        mirrorCamera.transform.position = mirrorPos + reflectedPos;

        // カメラの向きも鏡で反射させる
        Vector3 reflectedForward = Vector3.Reflect(playerCamera.forward, mirrorNormal);

        mirrorCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, Vector3.up);
    }
}