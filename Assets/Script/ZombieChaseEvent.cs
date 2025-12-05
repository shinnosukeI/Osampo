using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieChaseEvent : MonoBehaviour
{
    [Header("目的地（廊下の突き当たりなど）")]
    [SerializeField] private Transform targetDestination;

    [Header("アニメーション設定")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runAnimationName = "Run";
    [SerializeField] private string idleAnimationName = "Idle"; // ★ 追加

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 3.5f; // 走るスピード
    [SerializeField] private float walkSpeed = 1.0f; // ★ 追加: 歩くスピード（振り返った時）

    [Header("音響設定")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamSound;

    private NavMeshAgent agent;
    private bool isActive = false;
    private Renderer[] renderers; // 視界判定用
    private bool wasVisible = false; // 前フレームの状態

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        if (animator != null) 
        {
            animator.enabled = false;
        }
        
        // 子要素含む全てのRendererを取得（視界判定のため）
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (isActive && targetDestination != null && agent.enabled)
        {
            // 1. カメラに映っているか判定
            bool isVisible = IsVisibleByCamera();

            if (isVisible)
            {
                // 見られている -> 歩く
                // Agentの自動移動は止める
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                
                if (animator != null)
                {
                    if (!wasVisible)
                    {
                        animator.Play(idleAnimationName); // 歩きアニメーション再生
                        animator.applyRootMotion = true;  // Root Motion有効化（動きを吸い出すため）
                    }

                    // ★ スクリプトで手動で前進させる
                    // Root MotionをOnAnimatorMoveで無効化しているため、ここで動かさないと進まない
                    agent.Move(transform.forward * walkSpeed * Time.deltaTime);
                }
            }
            else
            {
                // 見られていない -> 走る（Agentで進む）
                agent.isStopped = false;
                agent.SetDestination(targetDestination.position);
                
                if (animator != null && wasVisible)
                {
                    animator.Play(runAnimationName); // 走り再生
                    animator.applyRootMotion = false; // Root Motion無効化
                }
            }
            
            wasVisible = isVisible; // 状態更新
        }
    }

    // Root Motionを「吸い出して捨てる」ための処理
    private void OnAnimatorMove()
    {
        // applyRootMotionが有効な時、ここが呼ばれる。
        // ここで何もしなければ、アニメーションの移動（Root Motion）は無視される（＝Bake Into Poseと同じ状態になる）。
        // これにより、アニメーションによる「カクつき（戻り）」を防ぎつつ、
        // Update内で agent.Move を使ってスムーズに移動させることができる。
        
        // 必要であれば回転だけ適用するなど調整可能だが、今回は何もしない。
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
                return true; // どれか一つのパーツでも映っていれば「見えている」とみなす
            }
        }
        return false;
    }

    public void ActivateEvent()
    {
        Debug.Log("🧟 ゾンビイベント開始（だるまさんが転んだモード）");

        if (targetDestination == null)
        {
            Debug.LogError("ZombieChaseEvent: Target Destination が設定されていません！");
            return;
        }

        // NavMeshAgent設定
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = moveSpeed;
            agent.angularSpeed = 360.0f;
            agent.acceleration = 20.0f;
            agent.updateRotation = true;
        }

        // アニメーション開始
        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.Play(runAnimationName);
        }

        // 音再生
        if (audioSource != null && screamSound != null)
        {
            audioSource.PlayOneShot(screamSound);
        }

        isActive = true;
    }
}
