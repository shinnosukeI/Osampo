using UnityEngine;

public class RadioTriggerZone : MonoBehaviour
{
    [SerializeField] private RadioEventController radioController;

    [Header("○周目以降で作動させる")]
    [SerializeField] private int requiredCycle = 2; // 例：2周目で発動

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (used) return;

        Debug.Log("[RadioTrigger] プレイヤーがトリガー領域に侵入");

        // 周回数チェック（EventManager を経由）
        st1_HorrorEventManager em = FindObjectOfType<st1_HorrorEventManager>();
        if (em == null)
        {
            Debug.LogError("[RadioTrigger] EventManager が見つかりません");
            return;
        }

        Debug.Log($"[RadioTrigger] 現在周回: {em.CycleCount}");

        if (em.CycleCount >= requiredCycle)
        {
            Debug.Log("[RadioTrigger] 周回条件を満たしたためラジオ再生開始");

            radioController.PlayRadioSequence();
            used = true;   // ★ 1回だけ発動
        }
        else
        {
            Debug.Log("[RadioTrigger] 周回条件未満のため発動しない");
        }
    }
}
