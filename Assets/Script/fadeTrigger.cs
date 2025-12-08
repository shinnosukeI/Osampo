using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeToWhiteTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager; // CycleCount を見る
    [SerializeField] private int requiredCycle = 6;               // 何周目から有効か

    [Header("遷移先シーン名")]
    [SerializeField] private string nextSceneName = "RestScene2";

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
{
    Debug.Log("[FadeTrigger] Enter: " + other.name);

    if (!other.CompareTag("Player"))
    {
        Debug.Log("[FadeTrigger] Playerじゃないので無視");
        return;
    }

    if (eventManager == null)
    {
        Debug.LogWarning("[FadeTrigger] eventManager 未設定");
        return;
    }

    Debug.Log("[FadeTrigger] CycleCount = " + eventManager.CycleCount);

    if (eventManager.CycleCount < requiredCycle)
    {
        Debug.Log("[FadeTrigger] まだ周回数が足りない");
        return;
    }

    Debug.Log("[FadeTrigger] フェード実行");

    if (FadeToWhiteManager.Instance != null)
    {
        FadeToWhiteManager.Instance.FadeToWhiteAndLoad(nextSceneName);
    }
    else
    {
        Debug.LogWarning("[FadeTrigger] FadeToWhiteManager.Instance が null");
        SceneManager.LoadScene(nextSceneName);
    }
}
}