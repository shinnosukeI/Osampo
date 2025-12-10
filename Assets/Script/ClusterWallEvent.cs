using UnityEngine;

public class ClusterWallEvent : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField]
    private GameObject elementPrefab; // へこみ+球体のプレハブ

    [SerializeField]
    private BoxCollider spawnArea; // 生成範囲（壁の表面に合わせたBoxCollider）

    [SerializeField]
    private int spawnCount = 100; // 生成する数

    [Header("ランダム設定")]
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.8f, 1.2f); // サイズのばらつき

    [SerializeField]
    private bool randomRotationZ = true; // Z軸（平面上の回転）をランダムにするか

    private bool hasTriggered = false;

    // イベント実行
    public void TriggerEvent()
    {
        if (hasTriggered || elementPrefab == null || spawnArea == null) return;

        hasTriggered = true;
        Debug.Log("🌑 [ClusterWallEvent] 集合体の生成を開始します");

        // Colliderの範囲を取得
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < spawnCount; i++)
        {
            // 範囲内でランダムな位置を決定
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPos = new Vector3(x, y, z);

            // プレハブを生成
            GameObject obj = Instantiate(elementPrefab, spawnPos, Quaternion.identity);

            // 親をこのオブジェクトにする（Hierarchyを散らかさないため）
            obj.transform.SetParent(this.transform);

            // 回転（壁の向きに合わせる必要があるため、基本はInspectorで設定したPrefabの向き依存）
            // 必要に応じてZ回転のみランダムにする
            if (randomRotationZ)
            {
                float rotZ = Random.Range(0f, 360f);
                obj.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
            }

            // サイズのランダム化
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            obj.transform.localScale = Vector3.one * scale;
        }
    }
}
