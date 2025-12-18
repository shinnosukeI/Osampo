using UnityEngine;

public class GlassStepSound : MonoBehaviour
{
    [Header("ガラスを踏む音（複数設定可）")]
    [SerializeField]
    private AudioClip[] glassSounds;

    [Header("オーディオソース（空なら自動追加）")]
    [SerializeField]
    private AudioSource audioSource;

    [Header("音量")]
    [SerializeField]
    [Range(0f, 1f)]
    private float volume = 1.0f;

    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 3Dサウンド

        // ★ 再修正：StartではなくAwakeで即座に行う
        // IsTriggerにするのではなく、Colliderコンポーネントそのものを削除（Destroy）して
        // 物理エンジンの計算負荷を完全になくす。
        Collider[] childColliders = GetComponentsInChildren<Collider>();
        foreach (var col in childColliders)
        {
            // 自分自身（親のトリガー）は除外
            if (col.gameObject == this.gameObject) continue;

            // コンポーネントごと削除
            Destroy(col);
        }

        // ★ 音声データの先行ロード（ラグ対策）
        if (glassSounds != null)
        {
            foreach (var clip in glassSounds)
            {
                if (clip != null)
                {
                    clip.LoadAudioData();
                }
            }
        }
    }

    void Start()
    {
        // ★ さらにダメ押し：無音で一度再生して、オーディオエンジンにキャッシュさせる（ウォームアップ）
        if (glassSounds != null && glassSounds.Length > 0 && glassSounds[0] != null)
        {
            audioSource.PlayOneShot(glassSounds[0], 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ログ削除（処理負荷軽減）
        
        // プレイヤーが踏んだ場合のみ
        if (other.CompareTag("Player"))
        {
            PlayRandomGlassSound();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // プレイヤーが離れたら音を止める
        if (other.CompareTag("Player"))
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlayRandomGlassSound()
    {
        if (glassSounds != null && glassSounds.Length > 0)
        {
            int index = Random.Range(0, glassSounds.Length);
            AudioClip clip = glassSounds[index];
            
            if (clip != null)
            {
                // PlayOneShotなら連続で踏んでも音が重なって自然に聞こえる
                audioSource.PlayOneShot(clip, volume);
            }
        }
    }
}
