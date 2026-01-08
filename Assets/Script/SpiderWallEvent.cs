using UnityEngine;

public class SpiderWallEvent : MonoBehaviour
{
    [Header("蜘蛛オブジェクト（複数可）")]
    [SerializeField] private GameObject[] spiderObjects;

    [Header("イベント発生時に消すオブジェクト（汚れのない壁など）")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearSound;

    void Awake()
    {
        // 初期状態の制御が必要ならここで記述
        // 基本的に HorrorEventManager の InitializeObjectStates で非表示にされる想定
    }

    public void ActivateEvent()
    {
        Debug.Log("🕷️ [SpiderWallEvent] 蜘蛛イベント発生！");

        // 蜘蛛を表示
        if (spiderObjects != null)
        {
            foreach (var obj in spiderObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // 音を再生
        if (audioSource != null && appearSound != null)
        {
            audioSource.PlayOneShot(appearSound);
        }
    }

    // 周回開始時に呼ばれる準備メソッド
    public void PrepareEvent()
    {
        // 指定したオブジェクト（綺麗な壁など）を非表示にする
        if (objectsToHide != null)
        {
            foreach (var obj in objectsToHide)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
}
