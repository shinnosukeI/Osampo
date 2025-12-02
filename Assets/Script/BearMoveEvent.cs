using UnityEngine;

public class BearMoveEvent : MonoBehaviour
{
    [Header("移動させるオブジェクト（クマの人形など）")]
    [SerializeField] private GameObject bearObject;

    [Header("移動先リスト（順番に移動します）")]
    [Tooltip("空のGameObjectなどを配置して、そのTransformを登録してください")]
    [SerializeField] private Transform[] movePositions;

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveSound;

    private int currentIndex = 0;

    public void MoveToNextPosition()
    {
        if (bearObject == null)
        {
            Debug.LogError("BearMoveEvent: 移動させるオブジェクトが設定されていません。");
            return;
        }

        if (movePositions == null || movePositions.Length == 0)
        {
            Debug.LogWarning("BearMoveEvent: 移動先が設定されていません。");
            return;
        }

        // まだ移動先が残っている場合
        if (currentIndex < movePositions.Length)
        {
            Transform targetTransform = movePositions[currentIndex];
            if (targetTransform != null)
            {
                // 位置と回転を更新（ワープ）
                bearObject.transform.position = targetTransform.position;
                bearObject.transform.rotation = targetTransform.rotation;
                
                Debug.Log($"🧸 クマが移動しました: {currentIndex + 1}番目の位置 ({targetTransform.name})");
            }

            // 次の移動先に進める
            currentIndex++;

            // 音を再生
            if (audioSource != null && moveSound != null)
            {
                audioSource.PlayOneShot(moveSound);
            }
        }
        else
        {
            // 全ての移動が終わった場合（必要なら非表示にするなどの処理を追加可能）
            Debug.Log("🧸 クマはこれ以上移動しません（リストの最後まで到達しました）");
            // bearObject.SetActive(false); // 例: 最後は消える場合
        }
    }
}
