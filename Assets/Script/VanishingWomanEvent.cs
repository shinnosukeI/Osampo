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

        // 4. 女性を消す ＆ ノイズも消す
        womanObject.SetActive(false);
        if (noiseEffectUI != null)
        {
            noiseEffectUI.SetActive(false);
        }

        Debug.Log("👻 女性が消えました...");
    }
}
