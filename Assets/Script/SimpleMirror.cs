using UnityEngine;

public class SimpleMirror : MonoBehaviour
{
    // プレイヤーのカメラ（Main Camera）
    public Transform playerCamera;

    // 鏡用カメラ
    public Camera mirrorCamera;

    // 鏡の平面 (MirrorPlane：ガラスの Quad)
    public Transform mirrorPlane;

    void LateUpdate()
    {
        if (!playerCamera || !mirrorCamera || !mirrorPlane) return;

        // --- 位置を鏡に対して対称にする --- //

        // プレイヤーの位置を鏡ローカルに変換
        Vector3 localPos = mirrorPlane.InverseTransformPoint(playerCamera.position);
        // Z を反転（鏡の向こう側へ）
        localPos.z *= -1f;
        // ワールド座標に戻してカメラに適用
        mirrorCamera.transform.position = mirrorPlane.TransformPoint(localPos);

        // --- 向きも反転 --- //

        // 向きをローカルに変換
        Vector3 localForward = mirrorPlane.InverseTransformDirection(playerCamera.forward);
        Vector3 localUp      = mirrorPlane.InverseTransformDirection(playerCamera.up);

        // Z を反転
        localForward.z *= -1f;
        localUp.z      *= -1f;

        // ワールド向きに戻して反映
        Vector3 worldForward = mirrorPlane.TransformDirection(localForward);
        Vector3 worldUp      = mirrorPlane.TransformDirection(localUp);
        mirrorCamera.transform.rotation = Quaternion.LookRotation(worldForward, worldUp);
    }
}