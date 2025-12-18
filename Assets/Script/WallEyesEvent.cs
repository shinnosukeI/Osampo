using UnityEngine;

public class WallEyesEvent : MonoBehaviour
{
    [Header("目玉オブジェクト（複数可）")]
    [Tooltip("LookAtPlayerスクリプトをアタッチして、プレイヤーを見るように設定してください")]
    [SerializeField] private GameObject[] eyeObjects;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearSound;

    void Awake()
    {
        // 初期状態では「プレイヤーを見る」動きを止めておく（表示はそのまま）
        if (eyeObjects != null)
        {
            foreach (var obj in eyeObjects)
            {
                if (obj != null)
                {
                    var lookScript = obj.GetComponent<LookAtPlayer>();
                    if (lookScript != null)
                    {
                        lookScript.enabled = false;
                    }
                }
            }
        }
    }

    public void ActivateEvent()
    {
        Debug.Log("👁️ 壁に目イベント発生！");

        // 「プレイヤーを見る」動きを開始
        if (eyeObjects != null)
        {
            foreach (var obj in eyeObjects)
            {
                if (obj != null)
                {
                    var lookScript = obj.GetComponent<LookAtPlayer>();
                    if (lookScript != null)
                    {
                        lookScript.enabled = true;
                    }
                }
            }
        }

        // 音を再生
        if (audioSource != null && appearSound != null)
        {
            audioSource.PlayOneShot(appearSound);
        }
    }
}
