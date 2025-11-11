using UnityEngine;

// AudioSourceが必須であることを示す
[RequireComponent(typeof(AudioSource))] 
public class RadioAudio45 : MonoBehaviour
{
    [SerializeField]
    private AudioClip radioSoundClip; // インスペクターで設定するラジオの音 (ザザ...など)

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // 3Dサウンド（音がその場所から聞こえるようにする）
        // 0.0 = 2D (どこでも同じ音量), 1.0 = 3D (その場から聞こえる)
        audioSource.spatialBlend = 1.0f; 
        
        // 起動時には再生しない
        audioSource.playOnAwake = false;
        
        // 音をループさせる (ラジオの音はループすることが多いため)
        audioSource.loop = true;
    }

    // EventManagerから呼び出される公開メソッド
    public void PlayRadio()
    {
        // 既に再生中なら何もしない
        if (audioSource.isPlaying) return;

        if (radioSoundClip != null)
        {
            audioSource.clip = radioSoundClip;
            audioSource.Play();
            Debug.Log("📻 ラジオの再生を開始します。");
        }
        else
        {
            Debug.LogError("ラジオのオーディオクリップが設定されていません。");
        }
    }

    // (おまけ) イベントで音を止めたくなった時用のメソッド
    public void StopRadio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            Debug.Log("🔇 ラジオの再生を停止します。");
        }
    }
}