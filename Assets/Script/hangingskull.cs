using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]   // ★ 追加
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
    public float ropeWidth = 0.02f;

    [Header("出現音")]
    public AudioClip appearSound;          // ★ 追加

    private Rigidbody rb;
    private HingeJoint joint;
    private LineRenderer lineRenderer;
    private AudioSource audioSource;       // ★ 追加

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();   // ★ 追加

        rb.isKinematic = true;
        rb.useGravity = false;

        lineRenderer.enabled = false;

        // 音設定
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D音
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 10f;
    }

    private void Update()
    {
        if (hangAnchor != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, hangAnchor.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    public void HangAndSwing(Transform anchor)
    {
        hangAnchor = anchor;

        transform.position = hangAnchor.position + Vector3.down * ropeLength;
        transform.rotation = Quaternion.Euler(0, -90f, 0);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.angularDamping = angularDrag;

        joint = GetComponent<HingeJoint>();
        if (joint == null) joint = gameObject.AddComponent<HingeJoint>();

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = null;
        joint.connectedAnchor = hangAnchor.position;
        joint.axis = Vector3.forward;
        joint.anchor = transform.InverseTransformPoint(hangAnchor.position);

        rb.AddForce(hangAnchor.forward * swingImpulse, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque)
        );
        rb.AddTorque(torque, ForceMode.Impulse);

        SetupRopeVisuals();

        // ★ 出現音を鳴らす
        if (appearSound != null)
            audioSource.PlayOneShot(appearSound);
    }

    private void SetupRopeVisuals()
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;

        Color ropeColor = new Color(1f, 1f, 1f, 0.3f);
        lineRenderer.startColor = ropeColor;
        lineRenderer.endColor = ropeColor;
        lineRenderer.useWorldSpace = true;
    }
}