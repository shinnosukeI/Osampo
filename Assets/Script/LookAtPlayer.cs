using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    // ターゲット（プレイヤーのカメラ）
    private Transform target;

    void Start()
    {
        // 自動的にメインカメラを探してターゲットにする
        if (Camera.main != null)
        {
            target = Camera.main.transform;
        }
    }

    void Update()
    {
        if (target != null)
        {
            // ターゲットの方を向く
            transform.LookAt(target);
        }
    }
}