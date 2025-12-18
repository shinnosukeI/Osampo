using UnityEngine;

public class Footstep : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;
    public float moveThreshold = 0.1f;

    private float stepTimer = 0f;
    private CharacterController cc;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float speed = (cc != null) ? cc.velocity.magnitude : 0f;

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
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)], 1f);
    }
}