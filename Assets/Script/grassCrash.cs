using UnityEngine;

public class GlassBreakTrigger : MonoBehaviour
{
    [Header("周回管理")]
    [SerializeField] private st1_HorrorEventManager eventManager;

    [Header("ガラス本体（Renderer）")]
    [SerializeField] private Renderer glassRenderer;
    [SerializeField] private Animation glassAnimation;

    [Header("非表示にしたいオブジェクト（55全体）")]
    [SerializeField] private GameObject objectToHide;

    [Header("何周目から有効にするか")]
    [SerializeField] private int enableCycle = 5;

    private bool hasBroken = false;

    private void Awake()
    {
        if (eventManager == null)
            eventManager = FindObjectOfType<st1_HorrorEventManager>();
    }

    private void Reset()
    {
        if (glassRenderer == null)
            glassRenderer = GetComponentInChildren<Renderer>();
        if (glassAnimation == null)
            glassAnimation = GetComponentInChildren<Animation>();
    }

    private void Start()
    {
        // 最初は不可視
        if (objectToHide != null)
            objectToHide.SetActive(false);

        if (glassAnimation != null)
            glassAnimation.playAutomatically = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBroken) return;
        if (!other.CompareTag("Player")) return;
        if (eventManager == null)
        {
            Debug.LogError("[GlassBreakTrigger] eventManager が見つかりません");
            return;
        }
        if (eventManager.CycleCount < enableCycle) return;

        hasBroken = true;

        // ★ ログだけ残す（演出はこのスクリプトでやる）
        eventManager.LogOnly(55);

        // 55 のPrefab全体を表示
        if (objectToHide != null)
            objectToHide.SetActive(true);

        // ガラスrendererも ON
        if (glassRenderer != null)
            glassRenderer.enabled = true;

        // アニメ再生
        if (glassAnimation != null)
            glassAnimation.Play();
    }
}