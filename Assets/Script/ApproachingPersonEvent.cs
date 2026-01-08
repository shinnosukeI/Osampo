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

    // プレイヤー制御スクリプトへの参照
    private FreeMoveInputSystem playerMove;
    private CameraLookInputSystem playerLook;


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

        // プレイヤーの操作スクリプトを取得（タグ検索 -> コンポーネント取得）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMove = player.GetComponent<FreeMoveInputSystem>();
            // CameraLookInputSystemはCameraにある場合とPlayerにある場合があるが、
            // 今回のプロジェクトでは CameraLookInputSystem.cs を見ると playerBody を参照しているので
            // Cameraオブジェクトか、Playerの子オブジェクトについている可能性がある。
            // 確実なのは FindObjectOfType か、Playerから探すこと。
            playerLook = player.GetComponentInChildren<CameraLookInputSystem>();
            if (playerLook == null) playerLook = FindObjectOfType<CameraLookInputSystem>();
        }
    }

    public void TriggerEvent()
    {
        if (isEventActive) return;
        isEventActive = true;
        hasLookedAt = false;

        Debug.Log("👻 [ApproachingPersonEvent] イベント開始 - 強制振り向きシーケンス");

        // 強制振り向きコルーチン開始
        StartCoroutine(ForceTurnSequence());
    }

    private IEnumerator ForceTurnSequence()
    {
        // 1. 操作不能にする
        if (playerMove != null) playerMove.enabled = false;
        if (playerLook != null) playerLook.enabled = false;

        yield return null;

        // 2. 音を鳴らす（背後でガラスを踏む音など）
        if (glassSound != null)
        {
            audioSource.PlayOneShot(glassSound);
        }

        // 3. 180度振り向く
        if (playerCamera != null && playerMove != null)
        {
            Transform playerTransform = playerMove.transform;
            
            // 目標の回転（現在のY軸反対側）
            Quaternion startRot = playerTransform.rotation;
            Quaternion targetRot = startRot * Quaternion.Euler(0, -200, 0);
            
            // カメラの上下（Pitch）も0に戻す（正面を見る）
            Transform camTransform = playerCamera.transform;
            Quaternion startCamRot = camTransform.localRotation;
            Quaternion targetCamRot = Quaternion.identity; // ローカル回転0 = 正面

            float duration = 1.5f; // 振り向きにかかる時間
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // イージング（滑らかに）
                t = Mathf.SmoothStep(0f, 1f, t);

                playerTransform.rotation = Quaternion.Slerp(startRot, targetRot, t);
                camTransform.localRotation = Quaternion.Slerp(startCamRot, targetCamRot, t);

                yield return null;
            }

            // 念のため最終値をセット
            playerTransform.rotation = targetRot;
            camTransform.localRotation = targetCamRot;
        }

        yield return new WaitForSeconds(0.2f);

        // 4. 人を初期位置に表示（ここでようやく出現）
        if (approachPositions != null && approachPositions.Length > 0)
        {
            personObject.transform.position = approachPositions[0].position;
            personObject.transform.rotation = approachPositions[0].rotation;
            personObject.SetActive(true);
        }

        // 振り向き完了したので、次の検知フェーズへ
        // （Updateでの視界判定が走るようになる）
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
        
        // 操作を戻す
        if (playerMove != null) playerMove.enabled = true;
        if (playerLook != null) playerLook.enabled = true;

        Debug.Log("👻 [ApproachingPersonEvent] イベント終了");
    }
}
