using UnityEngine;
using System.Collections;

public class DoorRotate180to90Trigger : MonoBehaviour
{
    [Header("対象ドア")]
    public Transform targetDoor;

    [Header("回転設定")]
    public float targetAngleY = 90f;         // 戻したい角度
    public float checkAngleY = 180f;         // 判定する角度
    public float angleTolerance = 5f;        // 誤差許容
    public float rotateSpeed = 2f;           // 回転速度

    [Header("閉まる音（ドアから鳴らす）")]
    [SerializeField] private AudioClip closeSE;
    [SerializeField] private float seVolume = 1f;

    private Coroutine rotateCoroutine;

    // ★ ドア（targetDoor）側のAudioSource
    private AudioSource doorAudioSource;

    private void Awake()
    {
        SetupDoorAudioSource();
    }

    private void SetupDoorAudioSource()
    {
        if (targetDoor == null) return;

        // ★ 音源は必ず「閉めるドア」に付ける
        doorAudioSource = targetDoor.GetComponent<AudioSource>();
        if (doorAudioSource == null)
            doorAudioSource = targetDoor.gameObject.AddComponent<AudioSource>();

        doorAudioSource.playOnAwake = false;
        doorAudioSource.loop = false;

        // 好みで：ドアから鳴ってる感が欲しいなら 1f（3D）
        // 確実に聞かせたいなら 0f（2D）
        // doorAudioSource.spatialBlend = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (targetDoor == null) return;

        // targetDoorが後から差し替えられても対応
        if (doorAudioSource == null) SetupDoorAudioSource();

        // DoorController の上書き防止
        var controller = targetDoor.GetComponent<DoorController>();
        if (controller != null) controller.enabled = false;

        float currentY = NormalizeAngle(targetDoor.localEulerAngles.y);

        // ★ 180度付近なら 90度へ戻す
        if (Mathf.Abs(currentY - checkAngleY) <= angleTolerance)
        {
            if (rotateCoroutine != null)
                StopCoroutine(rotateCoroutine);

            rotateCoroutine = StartCoroutine(RotateToAngle(targetAngleY));
        }
    }

    private IEnumerator RotateToAngle(float targetY)
    {
        // ★ 回転開始時に「閉まる音」を鳴らす（ドアから）
        PlayCloseSE();

        Quaternion startRot = targetDoor.localRotation;
        Quaternion endRot = Quaternion.Euler(
            targetDoor.localEulerAngles.x,
            targetY,
            targetDoor.localEulerAngles.z
        );

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;
            targetDoor.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        targetDoor.localRotation = endRot;
        rotateCoroutine = null;
    }

    private void PlayCloseSE()
    {
        if (closeSE == null) return;
        if (targetDoor == null) return;

        if (doorAudioSource == null) SetupDoorAudioSource();
        if (doorAudioSource == null) return;

        doorAudioSource.PlayOneShot(closeSE, Mathf.Clamp01(seVolume));
    }

    // 0〜360 → -180〜180 に正規化
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
