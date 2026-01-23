using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RollingBall56 : MonoBehaviour
{
    [Header("転がる強さ")]
    [SerializeField]
    private float rollForce = 5.0f; // 数字が大きいほど速く転がる

    [Header("効果音")]
    [SerializeField]
    private AudioClip rollSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1.0f; // ★ 音量調整
    [SerializeField] private float minDistance = 1.0f; // ★ 減衰開始距離
    [SerializeField] private float maxDistance = 20.0f; // ★ 音が聞こえなくなる距離（Linearの場合）

    private AudioSource audioSource;

    void Start()
    {
        // 自動では転がらないようにする
    }

    // 外部（HorrorEventManager）から呼ばれたときに転がり始める
    public void StartRoll()
    {
        // Debug.Log("🌑 [RollingBall56] StartRoll called.");

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * rollForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * rollForce, ForceMode.Impulse);

            // 音を鳴らす
            if (rollSound != null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                
                audioSource.clip = rollSound;
                audioSource.volume = volume;
                audioSource.spatialBlend = 1.0f; // 3D音響
                
                // ★ 減衰設定 (Linearに変更して制御しやすくする)
                audioSource.minDistance = minDistance;
                audioSource.maxDistance = maxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                
                audioSource.Play();
                // Debug.Log($"🔊 [RollingBall56] Audio Played. Vol:{volume}, Min:{minDistance}, Max:{maxDistance}");
            }
            else
            {
                Debug.LogError("❌ [RollingBall56] Roll Sound is NOT assigned!");
            }
        }
        else
        {
             Debug.LogError("❌ [RollingBall56] Rigidbody missing!");
        }
    }
}