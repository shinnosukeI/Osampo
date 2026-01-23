using UnityEngine;

public class AudioSourceDebugger : MonoBehaviour
{
    void Update()
    {
        int count = FindObjectsOfType<AudioSource>().Length;
        Debug.Log($"[AudioDebug] AudioSource count = {count}");
    }
}