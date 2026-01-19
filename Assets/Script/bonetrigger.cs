using UnityEngine;

[RequireComponent(typeof(AudioSource))] // ★ 追加
public class BoneTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("何周目で発動させるか")]
    [SerializeField] private int targetCycle = 4;

    [Header("ログ用イベントID（骸骨）")]
    [SerializeField] private int boneEventID = 23;

    [Header("天井アンカー")]
    [SerializeField] private Transform hangAnchor;

    [Header("骸骨（Prefab）")]
    [SerializeField] private HangingSkull skullPrefab;

    [Header("一度きり")]
    [SerializeField] private bool onlyOnce = true;

    [Header("発動音")]
    [SerializeField] private AudioClip triggerSound;     // ★ 追加
    [SerializeField] private float triggerVolume = 1.0f; // ★ 追加
    [SerializeField] private bool playAtAnchor = true;   // ★ 追加（アンカー位置で鳴らすか）

    private bool triggered = false;

    private AudioSource audioSource; // ★ 追加

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();

        audioSource = GetComponent<AudioSource>(); // ★ 追加
        audioSource.playOnAwake = false;

        // 3D音にしたいならON（好みで）
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 12f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (onlyOnce && triggered) return;

        if (eventManager == null || hangAnchor == null || skullPrefab == null)
        {
            Debug.LogError("[BoneTrigger] 参照が不足しています");
            return;
        }

        int current = eventManager.CycleCount;
        if (current != targetCycle) return;

        triggered = true;

        // ★ ログ
        eventManager.LogOnly(boneEventID);

        // ★ 先に音（発動瞬間）
        PlayTriggerSound();

        // ★ 骸骨生成
        HangingSkull skull = Instantiate(skullPrefab);
        skull.gameObject.SetActive(true);

        // ★ 吊って揺らす
        skull.HangAndSwing(hangAnchor);

        Debug.Log($"☠️ [BoneTrigger] Cycle={current} 骸骨イベント発生（ID:{boneEventID}）");
    }

    private void PlayTriggerSound()
    {
        if (triggerSound == null) return;

        // アンカー位置で鳴らしたい（上から鳴る感じ）
        if (playAtAnchor && hangAnchor != null)
        {
            AudioSource.PlayClipAtPoint(triggerSound, hangAnchor.position, triggerVolume);
        }
        else
        {
            // トリガー位置で鳴らす
            audioSource.PlayOneShot(triggerSound, triggerVolume);
        }
    }
}
