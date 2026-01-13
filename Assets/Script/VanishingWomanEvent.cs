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

        // 4. 女性を消す (ノイズはまだ残す?) -> 画像乱れ中に消えるのが自然なので
        womanObject.SetActive(false);
        
        Debug.Log("👻 女性消失 -> 音量フェード開始");

        // ★ 音量フェード（消えた瞬間から照明が消えるまで大きくする）
        if (audioSource != null && vanishSound != null)
        {
            audioSource.clip = vanishSound;
            audioSource.volume = 0f;
            audioSource.Play();
        }

        // 指定時間（vanishDelay）かけて音量を上げつつ待機
        float elapsed = 0f;
        while (elapsed < vanishDelay)
        {
            elapsed += Time.deltaTime;
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Lerp(0f, 1f, elapsed / vanishDelay);
            }
            yield return null;
        }

        // 5. 照明OFF ＆ ノイズOFF ＆ 音停止
        if (noiseEffectUI != null)
        {
            noiseEffectUI.SetActive(false);
        }

        if (targetLights != null)
        {
            foreach (var light in targetLights)
            {
                if (light != null) light.enabled = false;
            }
        }
        if (audioSource != null)
        {
             audioSource.Stop();
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
