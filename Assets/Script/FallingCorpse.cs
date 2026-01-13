using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingCorpse : MonoBehaviour
{
    [Header("落下音設定")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float minImpactVelocity = 1.0f;

    [Header("倒れ方の調整")]
    [Tooltip("回転（トルク）を加えるかどうか")]
    [SerializeField] private bool enableRotation = false; // ★ デフォルト false に変更

    [Tooltip("ローカル座標系での回転方向 (例: X軸まわりなら 1,0,0)")]
    [SerializeField] private Vector3 localTorqueDir = new Vector3(1, 0, 0);
    [SerializeField] private float torqueImpulse = 5f;

    [Tooltip("ローカル座標系での押し出し方向 (例: 手前に押すなら 0,0,1)")]
    [SerializeField] private Vector3 localPushDir = new Vector3(0, 0, 1);
    [SerializeField] private float pushImpulse = 0.5f;

    [Header("ラグドール設定")]
    [Tooltip("力を加える対象の部位（腰など）。未設定なら自身のRigidbodyを使います")]
    [SerializeField] private Transform pushTarget;

    private AudioSource audioSource;
    private Rigidbody[] allRigidbodies; // 子階層含む全てのRB
    private bool hasImpacted = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // 自分自身と子階層のすべてのRigidbodyを取得
        allRigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (var rb in allRigidbodies)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = true; // 最初は物理演算OFF
        }
    }

    // EventManager から呼ぶ
    public void StartFalling()
    {
        if (allRigidbodies == null || allRigidbodies.Length == 0) return;

        Debug.Log("⏰ FallingCorpse: ラグドール落下開始");

        // 全てのRigidbodyをONにする
        foreach (var rb in allRigidbodies)
        {
            rb.isKinematic = false;
        }

        // 力を加える対象を決定（設定されていればそのTransformのRB、なければ自分のRB）
        Rigidbody mainRb = GetComponent<Rigidbody>();
        if (pushTarget != null)
        {
            Rigidbody targetRb = pushTarget.GetComponent<Rigidbody>();
            if (targetRb != null) mainRb = targetRb;
        }

        if (mainRb != null)
        {
            // ローカル→ワールドに変換して力を加える
            if (enableRotation) 
            {
                Vector3 torqueWorld = transform.TransformDirection(localTorqueDir.normalized) * torqueImpulse;
                mainRb.AddTorque(torqueWorld, ForceMode.Impulse); 
            }
            
            Vector3 pushWorld = transform.TransformDirection(localPushDir.normalized) * pushImpulse;
            mainRb.AddForce(pushWorld, ForceMode.Impulse);  
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 音声再生ロジック（そのまま維持）
        // ラグドールの場合、各部位が衝突する可能性があるため、
        // 何度も鳴らないように hasImpacted フラグ管理が重要
        if (hasImpacted) return;

        if (collision.relativeVelocity.magnitude >= minImpactVelocity)
        {
            if (audioSource != null && impactSound != null)
                audioSource.PlayOneShot(impactSound);

            hasImpacted = true;
        }
    }
}