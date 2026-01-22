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
    private List<Collider> spawnAreas = new List<Collider>(); // 生成範囲（BoxCollider または SphereCollider）

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

    [Header("配置設定")]
    [SerializeField]
    private bool spawnOnSurface = false; // ★ 表面だけに配置するか（中身スカスカにするか）
    [SerializeField]
    private bool alignToNormal = true;   // ★ 中心から外側を向くように回転させるか

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

        hasTriggered = true;
        Debug.Log($"🌑 [ClusterWallEvent] Generating {spawnCount} items...");

        HorrorEventManager hm = FindFirstObjectByType<HorrorEventManager>();
        if (hm != null) hm.LogEvent(35);

        spawnedPositions.Clear();

        int skippedCount = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Collider area = spawnAreas[Random.Range(0, spawnAreas.Count)];
                if (area == null) continue;

                Vector3 spawnPos = Vector3.zero;
                Quaternion spawnRot = Quaternion.identity;
                bool rotationCalculated = false;

                // ★ Colliderのタイプによって生成ロジックを分ける
                if (area is SphereCollider sphere)
                {
                    // 球体
                    Vector3 randomPoint;
                    if (spawnOnSurface)
                        randomPoint = Random.onUnitSphere;    // 表面上
                    else
                        randomPoint = Random.insideUnitSphere; // 内部含む

                    // ワールド座標へ変換
                    Vector3 localPos = (randomPoint * sphere.radius) + sphere.center;
                    spawnPos = sphere.transform.TransformPoint(localPos);
                    
                    // ★ 中心から外側を向く回転を計算
                    if (alignToNormal)
                    {
                        Vector3 centerPos = sphere.transform.TransformPoint(sphere.center);
                        Vector3 direction = (spawnPos - centerPos).normalized;
                        if (direction != Vector3.zero)
                        {
                            spawnRot = Quaternion.LookRotation(direction);
                            rotationCalculated = true;
                        }
                    }
                }
                else if (area is BoxCollider box)
                {
                    // Boxの場合
                    Bounds bounds = area.bounds;
                    float x = Random.Range(bounds.min.x, bounds.max.x);
                    float y = Random.Range(bounds.min.y, bounds.max.y);
                    float z = Random.Range(bounds.min.z, bounds.max.z);
                    spawnPos = new Vector3(x, y, z);
                    
                    // Boxの場合は「壁」とみなして、エリアの前方（Z軸?）などを向く処理を入れてもいいが
                    // 現状はPrefabの向き、あるいはランダム
                }
                else
                {
                    Bounds bounds = area.bounds;
                    spawnPos = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x),
                        Random.Range(bounds.min.y, bounds.max.y),
                        Random.Range(bounds.min.z, bounds.max.z)
                    );
                }

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

                if (isOverlapping)
                {
                    skippedCount++;
                    continue; 
                }

                spawnedPositions.Add(spawnPos);
                
                // 回転が決まっていなければPrefabのを使用 (SpawnAt内部で処理変更)
                SpawnAt(spawnPos, rotationCalculated ? spawnRot : (Quaternion?)null);
                break; 
            }
        }
        Debug.Log($"🌑 [ClusterWallEvent] Generation complete. Skipped {skippedCount} overlaps.");
    }

    private void SpawnAt(Vector3 spawnPos, Quaternion? explicitRotation = null)
    {
        float randomScaleFactor = Random.Range(scaleRange.x, scaleRange.y);

        if (elementPrefab1 != null)
        {
            Quaternion rot = explicitRotation.HasValue ? explicitRotation.Value : elementPrefab1.transform.rotation;
            GameObject obj1 = Instantiate(elementPrefab1, spawnPos, rot);
            obj1.transform.SetParent(this.transform);
            // ★ 修正: SetParentで自動調整されたlocalScaleに対して倍率をかける (親のScaleが大きくても破綻しないように)
            obj1.transform.localScale *= randomScaleFactor;
        }

        if (elementPrefab2 != null)
        {
            Quaternion rot = explicitRotation.HasValue ? explicitRotation.Value : elementPrefab2.transform.rotation;
            GameObject obj2 = Instantiate(elementPrefab2, spawnPos, rot);
            obj2.transform.SetParent(this.transform);
            // ★ 修正
            obj2.transform.localScale *= randomScaleFactor;
        }
    }
}
