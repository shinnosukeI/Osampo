using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieChaseEvent : MonoBehaviour
{
    [Header("ターゲット（プレイヤー）")]
    [SerializeField] private Transform player;

    [Header("アニメーション設定")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runAnimationName = "Run"; // AnimatorのState名またはパラメータ名

    [Header("移動設定")]
    [SerializeField] private float chaseSpeed = 2.0f; // 少しゆっくりに
    [SerializeField] private float minDistance = 3.0f; // プレイヤーとの最小距離（これ以上近づかない）

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamSound;

    private NavMeshAgent agent;
    private bool isChasing = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // 最初は止めておく
        agent.enabled = false;
        if (animator != null) animator.enabled = false;
    }

    private System.Collections.Generic.Queue<Vector3> pathHistory = new System.Collections.Generic.Queue<Vector3>();
    private Vector3 lastRecordedPosition;
    [SerializeField] private float recordInterval = 1.0f; // 1メートルごとに記録

    void Update()
    {
        if (isChasing && player != null && agent.enabled)
        {
            // 1. プレイヤーの軌跡を記録（ブレッドクラム）
            float distFromLast = Vector3.Distance(player.position, lastRecordedPosition);
            if (distFromLast > recordInterval)
            {
                pathHistory.Enqueue(player.position);
                lastRecordedPosition = player.position;
            }

            // 2. 移動目標の決定
            // 履歴がある場合は、一番古い履歴（プレイヤーが以前いた場所）を目指す
            if (pathHistory.Count > 0)
            {
                Vector3 target = pathHistory.Peek();
                agent.SetDestination(target);

                // その履歴地点に到達したら、リストから削除して次の地点へ
                if (Vector3.Distance(transform.position, target) < 1.5f) // 到達判定距離
                {
                    pathHistory.Dequeue();
                }
            }
            else
            {
                // 履歴をすべて消化したら、現在のプレイヤー位置を目指す
                agent.SetDestination(player.position);
            }

            // 3. プレイヤーとの距離チェック（追い越し防止）
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // 指定距離より近づいたら止まる
            if (distanceToPlayer <= minDistance)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // 慣性を消す
                if (animator != null) animator.speed = 0; // アニメーションも止める（滑り防止）
            }
            else
            {
                agent.isStopped = false;
                if (animator != null) animator.speed = 1; // アニメーション再開
            }
        }
    }

    public void ActivateEvent()
    {
        Debug.Log("🧟 ゾンビ追跡イベント発生！");

        // プレイヤーを自動検索（もし設定されていなければ）
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // 初期位置を記録
        if (player != null)
        {
            lastRecordedPosition = player.position;
            pathHistory.Clear();
            pathHistory.Enqueue(player.position);
        }

        // NavMeshAgentを有効化
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = chaseSpeed;
            agent.angularSpeed = 360.0f; // 回転速度を上げる（素早く向くように）
            agent.updateRotation = true; // 回転はAgentに任せる
        }

        // アニメーション開始
        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false; // RootMotionを切る（NavMeshと喧嘩しないように）
            // TriggerまたはBoolで遷移させるのが一般的だが、
            // シンプルにStateを再生、または "IsRunning" boolをオンにするなど
            // ここでは汎用的に Play を使用（AnimatorのState名と一致させる必要あり）
            animator.Play(runAnimationName);
        }

        // 叫び声
        if (audioSource != null && screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        isChasing = true;
    }
}
