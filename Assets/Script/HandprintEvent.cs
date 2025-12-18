using UnityEngine;

public class HandprintEvent : MonoBehaviour
{
    [Header("手形オブジェクト（複数可）")]
    [SerializeField] private GameObject[] handprintObjects;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scareSound;

    void Awake()
    {
        // 初期状態では手形を非表示にする
        if (handprintObjects != null)
        {
            foreach (var obj in handprintObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    public void ActivateEvent()
    {
        Debug.Log("🖐️ 手形イベント発生！");

        // 手形を表示
        if (handprintObjects != null)
        {
            foreach (var obj in handprintObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // 音を再生
        if (audioSource != null && scareSound != null)
        {
            audioSource.PlayOneShot(scareSound);
        }
    }
}
