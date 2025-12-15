using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq; // Added for list operations

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

    [Header("44: 接近する人影イベント")] // ★ 追加
    [SerializeField]
    private ApproachingPersonEvent approachingPersonEvent;

    [Header("35: 壁の集合体イベント")] // ★ 追加
    [SerializeField]
    private ClusterWallEvent clusterWallEvent;

    [Header("51: 通行人イベント")] // ★ 追加
    [SerializeField]
    private WalkingPersonEvent walkingPersonEvent;

    [Header("52: ドアの隙間から女")] // ★ 追加
    [SerializeField]
    private DoorGapEvent doorGapEvent;

    [Header("43: 鏡の中の幽霊")] // ★ 追加
    [SerializeField]
    private MirrorGhostEvent mirrorGhostEvent;

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

    // ログ保存用
    private StreamWriter eventLogWriter;

    private Dictionary<int, Action> eventActionMap = new Dictionary<int, Action>();

    // ==========================================
    // ★ ステージループ・照明管理用の定義
    // ==========================================
    public enum LightingType
    {
        Default,
        PitchBlack,      // 真っ暗 (2-1 Loop 4, 30-46 Loop 4)
        SlightlyDark,    // 少し暗め / 暗め (2-5 Loop 1, Loop 4, 2-4 Loop 2)
        FaintLight,      // ほんの少しの明かり (Loop 5 Common)
        RedSpace,        // 真っ赤な空間 (2-2 Loop 4, 2-3 Loop 4)
        ThunderBlack,    // 雷タイミングで真っ暗 (2-4 Loop 4) -> 初期はDefault、イベント側で制御も可だが今回は設定として用意
        HallwayDark      // 廊下もユニットバスも暗い (2-4 Loop 1)
    }

    [System.Serializable]
    public struct StageLoopData
    {
        public int eventID;
        public LightingType lighting;

        public StageLoopData(int id, LightingType light)
        {
            eventID = id;
            lighting = light;
        }
    }

    // 現在のスケジュールのリスト
    private List<StageLoopData> currentStageSchedule = new List<StageLoopData>();

    // 初期照明設定の保存用
    private Color initAmbientLight;
    private float initFogDensity;
    private Color initFogColor;
    private bool initFogEnabled;

    [Header("13: 死体の腐敗イベント")] // ★ 追加
    [SerializeField]
    private DecayingCorpseEvent decayingCorpseEvent;

    // ==========================================
    // ★ デバッグ機能
    // ==========================================
    [Header("Debug Settings")]
    [Tooltip("有効にすると、以下の設定でゲームを開始します")]
    [SerializeField] private bool debugMode = false;
    
    [Tooltip("デバッグ用：強制的にこのアンケート結果（ステージ）にする (1～5)")]
    [Range(1, 5)]
    [SerializeField] private int debugSurveyResult = 1;

    [Tooltip("デバッグ用：開始時の周回数 (0始まり。例: 3なら4周目から)")]
    [SerializeField] private int debugStartCycle = 0;

    // 12と22は現在不明のためプレースホルダ
    // 22: クマ移動2?
    // 12: 死体引きずり?

    void Start()
    {
        // 照明の初期値を保存
        initAmbientLight = RenderSettings.ambientLight;
        initFogDensity = RenderSettings.fogDensity;
        initFogColor = RenderSettings.fogColor;
        initFogEnabled = RenderSettings.fog;

        // GameManagerからアンケート結果を取得
        currentSurveyResult = GameManager.SavedSurveyResult;
        
        // ★ デバッグモードの優先適用
        if (debugMode)
        {
            currentSurveyResult = debugSurveyResult;
            cycleCount = debugStartCycle;
            Debug.Log($"🔧 [HorrorEventManager] Debug Mode ENABLED. Result: {currentSurveyResult}, Cycle: {cycleCount}");
        }

        if (currentSurveyResult == -1)
        {
            Debug.LogWarning("⚠️ [HorrorEventManager] アンケート結果が取得できていません (Result is -1)");
        }
        else
        {
            Debug.Log($"📊 [HorrorEventManager] アンケート結果を取得しました: {currentSurveyResult}");
        }

        // ログ保存の初期化
        InitializeEventLogger();

        if (eventDatabase != null)
        {
            eventDatabase.Initialize();
        }

        RegisterEventActions();

        // スケジュール設定
        SetupStageSchedule(currentSurveyResult);

        // ★ 使わないイベントトリガーを無効化（軽量化・バグ防止）
        PruneUnusedTriggers();

        // 指定周回数の設定を適用
        ApplyLoopSetting(cycleCount);
    }

    /// <summary>
    /// 現在のスケジュールに含まれないイベントのトリガーをシーンから削除（無効化）する
    /// </summary>
    private void PruneUnusedTriggers()
    {
        // スケジュールにあるイベントIDのリストを作成
        HashSet<int> allowedEvents = new HashSet<int>(currentStageSchedule.Select(x => x.eventID));

        // シーン上のすべての HorrorEventTrigger を取得
        HorrorEventTrigger[] allTriggers = FindObjectsByType<HorrorEventTrigger>(FindObjectsSortMode.None);
        
        int disabledCount = 0;

        foreach (var trigger in allTriggers)
        {
            // トリガーが担当するイベントIDが含まれていなければ無効化
            // ※ eventType=0 など特殊な用途がある場合は除外判定を入れる
            if (!allowedEvents.Contains(trigger.eventType))
            {
                trigger.gameObject.SetActive(false);
                disabledCount++;
            }
        }

        Debug.Log($"🗑 [HorrorEventManager] 不要なトリガーを {disabledCount} 個 無効化しました。");
    }

    /// <summary>
    /// 各イベントの実行アクションを登録
    /// </summary>
    private void RegisterEventActions()
    {
        eventActionMap[11] = TriggerCockroachSwarm;
        eventActionMap[14] = TriggerZombieFall;
        eventActionMap[31] = TriggerBloodDrip;
        eventActionMap[35] = TriggerClusterWall; 
        eventActionMap[44] = TriggerApproachingPerson; 
        eventActionMap[45] = TriggerRadio;
        eventActionMap[54] = TriggerFallEvent;
        eventActionMap[56] = TriggerBallRoll;

        eventActionMap[24] = TriggerHandprint;
        eventActionMap[25] = TriggerWallEyes;
        eventActionMap[21] = TriggerBearMove;
        eventActionMap[53] = TriggerVanishingWoman;
        eventActionMap[15] = TriggerZombieChase;
        eventActionMap[46] = TriggerThunder; 
        eventActionMap[42] = TriggerLaugh;   
        eventActionMap[41] = TriggerFootstepsBehind; 
        eventActionMap[34] = StartSoundFromLocation; 
        eventActionMap[32] = TriggerBloodstain;      
        eventActionMap[51] = TriggerWalkingPerson;
        eventActionMap[52] = TriggerDoorGap;       
        eventActionMap[43] = TriggerMirrorGhost;
        
        // ★ 新規追加
        eventActionMap[13] = TriggerDecayingCorpse;
        eventActionMap[12] = () => Debug.Log("🎃 [Event 12] Placeholder (Corpse Drag?) triggered.");
        eventActionMap[22] = () => Debug.Log("🎃 [Event 22] Placeholder (Bear Move 2?) triggered.");
    }

    /// <summary>
    /// アンケート結果に基づいてステージのイベントスケジュールを構築する
    /// </summary>
    private void SetupStageSchedule(int result)
    {
        currentStageSchedule.Clear();

        switch (result)
        {
            case 2: // Stage 2-2
                currentStageSchedule.Add(new StageLoopData(21, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(22, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(24, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(25, LightingType.RedSpace));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;

            case 3: // Stage 2-3
                currentStageSchedule.Add(new StageLoopData(31, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(34, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(32, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(35, LightingType.RedSpace));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;

            case 4: // Stage 2-4
                currentStageSchedule.Add(new StageLoopData(43, LightingType.HallwayDark));
                currentStageSchedule.Add(new StageLoopData(41, LightingType.SlightlyDark));
                currentStageSchedule.Add(new StageLoopData(42, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(46, LightingType.ThunderBlack));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;

            case 5: // Stage 2-5
                currentStageSchedule.Add(new StageLoopData(51, LightingType.SlightlyDark));
                currentStageSchedule.Add(new StageLoopData(52, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(56, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(53, LightingType.SlightlyDark));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;

            case 1:
            default: // Stage 2-1 (Default / -1)
                currentStageSchedule.Add(new StageLoopData(13, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(14, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(12, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(15, LightingType.PitchBlack));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;
        }

        Debug.Log($"📅 [HorrorEventManager] Schedule Setup Complete based on Result {result}. Steps: {currentStageSchedule.Count}");
    }

    /// <summary>
    /// ループ回数に応じた設定（イベント＋照明）を適用する
    /// </summary>
    private void ApplyLoopSetting(int loopIndex)
    {
        if (currentStageSchedule.Count == 0) return;

        // インデックス範囲チェック
        if (loopIndex < 0) loopIndex = 0;
        
        int index = loopIndex;
        // 5周目以降はどうするか？ 仕様では「Loop 5」までしかない。
        // とりあえず最後の要素を使い続ける（Loop 5のまま）
        if (index >= currentStageSchedule.Count)
        {
            index = currentStageSchedule.Count - 1;
        }

        StageLoopData data = currentStageSchedule[index];

        Debug.Log($"🔄 [HorrorEventManager] Applying Loop {loopIndex + 1} Setting (Index {index}). Event: {data.eventID}, Light: {data.lighting}");

        // 1. 照明設定
        SetLighting(data.lighting);

        // 2. イベント発生
        TriggerHorrorEvent(data.eventID);
    }

    private void SetLighting(LightingType type)
    {
        // 初期リセット
        RenderSettings.ambientLight = initAmbientLight;
        RenderSettings.fogDensity = initFogDensity;
        RenderSettings.fogColor = initFogColor;
        RenderSettings.fog = initFogEnabled;

        switch (type)
        {
            case LightingType.PitchBlack:
            case LightingType.ThunderBlack:
                RenderSettings.ambientLight = Color.black;
                RenderSettings.fog = true;
                RenderSettings.fogColor = Color.black;
                RenderSettings.fogDensity = 0.5f; 
                break;

            case LightingType.SlightlyDark:
                RenderSettings.ambientLight = initAmbientLight * 0.5f;
                break;

            case LightingType.FaintLight:
                RenderSettings.ambientLight = initAmbientLight * 0.1f;
                break;
            
            case LightingType.HallwayDark:
                 RenderSettings.ambientLight = initAmbientLight * 0.2f;
                 break;

            case LightingType.RedSpace:
                RenderSettings.ambientLight = Color.red * 0.5f;
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(0.5f, 0, 0);
                RenderSettings.fogDensity = 0.15f;
                break;

            case LightingType.Default:
            default:
                break;
        }
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

        // ログ保存
        LogEvent(eventType);

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
        GameObject zombie = Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
        
        // スクリプトを取得して落下開始メソッドを呼ぶ
        FallingCorpse corpseScript = zombie.GetComponent<FallingCorpse>();
        if (corpseScript != null)
        {
            corpseScript.StartFalling();
        }
        else
        {
            // なければ物理だけONにする（保険）
            Rigidbody rb = zombie.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
        }
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

    // 35: 壁の集合体
    public void TriggerClusterWall()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerClusterWall が呼ばれました");
        if (clusterWallEvent != null)
        {
            clusterWallEvent.TriggerEvent();
        }
    }

    // 44: 接近する人影
    public void TriggerApproachingPerson()
    {
        Debug.Log("🎃 [HorrorEventManager] TriggerApproachingPerson が呼ばれました");
        if (approachingPersonEvent != null)
        {
            approachingPersonEvent.TriggerEvent();
        }
        else
        {
            Debug.LogError("❌ [HorrorEventManager] approachingPersonEvent が割り当てられていません！Inspectorで設定してください。");
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

    // 51: 通行人
    public void TriggerWalkingPerson()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerWalkingPerson が呼ばれました");
        if (walkingPersonEvent != null)
        {
            walkingPersonEvent.TriggerEvent();
        }
        else
        {
             Debug.LogError("❌ [HorrorEventManager] walkingPersonEvent が割り当てられていません！Inspectorで設定してください。");
        }
    }

    // 52: ドアの隙間から女
    public void TriggerDoorGap()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerDoorGap が呼ばれました");
        if (doorGapEvent != null)
        {
            doorGapEvent.TriggerEvent();
        }
        else
        {
            Debug.LogError("❌ [HorrorEventManager] doorGapEvent is not assigned!");
        }
    }

    // 43: 鏡の中の幽霊
    public void TriggerMirrorGhost()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerMirrorGhost が呼ばれました");
        if (mirrorGhostEvent != null)
        {
            mirrorGhostEvent.TriggerEvent();
        }
        else
        {
            Debug.LogError("❌ [HorrorEventManager] mirrorGhostEvent is not assigned!");
        }
    }


    // ============================
    // ★ ドア（ワープ含む）で呼び出す周期カウント
    // ============================
    // 13: 死体の腐敗
    public void TriggerDecayingCorpse()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerDecayingCorpse が呼ばれました");
        if (decayingCorpseEvent != null)
        {
            decayingCorpseEvent.ActivateEvent();
        }
        else
        {
            Debug.LogError("❌ [HorrorEventManager] decayingCorpseEvent is not assigned!");
        }
    }

    // ============================
    // ★ ドア（ワープ含む）で呼び出す周期カウント
    // ============================
    public void OnDoorClicked()
    {
        // 周回カウントを増やす
        cycleCount++;
        Debug.Log($"🚪 ドア/ワープで周期カウント: {cycleCount}");

        // スケジュールに基づいてイベント適用
        ApplyLoopSetting(cycleCount);
    }
    // ========================================================================
    // ▼▼▼ ログ保存機能 ▼▼▼
    // ========================================================================

    private void InitializeEventLogger()
    {
        string subjectID = GameManager.SubjectID;
        if (string.IsNullOrEmpty(subjectID))
        {
            subjectID = "TestUser"; // IDがない場合のフォールバック
        }

        // ファイル名: 被験者ID_03_HorrorEvent_log.csv
        string fileName = $"{subjectID}_03_HorrorEvent_log.csv";
        string directoryPath = Path.Combine(Application.persistentDataPath, "CSV");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string fullPath = Path.Combine(directoryPath, fileName);

        try
        {
            // 上書きモード(false)で作成。追記したい場合はtrueにするが、今回は新規作成とする
            eventLogWriter = new StreamWriter(fullPath, false, Encoding.UTF8);
            eventLogWriter.WriteLine("Timestamp,EventID"); // ヘッダー
            eventLogWriter.Flush();
            Debug.Log($"📄 [HorrorEventManager] ログファイルを作成しました: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"⚠ [HorrorEventManager] ログファイル作成エラー: {e.Message}");
        }
    }

    public void LogEvent(int eventType)
    {
        if (eventLogWriter != null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            eventLogWriter.WriteLine($"{timestamp},{eventType}");
            eventLogWriter.Flush(); // 即時書き込み
        }
    }

    private void CloseEventLogger()
    {
        if (eventLogWriter != null)
        {
            eventLogWriter.Flush();
            eventLogWriter.Close();
            eventLogWriter = null;
            Debug.Log("📄 [HorrorEventManager] ログファイルを閉じました。");
        }
    }

    void OnDestroy()
    {
        CloseEventLogger();
    }
    // ============================
    // ★ 外部からスケジュール確認用
    // ============================
    public bool IsEventScheduled(int eventID)
    {
        // Debug Modeの場合は常に許可するか、あるいは設定に従う
        if (debugMode && currentStageSchedule.Count == 0 && eventID == debugSurveyResult) 
        {
             // 簡易的な救済（本来はScheduleが作られているはず）
             return true; 
        }

        foreach (var data in currentStageSchedule)
        {
            if (data.eventID == eventID) return true;
        }
        return false;
    }
}
