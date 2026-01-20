using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorSoundPlayer : MonoBehaviour
{
    [SerializeField] private DoorController door;
    [SerializeField] private AudioClip openSE;
    [SerializeField] private AudioClip closeSE;
    [SerializeField] private float volume = 1f;

    [Header("判定のゆるさ(度)")]
    [SerializeField] private float arrivedAngleEpsilon = 1.0f;

    private AudioSource audioSource;

    private bool lastIsOpen;
    private bool waitingCloseArrive = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (door == null)
            door = GetComponent<DoorController>();   // ★同じオブジェクトから自動取得

        if (door == null)
        {
            Debug.LogError("DoorSoundPlayer: DoorController が見つかりません！");
            enabled = false;
            return;
        }

        lastIsOpen = door.IsOpen;
    }

    void Update()
    {
        if (!enabled) return;

        if (door.IsOpen != lastIsOpen)
        {
            if (door.IsOpen)
            {
                Play(openSE);               // 開き始め
            }
            else
            {
                waitingCloseArrive = true;  // 閉じきり待ち
            }

            lastIsOpen = door.IsOpen;
        }

        if (waitingCloseArrive && Arrived(transform.localRotation, door.ClosedRotation))
        {
            Play(closeSE);                 // 閉じきり
            waitingCloseArrive = false;
        }
    }

    private bool Arrived(Quaternion current, Quaternion target)
    {
        return Quaternion.Angle(current, target) <= arrivedAngleEpsilon;
    }

    private void Play(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }
}