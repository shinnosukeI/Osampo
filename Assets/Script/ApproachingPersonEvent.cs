using System.Collections;
using UnityEngine;

public class ApproachingPersonEvent : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private GameObject personObject; // 人のモデル
    [SerializeField] private AudioClip glassSound;    // 最初のガラス音
    [SerializeField] private AudioClip jumpScareSound;// 最後の悲鳴など
    [SerializeField] private AudioClip appearanceVoiceSound; // ★ 追加: 出現時の声
    [SerializeField] private AudioClip lastMoveSound; // ★ 追加: 最後の移動が終わったときの音
    [SerializeField] private AudioSource audioSource;

    [Header("接近位置（順番に移動）")]
    [SerializeField] private Transform[] approachPositions; // 0:初期位置 ... Last:目の前

    [Header("パラメータ")]
    [SerializeField] private float lookThreshold = 0.7f; // 視界に入った判定の閾値
    [SerializeField] private float stepDelay = 0.5f;     // 移動間の待機時間
    [SerializeField] private float jumpScareDuration = 2.0f; // 最後の表示時間
    [Tooltip("出現してから移動を開始するまでの待機時間")]
    [SerializeField] private float initialWaitTime = 3.0f; // ★ 追加: 移動開始前の待機
    [Range(0f, 1f)]
    [SerializeField] private float eventVolume = 0.5f;   // ★ 追加: 音量設定
    [Range(0f, 10f)]
    [SerializeField] private float lastSoundVolume = 1.0f; // ★ 追加: 最後の音専用の音量

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
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        // ★ 音量適用
        if (audioSource != null)
        {
            audioSource.volume = eventVolume;
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

    [Header("位置合わせ")]
    [SerializeField] private Transform playerForceMoveTarget; // プレイヤーを強制移動させる位置

    private GameObject crosshairCanvas;

    private IEnumerator ForceTurnSequence()
    {
        // 1. 操作不能にする
        if (playerMove != null) playerMove.enabled = false;
        if (playerLook != null) playerLook.enabled = false;

        // ★ クロスヘアを一時的に消す
        crosshairCanvas = GameObject.Find("CrosshairCanvas");
        if (crosshairCanvas == null)
        {
            // 名前で見つからない場合、Canvas全検索で探す
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c.name == "CrosshairCanvas")
                {
                    crosshairCanvas = c.gameObject;
                    break;
                }
            }
        }

        if (crosshairCanvas != null)
        {
            crosshairCanvas.SetActive(false);
            Debug.Log("👁 [ApproachingPersonEvent] CrosshairCanvas deactivated.");
        }
        else
        {
            Debug.LogWarning("⚠ [ApproachingPersonEvent] CrosshairCanvas NOT FOUND! Cannot hide crosshair.");
        }

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
            
            // ★ 修正: 相対回転ではなく、出現位置(approachPositions[0])の方を向くように変更
            Quaternion targetRot = playerTransform.rotation; // デフォルトは現在値
            
            if (approachPositions != null && approachPositions.Length > 0 && approachPositions[0] != null)
            {
                // プレイヤーからターゲットへの方向ベクトル
                Vector3 direction = approachPositions[0].position - playerTransform.position;
                direction.y = 0; // 上下方向の回転はしない（水平回転のみ）
                
                if (direction != Vector3.zero)
                {
                    targetRot = Quaternion.LookRotation(direction);
                }
            }
            
            Quaternion startRot = playerTransform.rotation;

            // ★ 修正: 振り向く前に人を配置・表示する（振り返ったら既にいるようにする）
            if (approachPositions != null && approachPositions.Length > 0)
            {
                personObject.transform.position = approachPositions[0].position;
                personObject.transform.rotation = approachPositions[0].rotation;
                personObject.SetActive(true);

                // 出現時（配置時）に声を再生
                if (appearanceVoiceSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(appearanceVoiceSound);
                }
            }
            
            // ★ 位置合わせの計算
            Vector3 startPos = playerTransform.position;
            Vector3 targetPos = startPos;
            bool shouldMove = false;

            if (playerForceMoveTarget != null)
            {
                targetPos = playerForceMoveTarget.position;
                // Y軸（高さ）はプレイヤーの現在地を維持する場合
                // targetPos.y = startPos.y; 
                // または指定位置に合わせるか。CharacterControllerがあるなら高さはずれるとまずいが、
                // ここでは強制移動なのでTargetの位置(床)に合わせてもよい。
                // いったんTargetの座標をそのまま使う（空のオブジェクトを床に置く想定）
                shouldMove = true;
            }

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
                
                if (shouldMove)
                {
                    playerTransform.position = Vector3.Lerp(startPos, targetPos, t);
                }

                yield return null;
            }

            // 念のため最終値をセット
            playerTransform.rotation = targetRot;
            camTransform.localRotation = targetCamRot;
            if (shouldMove)
            {
                playerTransform.position = targetPos;
            }
        }

        yield return new WaitForSeconds(0.2f);

        // 4. 人を表示（上記で既に表示済みだが、念のため位置補正などあればここで行う）
        // if (approachPositions != null && approachPositions.Length > 0) ... 既に表示済みなのでスキップ
        // 念のためActive確認だけ
        if (personObject != null && !personObject.activeSelf)
        {
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
        // ★ 追加: 3秒待ってから移動開始
        yield return new WaitForSeconds(initialWaitTime);

        // 3. 接近（瞬間移動）
        // 初期位置(0)は既に表示済みなので、1から開始
        for (int i = 1; i < approachPositions.Length; i++)
        {
            yield return new WaitForSeconds(stepDelay);

            if (approachPositions[i] != null)
            {
                personObject.transform.position = approachPositions[i].position;
                personObject.transform.rotation = approachPositions[i].rotation;
                
                // ★ 最後の移動かどうか判定
                if (i == approachPositions.Length - 1)
                {
                    if (lastMoveSound != null)
                    {
                        // ★ 専用の音量で再生
                        audioSource.PlayOneShot(lastMoveSound, lastSoundVolume);
                    }
                }
            }
        }

        // 4. 最後（ジャンプスケア）
        if (jumpScareSound != null)
        {
            audioSource.PlayOneShot(jumpScareSound);
        }

        // ★ 最後の場所に移動した1秒後に遷移（ユーザー要望）
        yield return new WaitForSeconds(1.0f);

        Debug.Log("👻 [ApproachingPersonEvent] イベント終了 -> ResultSceneへ遷移します");
        
        // ★ ResultSceneへ遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("ResultScene");
    }
}
