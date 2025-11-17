using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightZoom : MonoBehaviour
{
    [Header("Camera 設定")]
    public Camera playerCamera;     // プレイヤーカメラ
    public float normalFOV = 60f;   // 通常時の視野角
    public float zoomFOV = 30f;     // ズーム時の視野角
    public float zoomLerpSpeed = 10f; // ズームの移動速度

    [Header("Flashlight (任意)")]
    public Light flashLight;        // 懐中電灯用の Spot Light
    public float normalSpotAngle = 60f;  // 通常時のスポット角度
    public float zoomSpotAngle = 30f;    // ズーム時のスポット角度（細くする）
    public float normalRange = 10f;      // 通常の照射距離
    public float zoomRange = 20f;        // ズーム時の照射距離（遠くを照らす）

    void Start()
    {
        // カメラが未指定なら、このオブジェクトのカメラを自動取得
        if (playerCamera == null)
        {
            playerCamera = GetComponent<Camera>();
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }

        if (flashLight != null)
        {
            flashLight.spotAngle = normalSpotAngle;
            flashLight.range = normalRange;
        }
    }

    void Update()
    {
        // 右クリックを押している間ズーム
       bool isZooming = Mouse.current != null && Mouse.current.rightButton.isPressed; // 1 = 右クリック

        float targetFOV = isZooming ? zoomFOV : normalFOV;

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * zoomLerpSpeed
            );
        }

        // 懐中電灯も一緒に変化させたい場合
        if (flashLight != null)
        {
            float targetSpot = isZooming ? zoomSpotAngle : normalSpotAngle;
            float targetRange = isZooming ? zoomRange : normalRange;

            flashLight.spotAngle = Mathf.Lerp(
                flashLight.spotAngle,
                targetSpot,
                Time.deltaTime * zoomLerpSpeed
            );
            flashLight.range = Mathf.Lerp(
                flashLight.range,
                targetRange,
                Time.deltaTime * zoomLerpSpeed
            );
        }
    }
}