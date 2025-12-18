using UnityEngine;
using UnityEngine.Audio;

public class RadioAudio45 : MonoBehaviour
{
    [SerializeField] private AudioClip radioSoundClip;
    [SerializeField] private AudioMixer audioMixer; // ★ ミキサーを受け取る
    private AudioSource audioSource;

    private float bgmBefore;
    private float sfxBefore;

    void Awake()
    {
    audioSource = GetComponent<AudioSource>();

    audioSource.spatialBlend = 1.0f;
    audioSource.playOnAwake = false;
    audioSource.loop = true;

    // ★ 最初は絶対鳴らさないようにクリップを外す
    audioSource.clip = null;
    }

    public void PlayRadio()
    {
        if (audioSource.isPlaying) return;

        if (radioSoundClip != null)
        {
            audioSource.clip = radioSoundClip;
            audioSource.Play();

            // ★ BGM/SFX の音量を下げる（ミュート）
            audioMixer.GetFloat("BGMVolume", out bgmBefore);
            audioMixer.GetFloat("SFXVolume", out sfxBefore);

            audioMixer.SetFloat("BGMVolume", -80f); // ミュート
            audioMixer.SetFloat("SFXVolume", -80f);

        }
        else
        {
            Debug.LogError("ラジオのオーディオクリップが設定されていません。");
        }
    }

    public void StopRadio()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();

            // ★ 元の音量に戻す
            audioMixer.SetFloat("BGMVolume", bgmBefore);
            audioMixer.SetFloat("SFXVolume", sfxBefore);

            
        }
    }
}