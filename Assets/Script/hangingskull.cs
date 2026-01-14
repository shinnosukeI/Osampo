using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HangingSkull : MonoBehaviour
{
    [Header("吊り点（天井アンカー）")]
    public Transform hangAnchor;

    [Header("吊り紐の長さ")]
    public float ropeLength = 1.5f;

    [Header("揺れの初速")]
    public float swingImpulse = 2.0f;

    [Header("ランダム回転")]
    public float randomTorque = 0.5f;

    [Header("揺れ減衰")]
    public float angularDrag = 0.5f;

    private Rigidbody rb;
    private HingeJoint joint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ★ 起動時は完全停止・非表示前提
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void HangAndSwing(Transform anchor)
    {
        hangAnchor = anchor;

        // 1) アンカー直下にワープ
        transform.position = hangAnchor.position + Vector3.down * ropeLength;

        // 2) Rigidbody有効化
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.angularDamping = angularDrag; // ← 正しいプロパティ

        // 3) HingeJoint 準備（重複防止）
        joint = GetComponent<HingeJoint>();
        if (joint == null)
            joint = gameObject.AddComponent<HingeJoint>();

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null; // ワールド固定
        joint.connectedAnchor = hangAnchor.position;

        // 回転軸（左右揺れ）
        joint.axis = Vector3.forward;

        // アンカー位置をローカルで指定
        joint.anchor = transform.InverseTransformPoint(hangAnchor.position);

        // 4) 揺れを与える
        rb.AddForce(hangAnchor.right * swingImpulse, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque)
        );
        rb.AddTorque(torque, ForceMode.Impulse);
    }
}

//ssss