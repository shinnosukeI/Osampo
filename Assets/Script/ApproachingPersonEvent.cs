using System.Collections;
using UnityEngine;

public class ApproachingPersonEvent : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private GameObject personObject; // 人のモデル
    [SerializeField] private AudioClip glassSound;    // 最初のガラス音
    [SerializeField] private AudioClip jumpScareSound;// 最後の悲鳴など
    [SerializeField] private AudioSource audioSource;

    [Header("接近位置（順番に移動）")]
    [SerializeField] private Transform[] approachPositions; // 0:初期位置 ... Last:目の前

    [Header("パラメータ")]
    [SerializeField] private float lookThreshold = 0.7f; // 視界に入った判定の閾値
    [SerializeField] private float stepDelay = 0.5f;     // 移動間の待機時間
    [SerializeField] private float jumpScareDuration = 2.0f; // 最後の表示時間

    private bool isEventActive = false;
    private bool hasLookedAt = false;
    private Camera playerCamera;

    void Start()
    {
        if (personObject != null)
        {
            personObject.SetActive(false);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        playerCamera = Camera.main;
    }

    public void TriggerEvent()
    {
        if (isEventActive) return;
        isEventActive = true;
        hasLookedAt = false;

        Debug.Log("👻 [ApproachingPersonEvent] イベント開始");

        // 1. 音を鳴らす（背後でガラスを踏む音など）
        if (glassSound != null)
        {
            audioSource.PlayOneShot(glassSound);
        }

        // 人を初期位置に表示（まだプレイヤーは見えていないはず）
        if (approachPositions != null && approachPositions.Length > 0)
        {
            personObject.transform.position = approachPositions[0].position;
            personObject.transform.rotation = approachPositions[0].rotation;
            personObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!isEventActive || hasLookedAt || personObject == null || playerCamera == null) return;

        // 2. プレイヤーが振り返って人を見たか判定
        if (IsVisibleFrom(playerCamera, personObject.transform))
        {
            Debug.Log("👻 [ApproachingPersonEvent] プレイヤーが目撃しました。接近を開始します。");
            hasLookedAt = true;
            StartCoroutine(ApproachSequence());
        }
    }

    private bool IsVisibleFrom(Camera cam, Transform target)
    {
        Vector3 viewPos = cam.WorldToViewportPoint(target.position);
        // ビューポート座標が 0～1 の範囲内なら画面に映っている
        // かつ、Zが正（カメラの前方）
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
        {
            // 障害物判定（Raycast）を入れるとより正確だが、今回は簡易的に画面内判定のみ
            return true;
        }
        return false;
    }

    private IEnumerator ApproachSequence()
    {
        // 3. 接近（瞬間移動）
        // 初期位置(0)は既に表示済みなので、1から開始
        for (int i = 1; i < approachPositions.Length; i++)
        {
            yield return new WaitForSeconds(stepDelay);

            if (approachPositions[i] != null)
            {
                personObject.transform.position = approachPositions[i].position;
                personObject.transform.rotation = approachPositions[i].rotation;
                
                // 移動音などを鳴らしても良い
                // audioSource.PlayOneShot(stepSound); 
            }
        }

        // 4. 最後（ジャンプスケア）
        if (jumpScareSound != null)
        {
            audioSource.PlayOneShot(jumpScareSound);
        }

        // しばらく表示してから消す
        yield return new WaitForSeconds(jumpScareDuration);

        personObject.SetActive(false);
        isEventActive = false;
        Debug.Log("👻 [ApproachingPersonEvent] イベント終了");
    }
}
