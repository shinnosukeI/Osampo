using UnityEngine;

public class GlassAreaFootstepPlayer : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;

    [Tooltip("st1_HorrorEventManager を入れる（周回数を見る用）")]
    public st1_HorrorEventManager eventManager;

    [Header("Sound")]
    public AudioClip glassStepClip;
    public float volume = 1f;

    [Header("Condition")]
    public int enableFromCycle = 5;          // 5週目以降
    public float moveThreshold = 0.05f;      // 動いてる判定
    public float stepInterval = 0.4f;        // 足音間隔

    [Header("If eventManager が読めない時の保険")]
    [Tooltip("eventManager の cycleCount が読めない場合、ここに手動で現在周回を入れてテストできる")]
    public int debugCycle = 0;
    public bool useDebugCycle = false;

    private CharacterController cc;
    private float timer;
    private bool inArea;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        cc = GetComponent<CharacterController>();
        if (audioSource != null) audioSource.loop = false;
    }

    public void SetInArea(bool value)
    {
        inArea = value;

        // エリア外に出た瞬間に止める
        if (!inArea && audioSource != null && audioSource.isPlaying)
            audioSource.Pause();
    }

    void Update()
    {
        if (audioSource == null || glassStepClip == null) return;

        // 5週目以降か？
        int cycle = GetCycleSafely();
        if (cycle < enableFromCycle)
        {
            if (audioSource.isPlaying) audioSource.Pause();
            timer = 0f;
            return;
        }

        // エリア内か？
        if (!inArea)
        {
            if (audioSource.isPlaying) audioSource.Pause();
            timer = 0f;
            return;
        }

        // 動いてるか？
        float speed = 0f;
        bool grounded = true;

        if (cc != null)
        {
            speed = cc.velocity.magnitude;
            grounded = cc.isGrounded;
        }
        else
        {
            // CCが無い場合は transform の移動量から速度算出（保険）
            // ※必要なら lastPos 方式に変えてもOK
            speed = 0f;
        }

        if (!grounded || speed <= moveThreshold)
        {
            if (audioSource.isPlaying) audioSource.Pause();
            timer = 0f;
            return;
        }

        // ここまで来たら「5週目以降 && エリア内 && 動いてる」
        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            PlayStep();
            timer = 0f;
        }
    }

    void PlayStep()
    {
        // PlayOneShotだと止められないので clip再生にする
        if (audioSource.isPlaying) return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.clip = glassStepClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    int GetCycleSafely()
    {
        if (useDebugCycle) return debugCycle;

        // ★ここがポイント：cycleCountがprivateだと直接読めない
        // なので eventManager 側に「public int CurrentCycleCount => cycleCount;」が必要
        // それがある前提で読む（無いなら debugCycle をONにしてテスト可能）
        if (eventManager != null)
            return eventManager.CycleCount;

        return 0;
    }
}
