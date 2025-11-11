using UnityEngine;

// 必要なコンポーネントを自動で追加する
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class FallingObjectAudio : MonoBehaviour
{
    [Header("オーディオクリップ")]
    [SerializeField]
    private AudioClip impactSound; // 落下時の「ドン！」という音
    
    // ※ RollingSound と Update() は転がらないので不要になりました

    private Rigidbody rb;
    private AudioSource audioSource;
    
    private bool hasImpacted = false; // 最初に衝突したかを判定するフラグ

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // 起動時は物理演算を切っておく
        if (rb != null)
        {
            rb.isKinematic = true; 
            
            // ★★★ ここが重要 ★★★
            // 物理演算による回転を「すべて」凍結（Freeze）する
            // これでUnityエディタの設定に関わらず、絶対に転がらなくなります。
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    // EventManagerから呼び出される公開メソッド
    public void StartFall()
    {
        if (rb == null) return;
        
        // 落下開始時の姿勢をリセット（まっすぐ落とすため）
        transform.rotation = Quaternion.identity; 
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // 物理演算を開始して落下させる
        rb.isKinematic = false;
        hasImpacted = false; // 落下開始時にフラグをリセット
        Debug.Log($"🪣 {this.gameObject.name} の落下を開始。");
    }

    // 1. 床や壁に「衝突した瞬間」に呼ばれる
    void OnCollisionEnter(Collision collision)
    {
        // 最初の1回目
        if (!hasImpacted)
        {
            // 衝突の強さが一定以上なら
            if (collision.relativeVelocity.magnitude > 0.5f) 
            {
                // 1. 衝撃音を再生
                audioSource.PlayOneShot(impactSound);
                hasImpacted = true; // 衝突フラグを立てる
                
                // --- 転がりに関する処理 (AddTorque, rollingSound) はすべて削除 ---
            }
        }
    }

    // 2. 「Update()」メソッド
    // 転がり音を再生する必要がなくなったため、Update()メソッド自体が不要です。
    // void Update() { ... }
}