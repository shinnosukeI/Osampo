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

    

        // 周回数チェック（EventManager を経由）
        st1_HorrorEventManager em = FindObjectOfType<st1_HorrorEventManager>();
        if (em == null)
        {
            Debug.LogError("[RadioTrigger] EventManager が見つかりません");
            return;
        }

        

        if (em.CycleCount >= requiredCycle)
        {
            

            radioController.PlayRadioSequence();
            used = true;   // ★ 1回だけ発動
        }
        else
        {
            Debug.Log("[RadioTrigger] 周回条件未満のため発動しない");
        }
    }
}
