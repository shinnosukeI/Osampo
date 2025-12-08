using UnityEngine;
using System;
using System.Collections.Generic;

public class HorrorEventManager : MonoBehaviour
{
    [SerializeField]
    private HorrorEventDatabase eventDatabase;

    [SerializeField]
    private FallingObjectAudio objectToFallTarget; // 54：物が落ちるイベント用

    [SerializeField]
    private CockroachSwarm cockroachSwarmTarget;// ★ 11：ゴキブリイベント用

    [Header("14: ゾンビ落下イベント")]
    [SerializeField]
    private GameObject zombiePrefab; // FallingCorpseスクリプト付きのプレハブ
    [SerializeField]
    private Transform zombieSpawnPoint;

    [SerializeField] private GameObject bloodSplashObject; // 32: 血痕

    [Header("31: 血が滴るイベント")]
    [SerializeField]
    private GameObject bloodDripObject;
    [SerializeField]
    private AudioClip bloodDripSound; // ★ 追加
    [SerializeField]
    [Tooltip("滴る音の間隔（秒）")]
    private float bloodDripInterval = 1.5f; // ★ 追加: 間隔調整用

    [Header("34: 特定の場所から鳴る音")] // ★ 追加
    [SerializeField]
    private AudioSource soundFromLocationSource;

    [Header("45: ラジオイベント")]
    [SerializeField]
    private RadioEventController radioController;

    [Header("55: 窓ガラスが割れるイベント")]
    [SerializeField]
    private GameObject normalWindowObject; // 割れる前の窓
    [SerializeField]
    private GameObject brokenWindowObject;

    [Header("56: ボールが転がるイベント")]
    [SerializeField]
    private GameObject ballPrefab;     // ボールのプレハブ
    [SerializeField]
    private Transform ballSpawnPoint;  // 出現位置

    [Header("24: 窓に手形イベント")]
    [SerializeField]
    private HandprintEvent handprintEventTarget;

    [Header("25: 壁に目イベント")]
    [SerializeField]
    private WallEyesEvent wallEyesEventTarget;

    [Header("21: 人形移動イベント")]
    [SerializeField]
    private BearMoveEvent bearMoveEventTarget;


    [Header("53: 消える女イベント")] // ★ 追加
    [SerializeField]
    private VanishingWomanEvent vanishingWomanEventTarget;

    [Header("15: ゾンビ追跡イベント")] // ★ 追加
    [SerializeField]
    private ZombieChaseEvent zombieChaseEventTarget;

    [Header("46: 雷イベント")] // ★ 追加
    [SerializeField]
    private AudioSource thunderAudioSource;
    [SerializeField]
    private AudioClip thunderSound;

    [Header("42: 笑い声イベント")] // ★ 追加
    [SerializeField]
    private AudioSource laughAudioSource;
    [SerializeField]
    private AudioClip laughSound;

    [Header("41: 背後の足音イベント")] // ★ 追加
    [SerializeField]
    private AudioClip footstepsSound;

    public List<(string Timestamp, int eventType)> eventLog = new List<(string, int)>();

    // ★ 周期カウント（ドア/ワープした回数）
    [Header("周回カウント")]
    [SerializeField] private int cycleCount = 0;
    public int CycleCount => cycleCount;

    // ★ 周回ごとに発生させるイベントタイプの一覧
    // 例: [54, 14, 31] → 1周目=54, 2周目=14, 3周目=31
    [Header("周回ごとのイベント設定")]
    [SerializeField] private List<int> cycleEventTypes = new List<int>();

    // イベントタイプ → 実行アクション のマップ
    // アンケート結果を保持する変数
    private int currentSurveyResult = -1;

    private Dictionary<int, Action> eventActionMap = new Dictionary<int, Action>();

