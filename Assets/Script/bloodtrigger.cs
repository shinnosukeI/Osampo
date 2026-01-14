using UnityEngine;

public class BloodTrigger : MonoBehaviour
{
    [Header("ログ管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("血イベントID")]
    [SerializeField] private int bloodEventID = 60;

    [Header("有効な周回数（固定）")]
    [SerializeField] private int targetCycle = 6;

    [Header("プレイヤータグ")]
    [SerializeField] private string playerTag = "Player";

    [Header("一度きりにする")]
    [SerializeField] private bool onlyOnce = true;

    private bool hasLogged = false;

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (onlyOnce && hasLogged) return;

        if (eventManager == null)
        {
            Debug.LogError("[BloodTrigger] EventManager が見つかりません");
            return;
        }

        int currentCycle = eventManager.CycleCount;

        // ★ 6周目以外は無視
        if (currentCycle != targetCycle)
        {
            Debug.Log($"[BloodTrigger] 周回 {currentCycle} → 対象外（必要: {targetCycle}）");
            return;
        }

        Debug.Log($"🩸 [BloodTrigger] 周回 {currentCycle} → 血イベント記録（ID:{bloodEventID}）");

        // ★ ログだけ記録
        eventManager.LogOnly(bloodEventID);

        hasLogged = true;
    }
}