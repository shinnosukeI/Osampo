using UnityEngine;
public class Footstep : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;
    public float moveThreshold = 0.1f; // どれくらい動いたら足音を鳴らすか

    private float stepTimer = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // 一定以上動いていたら
        if (speed > moveThreshold)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer > stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f; // 止まったらタイマーリセットしてもOK
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;

        audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.Play();
    }
} 