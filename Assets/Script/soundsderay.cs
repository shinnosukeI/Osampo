using System.Collections;
using UnityEngine;

public class BreakSoundSync : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // 無ければ自動取得
    [SerializeField] private AudioClip breakClip;

    [Header("Timing")]
    [Tooltip("割れ演出を開始してから、音を鳴らすまでの遅延(秒)")]
    [SerializeField] private float playDelay = 0.0f;

    [Header("Options")]
    [SerializeField] private bool usePlayOneShot = true;
    [SerializeField] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private Coroutine co;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 割れ演出を開始したタイミングで呼ぶ（アニメ開始・オブジェクト破壊処理と同じ所に置く）
    /// </summary>
    public void TriggerBreakSound()
    {
        if (breakClip == null || audioSource == null) return;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(PlayAfterDelay());
    }

    private IEnumerator PlayAfterDelay()
    {
        if (playDelay > 0f) yield return new WaitForSeconds(playDelay);

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);

        if (usePlayOneShot)
        {
            audioSource.PlayOneShot(breakClip, volume);
        }
        else
        {
            audioSource.clip = breakClip;
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
}