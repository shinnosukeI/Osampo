using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingCorpse : MonoBehaviour
{
    [Header("落下音設定")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float minImpactVelocity = 1.0f;

    private AudioSource audioSource;
    private Rigidbody rb;
    private bool hasImpacted = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        // ★ 最初は動かさない（イベントが来るまで落下禁止）
        rb.isKinematic = true;
    }

    // ★ EventManager から呼ばれる「落下開始」メソッド
    public void StartFalling()
    {
        if (rb.isKinematic == false) return; // すでに落下中なら無視

        Debug.Log("💀 FallingCorpse: 落下開始！");
        rb.isKinematic = false; // 重力ON
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted) return;

        if (collision.relativeVelocity.magnitude >= minImpactVelocity)
        {
            if (audioSource != null && impactSound != null)
            {
                audioSource.PlayOneShot(impactSound);
                Debug.Log("💀 死体が地面に衝突しました");
            }
            hasImpacted = true;
        }
    }
}