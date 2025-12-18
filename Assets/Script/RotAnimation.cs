using UnityEngine;

public class RotAnimation : MonoBehaviour
{
    [Header("アニメーション設定")]
    public float targetValue = 0.5f; // 目標の腐敗度（0.5）
    public float duration = 3.0f;    // かかる時間（5秒）

    private Material myMat;          // マテリアルを操作する用
    private float timer = 0.0f;      // 時間計測用
    private int rotPropID;           // シェーダーの変数のID

    void Start()
    {
        // このオブジェクトについているマテリアルを取得
        Renderer renderer = GetComponent<Renderer>();
        myMat = renderer.material;

        // シェーダーの "_RotAmount" という名前を検索してID化（高速化のため）
        // ※ShaderGraphのReference名と一致させる必要があります！
        rotPropID = Shader.PropertyToID("_RotAmount");

        // 最初は 0 にリセットしておく
        myMat.SetFloat(rotPropID, 0.0f);
    }

    void Update()
    {
        // 設定した時間よりタイマーが小さければ実行
        if (timer < duration)
        {
            // 時間を進める
            timer += Time.deltaTime;

            // 進行率（0～1）を計算
            float progress = timer / duration;

            // 0 から targetValue まで、進行率に合わせて数字を変化させる
            float currentValue = Mathf.Lerp(0.0f, targetValue, progress);

            // シェーダーに値を送る
            myMat.SetFloat(rotPropID, currentValue);
        }
    }
}