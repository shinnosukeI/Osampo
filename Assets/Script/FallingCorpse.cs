using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingCorpse : MonoBehaviour
{
    [Header("落下音設定")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float minImpactVelocity = 1.0f; // 音が鳴る最小の衝突速度

    private AudioSource audioSource;
    private bool hasImpacted = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // すでに音が鳴っていたら何もしない（または、跳ねるたびに鳴らしたい場合はここを調整）
        if (hasImpacted) return;

        // 衝突の強さをチェック
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
