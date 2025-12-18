using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeToWhiteManager : MonoBehaviour
{
    public static FadeToWhiteManager Instance { get; private set; }

    [Header("フェード用の白画像")]
    [SerializeField] private Image fadeImage;

    [Header("フェードにかかる時間")]
    [SerializeField] private float fadeDuration = 2.0f;

    private bool isFading = false;

    private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    if (fadeImage == null)
        fadeImage = GetComponent<Image>();  // ← これ追加するとさらに確実

    if (fadeImage != null)
    {
        Color c = fadeImage.color;
        c.a = 0f; // 最初は透明
        fadeImage.color = c;
    }
}

    public void FadeToWhiteAndLoad(string sceneName)
    {
        Debug.Log("[FadeManager] FadeToWhiteAndLoad 呼ばれた");

        if (isFading)
        {
            Debug.Log("[FadeManager] すでにフェード中");
            return;
        }

        if (fadeImage == null)
        {
            Debug.LogError("[FadeManager] fadeImage が設定されていません！");
            // 保険でそのままシーン遷移だけは行う
            SceneManager.LoadScene(sceneName);
            return;
        }

        StartCoroutine(FadeCoroutine(sceneName));
    }

    private IEnumerator FadeCoroutine(string sceneName)
    {
        isFading = true;

        float t = 0f;
        Color c = fadeImage.color;

        Debug.Log("[FadeManager] フェード開始");

        while (t < fadeDuration)
        {
            // TimeScale が 0 でも動くように unscaledDeltaTime を使用
            t += Time.unscaledDeltaTime;

            float a = Mathf.Clamp01(t / fadeDuration);
            c.a = a;
            fadeImage.color = c;

            yield return null;
        }

        Debug.Log("[FadeManager] フェード完了 → シーン遷移: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}