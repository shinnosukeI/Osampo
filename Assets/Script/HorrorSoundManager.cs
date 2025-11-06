using UnityEngine;
using System.Collections.Generic;

public class HorrorSoundManager : MonoBehaviour
{
    public static HorrorSoundManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string id;          // 例: "impact_fall", "whisper", "blood_drop"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("登録されたサウンド一覧")]
    public List<SoundEntry> sounds = new List<SoundEntry>();

    private Dictionary<string, SoundEntry> soundMap = new Dictionary<string, SoundEntry>();
    private AudioSource audioSource3D;
    private AudioSource audioSource2D;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // マップ構築
        foreach (var s in sounds)
        {
            if (!soundMap.ContainsKey(s.id))
                soundMap.Add(s.id, s);
        }

        // 2D/3D AudioSource セットアップ
        audioSource2D = gameObject.AddComponent<AudioSource>();
        audioSource2D.spatialBlend = 0f; // UI・環境音

        audioSource3D = gameObject.AddComponent<AudioSource>();
        audioSource3D.spatialBlend = 1f; // 効果音（位置付き）
    }

    // 📢 2Dサウンド（UI音・環境音など）
    public void Play2D(string id)
    {
        if (soundMap.TryGetValue(id, out var entry))
        {
            audioSource2D.PlayOneShot(entry.clip, entry.volume);
        }
        else
        {
            Debug.LogWarning($"❌ サウンドID '{id}' が見つかりません。");
        }
    }

    // 📢 3Dサウンド（空間位置で鳴る）
    public void Play3D(string id, Vector3 position)
    {
        if (soundMap.TryGetValue(id, out var entry))
        {
            AudioSource.PlayClipAtPoint(entry.clip, position, entry.volume);
        }
        else
        {
            Debug.LogWarning($"❌ サウンドID '{id}' が見つかりません。");
        }
    }
}
