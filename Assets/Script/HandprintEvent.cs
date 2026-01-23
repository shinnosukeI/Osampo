using UnityEngine;

public class HandprintEvent : MonoBehaviour
{
    [Header("手形オブジェクト（複数可）")]
    [SerializeField] private GameObject[] handprintObjects;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scareSound;

    void Awake()
    {
        // 初期状態では手形を非表示にしつつ、マテリアル設定を修正して透明部分を目立たなくする
        if (handprintObjects != null)
        {
            foreach (var obj in handprintObjects)
            {
                if (obj != null)
                {
                    // ★ 透明部分のテカリ/影対策
                    var renderers = obj.GetComponentsInChildren<Renderer>();
                    foreach (var r in renderers)
                    {
                        // 影を落とさない（四角い影が出るのを防ぐ）
                        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        r.receiveShadows = false; // 影を受けない（壁の影で暗くなりすぎないように）

                        // マテリアルのテカリを消す
                        foreach (var mat in r.materials)
                        {
                            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0f); // Smoothness 0
                            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0f);     // Metallic 0
                            
                            // スペキュラーハイライトと反射を無効化
                            mat.DisableKeyword("_SPECULARHIGHLIGHTS_OFF"); 
                            mat.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
                            mat.SetFloat("_SpecularHighlights", 0f);

                            mat.DisableKeyword("_GLOSSYREFLECTIONS_OFF");
                            mat.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
                            mat.SetFloat("_GlossyReflections", 0f);
                        }
                    }
                    
                    obj.SetActive(false);
                }
            }
        }
    }

    public void ActivateEvent()
    {
        // Debug.Log("🖐️ 手形イベント発生！");

        // 手形を表示
        if (handprintObjects != null)
        {
            foreach (var obj in handprintObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // 音を再生
        if (audioSource != null && scareSound != null)
        {
            audioSource.PlayOneShot(scareSound);
        }
    }
}
