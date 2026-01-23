using UnityEngine;

public class Footstep : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;

    public float stepInterval = 0.5f;
    public float moveThreshold = 0.05f;

    [Range(0f, 1f)] public float volume = 1.0f; // ★ 音量追加

    private float stepTimer = 0f;
    private CharacterController cc;
    private Vector3 lastPos;
    private bool isMoving = false;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        cc = GetComponent<CharacterController>();
        lastPos = transform.position;

        // ★ ループはしない（1歩ずつ鳴らす）
        if (audioSource != null) audioSource.loop = false;
    }

    void Update()
    {
        float speed;

        if (cc != null)
        {
            speed = cc.velocity.magnitude;
            if (!cc.isGrounded)
            {
                PauseFootstep();
                return;
            }
        }
        else
        {
            Vector3 delta = transform.position - lastPos;
            speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        }

        lastPos = transform.position;

        if (speed > moveThreshold)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }

            isMoving = true;
        }
        else
        {
            // ★ 止まったら即一時停止
            PauseFootstep();
            stepTimer = 0f;
            isMoving = false;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        if (audioSource == null) return;

        // 再生中なら新しく鳴らさない
        if (audioSource.isPlaying) return;

        audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = volume; // ★ 音量適用
        audioSource.Play();
    }

    void PauseFootstep()
    {
        if (audioSource.isPlaying)
            audioSource.Pause();
    }
}
