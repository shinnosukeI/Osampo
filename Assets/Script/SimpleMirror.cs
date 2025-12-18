using UnityEngine;

public class MirrorCameraController : MonoBehaviour
{
    [Header("参照するカメラ（プレイヤー側）")]
    public Transform playerCamera;   // メインカメラ（プレイヤーの視点）

    [Header("鏡オブジェクト（Quad）")]
    public Transform mirror;        // 鏡のQuad

    [Header("鏡用カメラ")]
    public Camera mirrorCamera;     // MirrorCamera

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