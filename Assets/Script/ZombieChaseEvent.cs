using UnityEngine;

public class ZombieChaseEvent : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("プレイヤーに向かってくる速度")]
    [SerializeField] private float moveSpeed = 4.0f;
    [Tooltip("この距離まで近づいたら消える")]
    [SerializeField] private float disappearDistance = 1.5f;

    [Header("アニメーション設定")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runAnimationName = "Run";

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamSound;

    private Transform playerTransform;
    private Renderer[] renderers;
    private bool isInitialized = false;

    void Awake()
    {
        // レンダラー取得（視界判定用）
        renderers = GetComponentsInChildren<Renderer>();
        
        // アニメーター等の初期化
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = true; // ★ 基本はRootMotionをオン
        }
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>(); // ★ ない場合は自動追加
        
        // 初期状態は非表示（マネージャーから呼ばれるまで）
        // ただし、AwakeはSetActive(true)された直後にも呼ばれる可能性があるため、
        // ここでの非表示は「シーン開始時」のみ有効にしたいが、
        // HorrorEventManagerが制御するので、ここは安全策として。
        if (!isInitialized)
        {
            gameObject.SetActive(false);
            isInitialized = true; // 初回通過済みフラグ
        }
    }

    void Start()
    {
        // プレイヤーを探す
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else
            {
                // Tagで見つからない場合、Camera.mainから探す
                if (Camera.main != null) playerTransform = Camera.main.transform;
            }
        }
    }

    void OnEnable()
    {
        // アクティブになった瞬間（トリガーされた瞬間）
        // プレイヤー再取得のチャンス
        if (playerTransform == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // 音再生（ループ再生）
        if (audioSource != null && screamSound != null)
        {
            audioSource.clip = screamSound;
            audioSource.loop = true; // 追いかけている間はずっと鳴らす
            audioSource.spatialBlend = 1.0f; // 3Dサウンド
            
            // 距離減衰設定（デフォルト）
            // 必要に応じて調整してください
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 15.0f; 
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        // アニメーション開始
        if (animator != null)
        {
            animator.Play(runAnimationName);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 1. 視界判定（カメラに映っているか？）
        if (IsVisibleByCamera())
        {
            // ■ 見ている間：近づく

            // 向きをプレイヤーに向ける
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            // 高さ（Y）は合わせない（地面を歩く前提ならLookAtでX/Zのみ向けるのが良いが、簡易的にLookAt）
            direction.y = 0; 
            if (direction != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }

            // 前進処理は OnAnimatorMove で行うため、ここでのManual移動は削除
            // transform.position += transform.forward * moveSpeed * Time.deltaTime;

            // アニメーション更新（常に走る）
            if (animator != null)
            {
                // アニメーションが止まっていたら（ループ設定がなくても）無理やり再生し直す
                // 1.0を超えていたら再び0から再生
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
                {
                     // 手動ループ：即座に最初に戻す
                     animator.Play(runAnimationName, 0, 0f);
                }
            }

            // ■ 距離判定：近すぎたら消える
            // Y軸（高さ）の差を無視して、平面距離で判定する
            Vector3 myPos = transform.position;
            Vector3 targetPos = playerTransform.position;
            myPos.y = 0;
            targetPos.y = 0;

            float dist = Vector3.Distance(myPos, targetPos);
            
            // デバッグログ（距離を確認）
            // Debug.Log($"🧟 Dist: {dist}"); 

            if (dist <= disappearDistance)
            {
                Debug.Log($"🧟 [ZombieChaseEvent] Too close ({dist}m < {disappearDistance}m)! Disappearing.");
                gameObject.SetActive(false);
            }
        }
        else
        {
            // ■ 見ていない（後ろを向いた）：消える
            // プレイ開始直後の一瞬の判定除外が必要か？ -> OnEnable直後は許容するなど。
            // しかし「プレイヤーが後ろを向いたら非表示」という要件なので、即座に消して良い。
            Debug.Log("🧟 [ZombieChaseEvent] Player looked away. Disappearing.");
            gameObject.SetActive(false);
        }
    }

    // カメラの視錐台（Frustum）に入っているかチェック
    private bool IsVisibleByCamera()
    {
        if (Camera.main == null) return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (var r in renderers)
        {
            if (r != null && GeometryUtility.TestPlanesAABB(planes, r.bounds))
            {
                return true;
            }
        }
        return false;
    }

    // マネージャーから呼ばれる用（インターフェース合わせ）
    public void ActivateEvent()
    {
        // SetActive(true) はマネージャーが呼んでいるはずだが、念のため
        gameObject.SetActive(true);
        Debug.Log("🧟 [ZombieChaseEvent] ActivateEvent called.");
    }

    // Root Motion処理（ハイブリッド方式）
    void OnAnimatorMove()
    {
        if (animator != null)
        {
            // 1. アニメーションから移動量を取得
            Vector3 delta = animator.deltaPosition;
            
            // 2. 移動量がほぼゼロ（Bakeされている、またはInPlaceアニメーション）の場合
            //    -> 手動設定した moveSpeed で動かす
            if (delta.magnitude < 0.0001f)
            {
                // 手動移動（Time.deltaTimeはOnAnimatorMove内でも有効？）
                // OnAnimatorMoveは毎フレーム呼ばれるので deltaPosition はフレーム間移動量。
                // 手動でやるなら Time.deltaTime を使う。
                delta = transform.forward * moveSpeed * Time.deltaTime;
            }
            else
            {
                // 3. 移動量がある場合（RootMotion有効）
                //    ループ時の「引き戻し（マイナス移動）」を検知して無視する
                if (Vector3.Dot(delta, transform.forward) < 0)
                {
                    delta = Vector3.zero;
                }
            }

            // 適用
            transform.position += delta;
            
            // 回転はUpdate側で制御（LookAt）しているため、deltaRotationは適用しない
        }
    }
}


