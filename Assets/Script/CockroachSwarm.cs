using UnityEngine;

[RequireComponent(typeof(ParticleSystem))] 
public class CockroachSwarm : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystem.Stop(); 
    }

    public void StartSwarm()
    {
        if (particleSystem.isPlaying) return;

        Debug.Log("🪳 大量のゴキブリが出現します！");
        particleSystem.Play();
    }

    public void StopSwarm()
    {
        if (particleSystem.isPlaying)
        {
            particleSystem.Stop();
            Debug.Log("ゴキブリが消えました。");
        }
    }
}