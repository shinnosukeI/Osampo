using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip titleBGM; // インスペクタで設定するBGM

    void Start()
    {
        // SoundManager経由でBGMを再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
        }
    }
}