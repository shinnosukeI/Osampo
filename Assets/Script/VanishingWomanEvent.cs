using UnityEngine;
using System.Collections;

public class VanishingWomanEvent : MonoBehaviour
{
    [Header("消える女性オブジェクト")]
    [SerializeField] private GameObject womanObject;

    [Header("消えるまでの時間（秒）")]
    [SerializeField] private float vanishDelay = 2.0f;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearSound;
    [SerializeField] private AudioClip vanishSound;

    [Header("ノイズ演出用UI")]
    [SerializeField] private GameObject noiseEffectUI;
    [SerializeField] private float noiseDuration = 0.5f;

    [Header("照明演出")]
    [SerializeField] private Light[] targetLights; // 操作するライトのリスト
    [SerializeField] private float darknessDuration = 3.0f; // 暗転している時間
    [SerializeField] private float brightnessMultiplier = 2.0f; // 復帰時の明るさ倍率

    // 元の明るさを保持するリスト
    private float[] originalIntensities;

    void Start()
    {
        // デフォルトでは表示状態にする
        if (womanObject != null)
        {
            womanObject.SetActive(true);
        }
        
        // ノイズUIは非表示にしておく
        if (noiseEffectUI != null)
        {
            noiseEffectUI.SetActive(false);
        }

        // ライトの元の明るさを保存
        if (targetLights != null && targetLights.Length > 0)
        {
            originalIntensities = new float[targetLights.Length];
            for (int i = 0; i < targetLights.Length; i++)
            {
                if (targetLights[i] != null)
                {
                    originalIntensities[i] = targetLights[i].intensity;
                }
            }
        }
    }

    public void ActivateEvent()
    {
        StartCoroutine(EventSequence());
    }

    private IEnumerator EventSequence()
    {
        if (womanObject == null)
        {
            Debug.LogError("VanishingWomanEvent: 女性オブジェクトが設定されていません。");
            yield break;
        }

        Debug.Log("👻 消える女イベント開始");

        // 1. ノイズ音再生
        if (audioSource != null && vanishSound != null)
        {
            audioSource.PlayOneShot(vanishSound);
        }

        // 2. ノイズ映像ON
        if (noiseEffectUI != null)
        {
            noiseEffectUI.SetActive(true);
        }

        // 3. 少し待つ（ノイズが表示されている時間）
        yield return new WaitForSeconds(noiseDuration);

        // 4. 女性を消す ＆ ノイズも消す ＆ ライトを消す
        womanObject.SetActive(false);
        if (noiseEffectUI != null)
        {
            noiseEffectUI.SetActive(false);
        }

        // ライトOFF
        if (targetLights != null)
        {
            foreach (var light in targetLights)
            {
                if (light != null) light.enabled = false;
            }
        }
        Debug.Log("🌑 照明OFF");

        // 5. 暗闇で待機
        yield return new WaitForSeconds(darknessDuration);

        // 6. ライトを明るくしてON
        if (targetLights != null)
        {
            for (int i = 0; i < targetLights.Length; i++)
            {
                if (targetLights[i] != null)
                {
                    targetLights[i].enabled = true;
                    targetLights[i].intensity = originalIntensities[i] * brightnessMultiplier;
                }
            }
        }
        Debug.Log("💡 照明ON (増光)");

        Debug.Log("👻 イベント終了");
    }
}
