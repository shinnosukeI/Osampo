using UnityEngine;

[RequireComponent(typeof(ParticleSystem))] 
[RequireComponent(typeof(AudioSource))] 
public class CockroachSwarm : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private AudioSource audioSource;

    [Header("再生する音")]
    [SerializeField] private AudioClip swarmSound; 

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Stop(); 

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void StartSwarm()
    {
        if (particleSystem.isPlaying) return;

        // 1. Force Position to Floor
        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, 0.05f, pos.z);
        
        // 2. Reset Rotation to (0,0,0)
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // 3. Fix Settings
        var main = particleSystem.main;
        var collision = particleSystem.collision;
        var shape = particleSystem.shape;

        main.gravityModifier = 0.0f; 
        main.startSpeed = 0.5f; // ★ Reduced Speed (User Request: Too Fast -> 0.5)
        main.simulationSpace = ParticleSystemSimulationSpace.World; 

        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = 0.0f;
        collision.bounce = 0.0f;
        
        // ★ Disable Local Velocity (caused stopping)
        var vel = particleSystem.velocityOverLifetime;
        vel.enabled = false;
        
        // ★ Shape: Circle (Emits outward from center = Random Directions)
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle; // Use Circle for outward spread
        shape.radiusThickness = 0f; // Emit from edge? or Volume? Let's use Volume(1) or Edge(0)
                                    // Default is usually Volume, let's try Volume(1) to fill area
        shape.radiusThickness = 1.0f; 
        shape.rotation = new Vector3(90, 0, 0); // Flat on floor (Circle lies on XZ)

        // ★ THE KEY: Align particle rotation to its travel direction
        shape.alignToDirection = true; 
        // Note: This aligns the particle's Local Z to Velocity.
        // If mesh needs -90 X, we handle that in StartRotation.

        // 2. Fix Rotation
        main.startRotation3D = true;
        // Adjust these to ensure Mesh aligns with Velocity
        // Usually if Mesh comes "Flat" (-90 X), we need to maintain that.
        // AlignToDirection adds rotation on top.
        main.startRotationX = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad); // Keep Flat
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f); // Align module handles Y
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f); 

        // ★ FORCE FIX 2: Visibility (Looping & Emission)
        main.loop = true; 
        main.startSize = 1.0f; // Size 1.0
        main.maxParticles = 1000;

        var emission = particleSystem.emission;
        emission.enabled = true;
        emission.rateOverTime = 50f; 

        // Disable dynamic rotation
        var rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = false;

        Debug.Log("🪳 11: ゴキブリイベント発動！(修正版: GlobalSpeed+AlignToDirection)");
        particleSystem.Play();

        // ★ Play Sound (3D)
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1.0f; 
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic; 
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 15.0f; 
            audioSource.loop = true; 

            if (swarmSound != null)
            {
                audioSource.clip = swarmSound;
            }

            if (audioSource.clip != null)
            {
                audioSource.Play();
                Debug.Log("🔊 ゴキブリの音を再生開始 (3D)");
            }
        }
    }

    public void StopSwarm()
    {
        if (particleSystem.isPlaying)
        {
            // ★ Stop logic: Clear immediately
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystem.Clear(); // Double check
            Debug.Log("ゴキブリが消えました（即時削除）。");
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}