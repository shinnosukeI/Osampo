using UnityEngine;

public class IdleLoopController : MonoBehaviour
{
    // Animatorコンポーネントを格納する変数
    private Animator animator;

    // 待機アニメーションを切り替えるまでの間隔（最小/最大）
    public float minChangeTime = 3f; // 最小3秒
    public float maxChangeTime = 5f; // 最大5秒

    private float nextChangeTime; // 次に切り替える時刻
    private bool isIdle01 = true; // 現在Idle01が再生中かどうか

    void Start()
    {
        // 同じゲームオブジェクトからAnimatorコンポーネントを取得
        animator = GetComponent<Animator>();

        // 最初の切り替え時間をランダムに設定
        SetNextChangeTime();
    }

    void Update()
    {
        // ゲーム開始からの時間が、設定した切り替え時間を超えたかチェック
        if (Time.time >= nextChangeTime)
        {
            // 現在の待機状態を反転させる (Idle01ならIdle02へ、Idle02ならIdle01へ)
            isIdle01 = !isIdle01; 

            // AnimatorのChangeIdleパラメーターを操作してアニメーションを切り替え
            // Idle02を再生したい場合、ChangeIdleはtrueになる（Idle01からIdle02への遷移条件）
            animator.SetBool("ChangeIdle", !isIdle01); 

            // 次の切り替え時間を再設定
            SetNextChangeTime();
        }
    }

    // 次にアニメーションを切り替える時間をランダムに設定する関数
    void SetNextChangeTime()
    {
        // 現在の時刻に、ランダムな間隔を加算
        float randomInterval = Random.Range(minChangeTime, maxChangeTime);
        nextChangeTime = Time.time + randomInterval;
    }
}