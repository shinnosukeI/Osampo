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

    void Start()
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
    }

    void OnTriggerEnter(Collider other)
    {
        // デバッグ用：何が当たったかログに出す
        Debug.Log($"🦶 [GlassStepSound] Hit Object: {other.name}, Tag: {other.tag}");

        // プレイヤーが踏んだ場合のみ
        if (other.CompareTag("Player"))
        {
            Debug.Log("🦶 [GlassStepSound] プレイヤーがガラスを踏みました");
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
                Debug.Log("🔇 [GlassStepSound] プレイヤーが離れたため音を停止しました");
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
                Debug.Log($"🔊 [GlassStepSound] ガラス音を再生: {clip.name}");
            }
        }
    }
}
