using UnityEngine;
using System.Collections.Generic;

public class ClusterWallEvent : MonoBehaviour
{
    [Header("生成設定")]
    [SerializeField]
    private GameObject elementPrefab1; // 1つ目のプレハブ（例：へこみ）
    [SerializeField]
    private GameObject elementPrefab2; // 2つ目のプレハブ（例：球体）

    [SerializeField]
    private List<BoxCollider> spawnAreas = new List<BoxCollider>(); // 生成範囲（複数指定可能）

    [SerializeField]
    private int spawnCount = 100; // 生成する数

    [SerializeField]
    private float minDistance = 0.3f; // 重なり防止のための最小距離
    [SerializeField]
    private int maxAttempts = 10; // 生成位置決定の最大試行回数

    [Header("ランダム設定")]
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.8f, 1.2f); // サイズのばらつき

    [SerializeField]
    private bool randomRotationZ = true; // Z軸（平面上の回転）をランダムにするか（現在は使用していない）

    private bool hasTriggered = false;
    private List<Vector3> spawnedPositions = new List<Vector3>();

    // イベント実行
    public void TriggerEvent()
    {
        Debug.Log("🌑 [ClusterWallEvent] TriggerEvent called");

        if (hasTriggered)
        {
             Debug.Log("ℹ [ClusterWallEvent] Already triggered");
             return;
        }

        if (spawnAreas == null || spawnAreas.Count == 0)
        {
            Debug.LogError("❌ [ClusterWallEvent] No Spawn Areas Assigned!");
            return;
        }

        if (elementPrefab1 == null && elementPrefab2 == null)
        {
            Debug.LogError("❌ [ClusterWallEvent] Both Prefabs are None!");
            return;
        }

        hasTriggered = true;
        Debug.Log($"🌑 [ClusterWallEvent] Generating {spawnCount} items...");

        // イベントID 35 をログに記録
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(35);
        }

        spawnedPositions.Clear();

        int skippedCount = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // ランダムにエリア（壁）を1つ選ぶ
                BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Count)];
                if (area == null) continue;

                // 選んだエリアの範囲内でランダムな位置を決定
                Bounds bounds = area.bounds;
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float y = Random.Range(bounds.min.y, bounds.max.y);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 spawnPos = new Vector3(x, y, z);

                // 重なりチェック
                bool isOverlapping = false;
                foreach (Vector3 existingPos in spawnedPositions)
                {
                    if (Vector3.Distance(spawnPos, existingPos) < minDistance)
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                // 重なっていたら再抽選
                if (isOverlapping)
                {
                    skippedCount++;
                    continue; // 次の試行へ
                }

                // 重なっていなければ生成してループを抜ける
                spawnedPositions.Add(spawnPos);
                SpawnAt(spawnPos);
                break; // 成功したので i (生成個数) のループを進める
            }
        }

        Debug.Log($"🌑 [ClusterWallEvent] Generation complete. Skipped {skippedCount} overlaps.");
    }

    private void SpawnAt(Vector3 spawnPos)
    {
        // ランダムなスケールのみ適用（回転はしない）
        float randomScaleFactor = Random.Range(scaleRange.x, scaleRange.y);

        // 1つ目のプレハブ生成
        if (elementPrefab1 != null)
        {
            // プレハブ自体の回転をそのまま採用
            GameObject obj1 = Instantiate(elementPrefab1, spawnPos, elementPrefab1.transform.rotation);
            obj1.transform.SetParent(this.transform);
            
            // プレハブ自体のスケール × ランダム倍率
            obj1.transform.localScale = elementPrefab1.transform.localScale * randomScaleFactor;
        }

        // 2つ目のプレハブ生成
        if (elementPrefab2 != null)
        {
            GameObject obj2 = Instantiate(elementPrefab2, spawnPos, elementPrefab2.transform.rotation);
            obj2.transform.SetParent(this.transform);

            obj2.transform.localScale = elementPrefab2.transform.localScale * randomScaleFactor;
        }
    }
}
