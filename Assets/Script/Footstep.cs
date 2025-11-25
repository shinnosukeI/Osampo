
using UnityEngine;

public class Footstep : MonoBehaviour
{
    public CharacterController controller;
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;

    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    void Update()
    {
        // 動いているか判定
        if (controller.isGrounded && controller.velocity.magnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer > stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
    }

    void PlayFootstep()
    {
        audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.Play();
    }
}