using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingCorpse : MonoBehaviour
{
    [Header("落下音設定")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float minImpactVelocity = 1.0f;

    [Header("倒れ方の調整")]
    [Tooltip("ローカル座標系での回転方向 (例: X軸まわりなら 1,0,0)")]
    [SerializeField] private Vector3 localTorqueDir = new Vector3(1, 0, 0);
    [SerializeField] private float torqueImpulse = 5f;

    [Tooltip("ローカル座標系での押し出し方向 (例: 手前に押すなら 0,0,1)")]
    [SerializeField] private Vector3 localPushDir = new Vector3(0, 0, 1);
    [SerializeField] private float pushImpulse = 0.5f;

    private AudioSource audioSource;
    private Rigidbody rb;
    private bool hasImpacted = false;

    void Awake()
    {
    rb = GetComponent<Rigidbody>();
    audioSource = GetComponent<AudioSource>();

    // ここを追加（絶対に回転できるようにする）
    rb.constraints = RigidbodyConstraints.None;

    rb.isKinematic = true;
    }
    // EventManager から呼ぶ
    public void StartFalling()
    {
        if (!rb.isKinematic) return;

        Debug.Log("⏰ FallingCorpse: 落下開始");
        rb.isKinematic = false;  // 物理ON

        // ローカル→ワールドに変換して力を加える
        Vector3 torqueWorld = transform.TransformDirection(localTorqueDir.normalized) * torqueImpulse;
        Vector3 pushWorld   = transform.TransformDirection(localPushDir.normalized)   * pushImpulse;

        rb.AddTorque(torqueWorld, ForceMode.Impulse); // 前に倒す
        rb.AddForce(pushWorld,   ForceMode.Impulse);  // 机の外に少し押し出す
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        if (collision.relativeVelocity.magnitude >= minImpactVelocity)
        {
            if (audioSource != null && impactSound != null)
                audioSource.PlayOneShot(impactSound);

            hasImpacted = true;
        }
    }
}