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
    private List<BoxCollider> spawnAreas = new List<BoxCollider>(); // 複数の生成範囲

    [SerializeField]
    private int spawnCount = 100; // 生成する数

    [SerializeField]
    private float minDistance = 0.2f; // 重なり防止のための最小距離
    [SerializeField]
    private int maxAttempts = 10; // 生成位置決定の最大試行回数

    [Header("ランダム設定")]
    [SerializeField]
    private Vector2 scaleRange = new Vector2(0.8f, 1.2f); // サイズのばらつき

    // 回転はプレハブ依存とし、ランダム回転フラグは削除済み

    private bool hasTriggered = false;
    private List<Vector3> spawnedPositions = new List<Vector3>(); // 生成済み位置リスト

    [Header("回転微調整")]
    [SerializeField]
    private Vector3 rotationOffset = Vector3.zero; // プレハブの向きが合わない場合の補正値

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
        Debug.Log($"🌑 [ClusterWallEvent] Generating {spawnCount} items across {spawnAreas.Count} walls...");

        // イベントID 35 をログに記録
        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null)
        {
            hm.LogEvent(35);
        }

        spawnedPositions.Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            // 位置決定の試行（重ならない場所を探す）
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // ランダムなエリアを選択
                BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Count)];
                if (area == null) continue;

                // BoxColliderのローカル座標系内でランダム点を生成
                Vector3 center = area.center;
                Vector3 size = area.size;

                float rx = Random.Range(-size.x / 2f, size.x / 2f);
                float ry = Random.Range(-size.y / 2f, size.y / 2f);
                float rz = Random.Range(-size.z / 2f, size.z / 2f);

                Vector3 localPoint = center + new Vector3(rx, ry, rz);
                Vector3 worldPoint = area.transform.TransformPoint(localPoint);

                // 重なりチェック
                bool isOverlapping = false;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(worldPoint, pos) < minDistance)
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                if (!isOverlapping)
                {
                    // 生成実行
                    SpawnAt(worldPoint, area);
                    spawnedPositions.Add(worldPoint);
                    break; // 成功したので次のアイテムへ
                }
            }
        }
    }

    private void SpawnAt(Vector3 pos, BoxCollider area)
    {
        float randomScaleFactor = Random.Range(scaleRange.x, scaleRange.y);
        Quaternion offsetRot = Quaternion.Euler(rotationOffset);

        // 1つ目のプレハブ生成
        if (elementPrefab1 != null)
        {
            // 壁の回転 + プレハブの回転 + 補正回転 を合成
            Quaternion finalRot = area.transform.rotation * elementPrefab1.transform.rotation * offsetRot;
            GameObject obj1 = Instantiate(elementPrefab1, pos, finalRot);
            
            // マネージャーの子にする
            obj1.transform.SetParent(this.transform);
            
            // スケール適用
            obj1.transform.localScale = elementPrefab1.transform.localScale * randomScaleFactor;
        }

        // 2つ目のプレハブ生成
        if (elementPrefab2 != null)
        {
            Quaternion finalRot = area.transform.rotation * elementPrefab2.transform.rotation * offsetRot;
            GameObject obj2 = Instantiate(elementPrefab2, pos, finalRot);
            
            obj2.transform.SetParent(this.transform);

            obj2.transform.localScale = elementPrefab2.transform.localScale * randomScaleFactor;
        }
    }
}
