using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum FadeType
{
    Simple,
    Noise
}

public class ScreenFader : MonoBehaviour
{
    [SerializeField]
    private Image fadePanel;

    [Header("Settings")]
    [SerializeField]
    private float simpleFadeDuration = 0.5f;
    [SerializeField]
    private float noiseFadeDuration = 1.5f;

    [Header("Noise Material")]
    [SerializeField]
    private Material noiseFadeMaterialSource;

    private Material noiseFadeMaterialInstance;
    private bool isFading = false;
    private int cutoffPropertyId;

    void Awake()
    {
        if (fadePanel == null)
        {
            enabled = false;
            return;
        }

        cutoffPropertyId = Shader.PropertyToID("_Cutoff");

        if (noiseFadeMaterialSource != null)
        {
            noiseFadeMaterialInstance = Instantiate(noiseFadeMaterialSource);
        }

        // 初期状態は「真っ黒(Active)」
        fadePanel.gameObject.SetActive(true);
        
        // マテリアルをセットして真っ黒(0)にしておく
        if (noiseFadeMaterialInstance != null)
        {
            fadePanel.material = noiseFadeMaterialInstance;
            fadePanel.color = Color.black;
            // Shader修正により 0 = 黒
            noiseFadeMaterialInstance.SetFloat(cutoffPropertyId, 0.0f); 
        }
        else
        {
            fadePanel.material = null;
            fadePanel.color = Color.black; 
        }
    }

    public void FadeOut(FadeType type, Action onComplete = null)
    {
        if (isFading) return;
        isFading = true;
        fadePanel.gameObject.SetActive(true);

        if (type == FadeType.Simple)
        {
            fadePanel.material = null;
            Color c = Color.black;
            c.a = 0f; // 透明から
            fadePanel.color = c;
            StartCoroutine(PerformSimpleFade(0f, 1f, simpleFadeDuration, onComplete));
        }
        else
        {
            // Noise: 1(透明) -> 0(黒)
            // マテリアルがない場合はSimpleFadeにフォールバック
            if (noiseFadeMaterialInstance == null)
            {
                Debug.LogWarning("ScreenFader: Noise material missing, falling back to Simple fade.");
                fadePanel.material = null;
                Color c = Color.black;
                c.a = 0f;
                fadePanel.color = c;
                StartCoroutine(PerformSimpleFade(0f, 1f, simpleFadeDuration, onComplete));
                return;
            }

            fadePanel.material = noiseFadeMaterialInstance;
            fadePanel.color = Color.black;
            
            noiseFadeMaterialInstance.SetFloat(cutoffPropertyId, 1.0f); // 透明から

            // 1.0(透明) から 0.0(黒) へ減らす
            StartCoroutine(PerformNoiseFade(1.0f, 0.0f, noiseFadeDuration, onComplete));
        }
    }

    public void FadeIn(FadeType type, Action onComplete = null)
    {
        if (isFading) return;
        isFading = true;
        fadePanel.gameObject.SetActive(true);

        if (type == FadeType.Simple)
        {
            fadePanel.material = null;
            Color c = Color.black;
            c.a = 1f; // 黒から
            fadePanel.color = c;
            StartCoroutine(PerformSimpleFade(1f, 0f, simpleFadeDuration, () => {
                fadePanel.gameObject.SetActive(false);
                onComplete?.Invoke();
            }));
        }
        else
        {
            // Noise: 0(黒) -> 1(透明)
            // マテリアルがない場合はSimpleFadeにフォールバック
            if (noiseFadeMaterialInstance == null)
            {
                Debug.LogWarning("ScreenFader: Noise material missing, falling back to Simple fade.");
                fadePanel.material = null;
                Color c = Color.black;
                c.a = 1f;
                fadePanel.color = c;
                StartCoroutine(PerformSimpleFade(1f, 0f, simpleFadeDuration, () => {
                    fadePanel.gameObject.SetActive(false);
                    onComplete?.Invoke();
                }));
                return;
            }

            fadePanel.material = noiseFadeMaterialInstance;
            fadePanel.color = Color.black;

            noiseFadeMaterialInstance.SetFloat(cutoffPropertyId, 0.0f); // 黒から

            // 0.0(黒) から 1.0(透明) へ増やす
            StartCoroutine(PerformNoiseFade(0.0f, 1.0f, noiseFadeDuration, () => {
                fadePanel.gameObject.SetActive(false);
                onComplete?.Invoke();
            }));
        }
    }

    private IEnumerator PerformSimpleFade(float startAlpha, float endAlpha, float duration, Action onComplete)
    {
        float timer = 0f;
        Color c = Color.black;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = endAlpha;
        fadePanel.color = c;
        isFading = false;
        onComplete?.Invoke();
    }

    private IEnumerator PerformNoiseFade(float startVal, float endVal, float duration, Action onComplete)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float val = Mathf.Lerp(startVal, endVal, timer / duration);
            
            // 安全策: マテリアルが途中で消えた場合
            if (noiseFadeMaterialInstance != null)
            {
                noiseFadeMaterialInstance.SetFloat(cutoffPropertyId, val);
            }
            else
            {
                break; // ループを抜けて終了処理へ
            }
            yield return null;
        }
        
        if (noiseFadeMaterialInstance != null)
        {
            noiseFadeMaterialInstance.SetFloat(cutoffPropertyId, endVal);
        }

        isFading = false;
        onComplete?.Invoke();
    }
}