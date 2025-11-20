using UnityEngine;
using UnityEngine.UI; // Imageを使うため
using System.Collections; // コルーチンを使うため
using System; // Actionを使うため

/// <summary>
/// 画面のフェードイン・アウトを制御するスクリプト
/// </summary>
public class ScreenFader : MonoBehaviour
{
    [SerializeField]
    private Image fadePanel; // フェード用の黒いパネルをここに割り当てる

    [SerializeField]
    private float fadeDuration = 0.5f; // フェードにかかる時間 (秒)

    private bool isFading = false; // 現在フェード中かどうか

    // フェード完了時に外部に通知するイベント
    public event Action OnFadeOutComplete;
    public event Action OnFadeInComplete;

    void Awake()
    {
        // フェードパネルが設定されているか確認
        if (fadePanel == null)
        {
            Debug.LogError("ScreenFader: Fade Panelが割り当てられていません！");
            enabled = false; // スクリプトを無効にする
            return;
        }

        // 初期状態は透明にしておく
        Color color = fadePanel.color;
        color.a = 0f;
        fadePanel.color = color;
        fadePanel.gameObject.SetActive(false); // 初期は非表示
    }

    /// <summary>
    /// 画面を徐々に暗くする (フェードアウト)
    /// </summary>
    public void FadeOut(Action onComplete = null)
    {
        if (isFading) return; // フェード中なら何もしない
        isFading = true;
        fadePanel.gameObject.SetActive(true); // フェードパネルを表示
        StartCoroutine(PerformFade(0f, 1f, onComplete + OnFadeOutComplete));
    }

    /// <summary>
    /// 画面を徐々に明るくする (フェードイン)
    /// </summary>
    public void FadeIn(Action onComplete = null)
    {
        if (isFading) return; // フェード中なら何もしない
        isFading = true;
        fadePanel.gameObject.SetActive(true); // フェードパネルを表示
        StartCoroutine(PerformFade(1f, 0f, onComplete + OnFadeInComplete));
    }

    /// <summary>
    /// フェード処理の本体
    /// </summary>
    /// <param name="startAlpha">開始時の透明度 (0=透明, 1=不透明)</param>
    /// <param name="endAlpha">終了時の透明度</param>
    /// <param name="onComplete">フェード完了時に呼び出すコールバック</param>
    private IEnumerator PerformFade(float startAlpha, float endAlpha, Action onComplete)
    {
        float timer = 0f;
        Color color = fadePanel.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha; // 確実に最終値に設定
        fadePanel.color = color;

        isFading = false;

        // フェードイン完了時はパネルを非表示にする
        if (endAlpha == 0f)
        {
            fadePanel.gameObject.SetActive(false);
        }

        onComplete?.Invoke(); // 完了コールバックを呼び出す
    }
}