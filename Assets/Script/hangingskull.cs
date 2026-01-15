using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))] // ★ Add LineRenderer
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

    [Header("紐の太さ")]
    public float ropeWidth = 0.02f; // Very thin

    private Rigidbody rb;
    private HingeJoint joint;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ★ 起動時は完全停止・非表示前提
        rb.isKinematic = true;
        rb.useGravity = false;

        // LineRenderer初期化
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false; // 最初は消しておく
    }

    private void Update()
    {
        // 紐の描画更新（アンカーが存在する場合のみ）
        if (hangAnchor != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, hangAnchor.position);
            lineRenderer.SetPosition(1, transform.position); // 頭蓋骨の頭頂部（Pivot）
        }
    }

    public void HangAndSwing(Transform anchor)
    {
        hangAnchor = anchor;

        // 1) アンカー直下にワープ (Rotation offset 90 degrees added)
        // Position is slightly down based on rope length
        transform.position = hangAnchor.position + Vector3.down * ropeLength;

        // ★ Rotation Update: -90 degrees (Reversed direction)
        transform.rotation = Quaternion.Euler(0, -90f, 0); 

        // 2) Rigidbody有効化
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.angularDamping = angularDrag; 

        // 3) HingeJoint 準備（重複防止）
        joint = GetComponent<HingeJoint>();
        if (joint == null)
            joint = gameObject.AddComponent<HingeJoint>();

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null; // ワールド固定
        joint.connectedAnchor = hangAnchor.position;

        // 回転軸: 前後揺れ (Swing Forward/Back)
        // Since rotation is -90Y:
        // Local Right (+X) -> World Forward (+Z)
        // Local Forward (+Z) -> World Left (-X)
        // To swing Forward/Back (World Z movement), we need Axis on World X.
        // Therefore, use Local Forward (Z) as axis.
        joint.axis = Vector3.forward; 

        // アンカー位置をローカルで指定
        joint.anchor = transform.InverseTransformPoint(hangAnchor.position);

        // 4) 揺れを与える (前方へ)
        rb.AddForce(hangAnchor.forward * swingImpulse, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque)
        );
        rb.AddTorque(torque, ForceMode.Impulse);

        // ★ 5) 紐の表示 (LineRenderer setup)
        SetupRopeVisuals();
    }

    private void SetupRopeVisuals()
    {
        if (lineRenderer == null) return;

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        // Make it semi-transparent white (looks like a ghost string or fishing line)
        // Note: Material shader needs to support transparency (e.g., Particles/Standard Unlit or Sprites-Default)
        // We set colors here, assuming default material can handle vertex colors.
        Color ropeColor = new Color(1f, 1f, 1f, 0.3f); // 30% Alpha
        lineRenderer.startColor = ropeColor;
        lineRenderer.endColor = ropeColor;
        
        // Use World Space to connect anchor (world) to skull (world)
        lineRenderer.useWorldSpace = true;
    }
}