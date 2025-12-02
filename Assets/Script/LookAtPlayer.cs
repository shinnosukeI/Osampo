using UnityEngine;
using UnityEngine;
using UnityEngine;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    // ターゲット（プレイヤーのカメラ）
    private Transform target;

    // 向きの補正（モデルの正面がZ軸でない場合に使用）
    public Vector3 rotationOffset = new Vector3(0, 0, 0);

    void Start()
    {
        // 1. MainCameraを探す
        if (Camera.main != null)
        {
            target = Camera.main.transform;
        }
        
        // 2. 見つからなければ "Player" タグを探す
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("LookAtPlayer: ターゲット（MainCamera または Playerタグ）が見つかりません。");
        }
    }

    void Update()
    {
        if (target != null)
        {
            // ターゲットの方を向く
            transform.LookAt(target);
            
            // 補正を加える（ローカル座標で回転）
            transform.Rotate(rotationOffset);
        }
    }
}