    void Start()
    {
        // GameManagerからアンケート結果を取得
        currentSurveyResult = GameManager.SavedSurveyResult;
        Debug.Log($"📊 [HorrorEventManager] アンケート結果を取得しました: {currentSurveyResult}");

        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        RegisterEventActions();

        // 起動時テストは必要なら使う
        //TriggerHorrorEvent(54);
        //TriggerHorrorEvent(14);
        //TriggerHorrorEvent(31);

        // 34: 特定の場所から鳴る音を開始
        StartSoundFromLocation();
    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {
        eventActionMap[11] = TriggerCockroachSwarm;
        eventActionMap[14] = TriggerZombieFall;
        eventActionMap[31] = TriggerBloodDrip;
        eventActionMap[45] = TriggerRadio;
        eventActionMap[54] = TriggerFallEvent;
        eventActionMap[56] = TriggerBallRoll;

        eventActionMap[24] = TriggerHandprint;
        eventActionMap[25] = TriggerWallEyes;
        eventActionMap[21] = TriggerBearMove;
        eventActionMap[21] = TriggerBearMove;
        eventActionMap[53] = TriggerVanishingWoman;
        eventActionMap[15] = TriggerZombieChase;
        eventActionMap[46] = TriggerThunder; // ★ 追加
        eventActionMap[42] = TriggerLaugh;   // ★ 追加
        eventActionMap[41] = TriggerFootstepsBehind; // ★ 追加
        eventActionMap[34] = StartSoundFromLocation; // ★ 追加
        eventActionMap[32] = TriggerBloodstain;      // ★ 追加
    }

    /// <summary>
    /// イベントを発動
    /// </summary>
    public void TriggerHorrorEvent(int eventType)
    {
        HorrorEventData data = eventDatabase?.GetEventData(eventType);

        if (data == null)
        {
            Debug.LogWarning($"イベントタイプ {eventType} がデータベースに存在しません。");
            return;
        }

        Debug.Log($"🎃 イベント発生: {data.eventName} (Type: {eventType})");

        string currentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        eventLog.Add((currentTimestamp, eventType));

        // イベント固有のアクションが登録されていれば実行
        if (eventActionMap.TryGetValue(eventType, out Action action))
        {
            action.Invoke();
        }
        else
        {
            Debug.Log($"⚠ イベントタイプ {eventType} に対応するアクションが登録されていません。");
        }
    }

    // ======== 各イベント処理 ========

    // 11: 大量のゴキブリが出現する
    public void TriggerCockroachSwarm()
    {
        if (cockroachSwarmTarget != null)
        {
            cockroachSwarmTarget.StartSwarm();
        }
        else
        {
            Debug.LogError("ゴキブリ(CockroachSwarm)が設定されていません。");
        }
    }

    // 14: ゾンビが降ってくる
    public void TriggerZombieFall()
    {
        if (zombiePrefab == null || zombieSpawnPoint == null)
        {
            Debug.LogError("14: ゾンビのプレハブまたは出現位置が設定されていません。");
            return;
        }

        Debug.Log("😱 ゾンビが降ってきます！");
        Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
    }

    // 31: 血がしたたり落ちる
    public void TriggerBloodDrip()
    {
        if (bloodDripObject != null)
        {
            Debug.Log("🩸 血が滴り始めました...");
            bloodDripObject.SetActive(true);

            // コルーチンで間隔をあけて再生
            StartCoroutine(PlayBloodDripLoop());
        }
        else
        {
            Debug.LogError("31: 血のパーティクルが設定されていません。");
        }
    }

    private System.Collections.IEnumerator PlayBloodDripLoop()
    {
        AudioSource source = bloodDripObject.GetComponent<AudioSource>();
        
        // AudioSourceがない場合は追加する
        if (source == null)
        {
            source = bloodDripObject.AddComponent<AudioSource>();
            source.spatialBlend = 1.0f; // 3Dサウンドにする
        }

        // Clipがない場合は何もしない
        if (bloodDripSound == null) yield break;

        source.clip = bloodDripSound;
        source.loop = false; // 標準ループはオフにする（コルーチンで制御するため）

        while (true)
        {
            source.Play();
            // 指定した間隔だけ待つ
            yield return new WaitForSeconds(bloodDripInterval);
        }
    }

    // 32: 血痕
    public void TriggerBloodstain()
    {
        if (bloodSplashObject != null)
        {
            Debug.Log("🩸 血痕が現れました！");
            bloodSplashObject.SetActive(true);
        }
        else
        {
            Debug.LogError("32: 血痕オブジェクト(bloodSplashObject)が設定されていません。");
        }
    }

    // 45: ラジオから音がする
    public void TriggerRadio()
    {
        if (radioController != null)
        {
            radioController.PlayRadioSequence();
        }
        else
        {
            Debug.LogError("45: ラジオコントローラーが設定されていません。");
        }
    }

    // 54: 物が落ちる
    public void TriggerFallEvent()
    {
        if (objectToFallTarget != null)
        {
            objectToFallTarget.StartFall();
        }
        else
        {
            Debug.LogError("落下対象(FallingObjectAudio)が設定されていません。");
        }
    }

    // 55: 窓ガラスが割れる
    public void TriggerWindowBreak()
    {
        if (normalWindowObject != null && brokenWindowObject != null)
        {
            Debug.Log("💥 窓ガラスが割れます！");
            normalWindowObject.SetActive(false);
            brokenWindowObject.SetActive(true);
        }
        else
        {
            Debug.LogError("55: 窓ガラスのGameObjectが設定されていません。");
        }
    }

    // 56: ボールが転がってくる
    public void TriggerBallRoll()
    {
        if (ballPrefab != null && ballSpawnPoint != null)
        {
            Debug.Log("⚽ ボールが転がってきます！");
            GameObject ball = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
            
            // RollingBallコンポーネントを取得して転がす
            RollingBall rbScript = ball.GetComponent<RollingBall>();
            if (rbScript != null)
            {
                rbScript.StartRoll();
            }
        }
        else
        {
            Debug.LogError("56: ボールのプレハブまたは出現位置が設定されていません。");
        }
    }

    // 24: 窓に手形
    public void TriggerHandprint()
    {
        if (handprintEventTarget != null)
        {
            handprintEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError($"24: HandprintEventが設定されていません。(Object: {gameObject.name})");
        }
    }

    // 25: 壁に目
    public void TriggerWallEyes()
    {
        if (wallEyesEventTarget != null)
        {
            wallEyesEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError("25: WallEyesEventが設定されていません。");
        }
    }

    // 21: 人形移動
    public void TriggerBearMove()
    {
        if (bearMoveEventTarget != null)
        {
            bearMoveEventTarget.MoveToNextPosition();
        }
        else
        {
            Debug.LogError("21: BearMoveEventが設定されていません。");
        }
    }

    // 53: 消える女
    public void TriggerVanishingWoman()
    {
        if (vanishingWomanEventTarget != null)
        {
            vanishingWomanEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError("53: VanishingWomanEventが設定されていません。");
        }
    }

    // 15: ゾンビ追跡
    public void TriggerZombieChase()
    {
        if (zombieChaseEventTarget != null)
        {
            zombieChaseEventTarget.ActivateEvent();
        }
        else
        {
            Debug.LogError("15: ZombieChaseEventが設定されていません。");
        }
    }

    // 46: 雷
    public void TriggerThunder()
    {
        if (thunderAudioSource != null && thunderSound != null)
        {
            Debug.Log("⚡ 雷が鳴りました！");
            thunderAudioSource.PlayOneShot(thunderSound);
        }
        else
        {
            Debug.LogError("46: 雷のAudioSourceまたはAudioClipが設定されていません。");
        }
    }

    // 42: 笑い声
    public void TriggerLaugh()
    {
        if (laughAudioSource != null && laughSound != null)
        {
            Debug.Log("😈 笑い声が聞こえます...");
            laughAudioSource.PlayOneShot(laughSound);
        }
        else
        {
            Debug.LogError("42: 笑い声のAudioSourceまたはAudioClipが設定されていません。");
        }
    }

    // 41: 背後の足音
    public void TriggerFootstepsBehind()
    {
        if (footstepsSound != null && Camera.main != null)
        {
            Debug.Log("👣 背後から足音が聞こえます...");
            // プレイヤーの2メートル後ろ
            Vector3 spawnPos = Camera.main.transform.position - Camera.main.transform.forward * 2.0f;
            AudioSource.PlayClipAtPoint(footstepsSound, spawnPos);
        }
        else
        {
            Debug.LogError("41: 足音のAudioClipまたはMainCameraが設定されていません。");
        }
    }

    // 34: 特定の場所から鳴る音
    public void StartSoundFromLocation()
    {
        if (soundFromLocationSource != null)
        {
            // 3D設定を強制
            soundFromLocationSource.spatialBlend = 1.0f; // 1.0 = 完全3D
            soundFromLocationSource.loop = true;

            if (!soundFromLocationSource.isPlaying)
            {
                soundFromLocationSource.Play();
                Debug.Log("🔊 特定の場所からの音(34)を再生開始しました。");
            }
        }
        // 設定されていない場合は何もしない（エラーログは出さない、必須ではないかもしれないため）
    }


    // ============================
    // ★ ドア（ワープ含む）で呼び出す周期カウント
    // ============================
    public void OnDoorClicked()
    {
        // 周回カウントを増やす
        cycleCount++;
        Debug.Log($"🚪 ドア/ワープで周期カウント: {cycleCount}");

        if (cycleEventTypes == null || cycleEventTypes.Count == 0)
        {
            Debug.LogWarning("周回ごとのイベントが設定されていません。");
            return;
        }

        // --- パターンA: 最後の要素を以降も使い続ける ---
        int index = cycleCount - 1;
        if (index >= cycleEventTypes.Count)
        {
            index = cycleEventTypes.Count - 1; // 最後の要素
        }

        int eventType = cycleEventTypes[index];
        Debug.Log($"🎃 周回 {cycleCount} でイベント {eventType} を実行");
        TriggerHorrorEvent(eventType);

        /* --- パターンB: リストをループさせたい場合 ---
        // 例: [54,14,31] → 1周目=54, 2=14, 3=31, 4=54...
        int index = (cycleCount - 1) % cycleEventTypes.Count;
        int eventType = cycleEventTypes[index];
        TriggerHorrorEvent(eventType);
        ------------------------------------------------- */
    }
}
