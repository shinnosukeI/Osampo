using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq; // Added for list operations

public class HorrorEventManager : MonoBehaviour
{
    // ==========================================
    // ★ デバッグ機能 (最上部に移動)
    // ==========================================
    [Header("Debug Settings")]
    [Tooltip("有効にすると、以下のDebug設定でゲームを開始します")]
    [SerializeField] private bool debugMode = false;

    [Header("Lighting Settings")]
    [Tooltip("制御したい照明のリスト（複数可）")]
    [SerializeField] private List<Light> targetLights;
    
    [Tooltip("デバッグ用：強制的にこのアンケート結果（ステージ）にする (1～5)")]
    [Range(1, 5)]
    [SerializeField] private int debugSurveyResult = 1;

    [Tooltip("デバッグ用：開始時の周回数 (0始まり。例: 3なら4周目から)")]
    [SerializeField] private int debugStartCycle = 0;

    [Space(10)]
    [Header("Current Status (Read Only)")]
    [Tooltip("現在のアンケート結果（実行時に決定されます）")]
    [SerializeField] private int currentSurveyResult = -1;

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

    [Header("32: 血痕イベント")] // ★ 追加: ユーザーが見つけられるようにHeader付与
    [SerializeField] private GameObject bloodSplashObject;

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
    [SerializeField]
    private GameObject wallEyesRootObject; // ★ 追加: 目が表示されている親オブジェクトを指定

    [Header("21: 人形移動イベント")]
    [SerializeField]
    private BearMoveEvent bearMoveEventTarget;
    [SerializeField]
    private GameObject bearMoveRootObject; // ★ 追加


    [Header("53: 消える女イベント")] // ★ 追加
    [SerializeField]
    private VanishingWomanEvent vanishingWomanEventTarget;
    [SerializeField]
    private GameObject vanishingWomanRootObject; // ★ 追加

    [Header("15: ゾンビ追跡イベント")] // ★ 追加
    [SerializeField]
    private ZombieChaseEvent zombieChaseEventTarget;

    [Header("46: 雷イベント")] // ★ 追加
    [SerializeField]
    private AudioSource thunderAudioSource;
    [SerializeField] private AudioClip thunderSound;
    [Range(0f, 5f)] [SerializeField] private float thunderVolume = 1.0f; // ★ 追加: 雷の音量
    [SerializeField] private Light[] thunderLights; // ★ 追加: 雷で光らせるライト
    [SerializeField] private float thunderIntensityMultiplier = 5.0f; // ★ 追加: 光の強さ倍率
    [SerializeField] private int flashCount = 10; // ★ 追加: 点滅回数

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
    [SerializeField]
    private GameObject clusterWallRootObject; // ★ 追加: 集合体の親オブジェクト

    [Header("51: 通行人イベント")] // ★ 追加
    [SerializeField]
    private WalkingPersonEvent walkingPersonEvent;
    [SerializeField] private AudioSource walkingPersonAudioSource; // ★ 追加
    [SerializeField] private AudioClip walkingPersonSound;       // ★ 追加

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
    // アンケート結果を保持する変数 (上部へ移動済み)
    // private int currentSurveyResult = -1;

    // ログ保存用
    private StreamWriter eventLogWriter;
    // ★ 追加: 同一ループ内での重複ログ防止用
    private HashSet<int> currentLoopLoggedEvents = new HashSet<int>();

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
    public struct LightSetting
    {
        public Color color;
        public float intensity;

        public LightSetting(Color col, float inten)
        {
            color = col;
            intensity = inten;
        }
    }

    [System.Serializable]
    public struct StageLoopData
    {
        public int eventID;
        public LightingType lighting;
        public List<LightSetting> lightSettings; // ★ 複数照明対応
        // ★ 追加: 一括設定用
        public bool useGlobalLightSetting;
        public LightSetting globalLightSetting;

        public StageLoopData(int id, LightingType light, List<LightSetting> settings = null, bool useGlobal = false, LightSetting globalSet = default)
        {
            eventID = id;
            lighting = light;
            lightSettings = settings ?? new List<LightSetting>();
            useGlobalLightSetting = useGlobal;
            globalLightSetting = globalSet;
        }
    }

    // 現在のスケジュールのリスト
    private List<StageLoopData> currentStageSchedule = new List<StageLoopData>();

    // ★ インスタンス化された一時的なオブジェクト（ゾンビ、ボールなど）の追跡リスト
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // ★ 照明の初期状態保存用
    private List<LightSetting> initialLightSettings = new List<LightSetting>();

    // 初期照明設定の保存用 (RenderSettings)
    private Color initAmbientLight;
    private float initFogDensity;
    private Color initFogColor;
    private bool initFogEnabled;

    [Header("13: 蜘蛛イベント")] // ★ 変更
    [SerializeField]
    private SpiderWallEvent spiderWallEvent;
    [SerializeField]
    private GameObject spiderWallRootObject; // 親オブジェクト

    [Header("12: 死体の腐敗（13とは別扱い?）")]
    [SerializeField] private GameObject corpseRotTarget;

    [Header("22: マネキン")]
    [SerializeField] private GameObject mannequinTarget;

    [Header("23: 水回りの髪")]
    [SerializeField] private GameObject hairTarget;

    [Header("31: 血が滴るイベント(静的表示用)")]
    [SerializeField] private GameObject event31StaticObject; // 31の時だけ表示する

    [Header("33: 血とガラス片")]
    [SerializeField] private GameObject bloodAndGlassTarget;

    [Header("56: ポールポンポン")]
    [SerializeField] private GameObject polePonPonTarget;



    // 12と22は現在不明のためプレースホルダ
    // 22: クマ移動2?
    // 12: 死体引きずり?

    void Start()
    {
        // 照明の初期値を保存 (RenderSettings)
        initAmbientLight = RenderSettings.ambientLight;
        initFogDensity = RenderSettings.fogDensity;
        initFogColor = RenderSettings.fogColor;
        initFogEnabled = RenderSettings.fog;

        // ★ Target Lights の初期値を保存
        if (targetLights != null)
        {
            foreach (var l in targetLights)
            {
                if (l != null)
                {
                    initialLightSettings.Add(new LightSetting(l.color, l.intensity));
                }
                else
                {
                    // インデックスズレ防止のためダミー
                    initialLightSettings.Add(new LightSetting(Color.white, 1f));
                }
            }
        }

        // GameManagerからアンケート結果を取得
        currentSurveyResult = GameManager.SavedSurveyResult;
        
        // ★ デバッグモードの優先適用
        if (debugMode)
        {
            currentSurveyResult = debugSurveyResult;
            cycleCount = debugStartCycle;
            Debug.Log($"🔧 [HorrorEventManager] Debug Mode ENABLED. Result: {currentSurveyResult}, Cycle: {cycleCount}");
        }
        else
        {
            // デバッグでない通常時、スタートは1周目
            cycleCount = 1;
        }

        // ★ 安全策: 0以下になっていたら1にする（インデックスズレ防止）
        if (cycleCount < 1) cycleCount = 1;

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

        // ★ 初期状態の設定（該当イベントがあっても、最初は隠しておくものなど）
        InitializeObjectStates();

        // ★ 不要なトリガー・オブジェクトの削除は ApplyLoopSetting で行われるため、ここでの呼び出しは削除
        // 指定周回数の設定を適用
        ApplyLoopSetting(cycleCount);
    }

    /// <summary>
    /// 現在のスケジュールに含まれないイベントのトリガーをシーンから削除（無効化）する
    /// </summary>
    /// <summary>
    /// 現在のループで有効なイベント(activeEventID)以外のトリガーをすべて無効化する
    /// トリガーが見つかった場合は true を返す
    /// </summary>
    private bool PruneUnusedTriggers(int activeEventID)
    {
        // 1. HorrorEventTrigger (コライダー式の従来のトリガー)
        HorrorEventTrigger[] allTriggers = FindObjectsByType<HorrorEventTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        // 2. ProximityLogTrigger (接近検知式の新しいトリガー)
        ProximityLogTrigger[] proxTriggers = FindObjectsByType<ProximityLogTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        Debug.Log($"👀 [Prune] Searching triggers for ActiveID: {activeEventID}. Found Standard: {allTriggers.Length}, Proximity: {proxTriggers.Length}");
        
        int disabledCount = 0;
        bool foundActive = false;

        // HorrorEventTrigger の処理
        foreach (var trigger in allTriggers)
        {
            if (trigger.eventType != activeEventID)
            {
                if (trigger.gameObject.activeSelf)
                {
                    trigger.gameObject.SetActive(false);
                    disabledCount++;
                }
            }
            else
            {
                trigger.gameObject.SetActive(true);
                trigger.ResetTrigger();
                foundActive = true;
                Debug.Log($"✅ [Prune] ACTIVATED Standard Trigger for Event {activeEventID} (Obj: {trigger.gameObject.name})");
            }
        }

        // ProximityLogTrigger の処理
        foreach (var trigger in proxTriggers)
        {
            if (trigger.eventType != activeEventID)
            {
                // 自分自身のコンポーネントのみ無効化するか、GameObjectごと消すか？
                // 汎用性を考えてGameObjectごと消すが、他のコンポーネントがある場合は注意が必要。
                // 今回は「トリガー専用オブジェクト」とみなしてSetActive(false)する。
                if (trigger.gameObject.activeSelf)
                {
                    trigger.gameObject.SetActive(false);
                    disabledCount++;
                }
            }
            else
            {
                trigger.gameObject.SetActive(true);
                // ProximityLogTriggerにはResetTriggerがないが、Enable時にログ済みフラグが残っている場合があるため、
                // 必要ならリセット機能を追加すべきだが、今回はActive化のみ行う
                foundActive = true; 
                Debug.Log($"✅ [Prune] ACTIVATED Proximity Trigger for Event {activeEventID} (Obj: {trigger.gameObject.name})");
            }
        }

        Debug.Log($"🗑 [HorrorEventManager] 現在のイベント({activeEventID})以外のトリガー {disabledCount} 個を無効化しました。");
        return foundActive;
    }

    /// <summary>
    /// 現在のスケジュールに含まれないイベントのオブジェクトを非表示/無効化する
    /// </summary>
    /// <summary>
    /// 現在のループで有効なイベント(activeEventID)以外のオブジェクトを非表示/無効化する
    /// </summary>
    private void PruneUnusedObjects(int activeEventID)
    {
        // イベントID と 管理オブジェクト のペアをリスト化
        // ここに登録されたオブジェクトは、activeEventIDでなければ Disable される
        var managedObjects = new List<(int id, Component comp, GameObject obj)>
        {
            (11, cockroachSwarmTarget, null),
            (14, null, null), 
            (31, null, bloodDripObject),
            (32, null, bloodSplashObject),
            (45, radioController, null),
            (54, objectToFallTarget, null),
            (55, null, normalWindowObject), 
            (55, null, brokenWindowObject), 
            (56, null, polePonPonTarget),
            (24, handprintEventTarget, null),
            (25, wallEyesEventTarget, wallEyesRootObject),
            (21, bearMoveEventTarget, bearMoveRootObject), // updated
            (53, vanishingWomanEventTarget, vanishingWomanRootObject), // updated
            (15, zombieChaseEventTarget, null),
            (46, thunderAudioSource, null),
            (42, laughAudioSource, null),
            (34, soundFromLocationSource, null),
            (44, approachingPersonEvent, null),
            (35, clusterWallEvent, clusterWallRootObject),
            (51, walkingPersonEvent, null),
            (52, doorGapEvent, null),
            (43, mirrorGhostEvent, null),
            (13, spiderWallEvent, spiderWallRootObject), // ★ 変更
            (12, null, corpseRotTarget),
            (22, null, mannequinTarget),
            (23, null, hairTarget),
            (33, null, bloodAndGlassTarget),
            (31, null, event31StaticObject) // ★ 追加: 31の静的オブジェクト（別枠で追加して管理）
        };

        int disabledCount = 0;

        foreach (var item in managedObjects)
        {
            // 現在のアクティブIDと一致するか？
            if (item.id == activeEventID) 
            {
                 // ★ Triggerで開始するイベント(15など)は、ここでのAuto-Activeはしない（最初は隠しておく）
                 // 13(蜘蛛)は「Event 25のようにその時だけ表示」との要望により、ここでActiveにする（Static表示）

                 // ★ Event 43 (MirrorGhost) の場合
                 if (item.id == 43)
                 {
                     // オブジェクトはここではActiveにしない（最初は隠しておく）
                     // トリガーイベントだけ呼んで「フォーカス待機状態」にする
                     if (mirrorGhostEvent != null)
                     {
                         mirrorGhostEvent.TriggerEvent();
                     }
                     // これで処理完了として次へ（下の共通SetActiveを通さない）
                     continue;
                 }

                 // ★ Event 15 (ZombieChase) の場合
                 if (item.id == 15)
                 {
                     // トリガーで開始するため、ここではActiveにしない
                     continue;
                 }

                 // ★ Event 52 (DoorGap) の場合: 開始直後から隙間を開けておきたい
                 if (item.id == 52)
                 {
                     if (doorGapEvent != null)
                     {
                         doorGapEvent.gameObject.SetActive(true);
                         doorGapEvent.TriggerEvent(); // ★ ここで開く処理を呼ぶ
                     }
                     // これで処理完了
                     continue;
                 }

                 // ★ アクティブなイベントのオブジェクトは、ループ開始時から表示しておく
                 if (item.comp != null) item.comp.gameObject.SetActive(true);
                 if (item.obj != null) item.obj.SetActive(true);

                 // ★ Event 25 (WallEyes) の場合は準備処理（オブジェクト削除）を呼ぶ
                 if (item.id == 25 && wallEyesEventTarget != null)
                 {
                     wallEyesEventTarget.PrepareEvent();
                 }

                 continue;
            }

            // 一致しない場合、無効化する
            if (item.comp != null && item.comp.gameObject.activeSelf)
            {
                item.comp.gameObject.SetActive(false);
                disabledCount++;
            }
            
            if (item.obj != null && item.obj.activeSelf)
            {
                item.obj.SetActive(false);
                disabledCount++;
            }
        }

        Debug.Log($"👻 [HorrorEventManager] イベント({activeEventID}) 以外のオブジェクト {disabledCount} 個を非表示にし、アクティブ対象を表示しました。");
    }

    /// <summary>
    /// 各イベントオブジェクトの初期状態を設定（基本非表示にするなど）
    /// </summary>
    private void InitializeObjectStates()
    {
        // st1_HorrorEventManagerを参考に、開始時は非表示にすべきものをここで切る
        if (bloodSplashObject != null) bloodSplashObject.SetActive(false);
        if (bloodDripObject != null)
        {
            bloodDripObject.SetActive(false);
            // ロード時の一瞬の再生を防ぐ（AudioSourceがすでにある場合）
            var source = bloodDripObject.GetComponent<AudioSource>();
            if (source != null)
            {
                 source.Stop();
                 source.playOnAwake = false;
            }
        }
        
        // 窓: 割れる前は表示、割れた後は非表示
        if (normalWindowObject != null) normalWindowObject.SetActive(true);
        if (brokenWindowObject != null) brokenWindowObject.SetActive(false);

        if (wallEyesRootObject != null) wallEyesRootObject.SetActive(false);
        if (clusterWallRootObject != null) clusterWallRootObject.SetActive(false);
        if (bearMoveRootObject != null) bearMoveRootObject.SetActive(false);
        if (vanishingWomanRootObject != null) vanishingWomanRootObject.SetActive(false);
        if (event31StaticObject != null) event31StaticObject.SetActive(false); // 初期非表示

        // 蜘蛛: 最初はいない
        if (spiderWallEvent != null) spiderWallEvent.gameObject.SetActive(false);
        if (spiderWallRootObject != null) spiderWallRootObject.SetActive(false);
        
        if (corpseRotTarget != null) corpseRotTarget.SetActive(false);

        // ミラーゴースト: 最初はいない
        if (mirrorGhostEvent != null) mirrorGhostEvent.gameObject.SetActive(false);

        // 隙間女: 最初はいない
        if (doorGapEvent != null) doorGapEvent.gameObject.SetActive(false);

        // 通行人: 最初はいない
        if (walkingPersonEvent != null) walkingPersonEvent.gameObject.SetActive(false);

        // 壁の集合体: 最初はいない
        if (clusterWallEvent != null) clusterWallEvent.gameObject.SetActive(false);
        
        // 人形移動: 最初は非表示 & リセット
        if (bearMoveEventTarget != null) 
        {
            bearMoveEventTarget.gameObject.SetActive(false);
            bearMoveEventTarget.ResetEvent(); // ★ インデックスリセット
        }

        // 接近する人影
        if (approachingPersonEvent != null) 
        {
            approachingPersonEvent.gameObject.SetActive(false);
            approachingPersonEvent.ResetEvent(); // ★ 重複発動防止のためにリセット
        }
        
        // 追加オブジェクト
        if (mannequinTarget != null) mannequinTarget.SetActive(false);
        if (hairTarget != null) hairTarget.SetActive(false);
        if (bloodAndGlassTarget != null) bloodAndGlassTarget.SetActive(false);
        if (polePonPonTarget != null) polePonPonTarget.SetActive(false);

        // 15: ゾンビ追跡 (これが抜けていたため毎周回残っていた可能性大)
        if (zombieChaseEventTarget != null) zombieChaseEventTarget.gameObject.SetActive(false);

        // ラジオなども最初は止めておく？（AutoPlayかどうかによるが念のため）
        if (radioController != null) radioController.gameObject.SetActive(false); 
        // 45はトリガーで開始するならOK。RadioEventControllerの実装次第。

        Debug.Log("🔒 [HorrorEventManager] イベントオブジェクトの初期表示状態をリセットしました");
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
        eventActionMap[13] = TriggerSpiderWall; // ★ 変更
        eventActionMap[12] = TriggerCorpseRot;
        eventActionMap[22] = TriggerMannequin; // ★ 実装
    }

    // 22: マネキン
    public void TriggerMannequin()
    {
        Debug.Log("🎃 [HorrorEventManager] TriggerMannequin (22) が呼ばれました");
        if (mannequinTarget != null)
        {
            mannequinTarget.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ [HorrorEventManager] mannequinTarget (Event 22) is not assigned!");
        }
    }




    // ★ アンケート結果ごとのイベント設定用クラス
    [System.Serializable]
    public class SurveyScenarioData
    {
        [Tooltip("アンケート結果の値 (例: 1, 2, 3...)")]
        public int surveyResultID;
        [Tooltip("このアンケート結果の場合に発生させる各周回のイベント設定")]
        public List<StageLoopData> loopSettings = new List<StageLoopData>();
    }

    [Header("アンケート結果別イベント設定")]
    [SerializeField]
    [Tooltip("アンケート結果ごとのイベントスケジュールをここで設定してください")]
    private List<SurveyScenarioData> scenarioSettings = new List<SurveyScenarioData>();

    /// <summary>
    /// アンケート結果に基づいてステージのイベントスケジュールを構築する
    /// </summary>
    private void SetupStageSchedule(int result)
    {
        currentStageSchedule.Clear();

        // Inspectorで設定されたリストから、該当するアンケート結果の設定を検索
        var scenario = scenarioSettings.FirstOrDefault(x => x.surveyResultID == result);

        if (scenario != null && scenario.loopSettings != null && scenario.loopSettings.Count > 0)
        {
            currentStageSchedule.AddRange(scenario.loopSettings);
            Debug.Log($"📅 [HorrorEventManager] アンケート結果 {result} に基づく設定をロードしました。設定数: {currentStageSchedule.Count}");
        }
        else
        {
            Debug.LogWarning($"⚠️ [HorrorEventManager] アンケート結果 {result} に対応するイベント設定がInspectorで見つかりません。" +
                             "デフォルトのハードコード設定を使用します（設定されている場合）。" +
                             "UnityエディタのInspectorで 'Scenario Settings' を設定してください。");

            // フォールバック（既存のハードコード設定を念のため残すか、完全に空にするか）
            // ユーザーのリクエストは「Unity上で指定したい」なので、基本はInspector優先。
            // 移行期のために一時的に古いswitch文をフォールバックとして残しておきますが、
            // 設定を行えばそちらが優先されます。
            SwitchFallbackSchedule(result);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"📅 [HorrorEventManager] Schedule Setup Finalized. Result: {result}, Steps: {currentStageSchedule.Count}");
        for(int i=0; i<currentStageSchedule.Count; i++)
        {
            sb.AppendLine($"  Loop {i+1}: Event {currentStageSchedule[i].eventID}, Light {currentStageSchedule[i].lighting}");
        }
        Debug.Log(sb.ToString());
    }

    private void SwitchFallbackSchedule(int result)
    {
        switch (result)
        {
            case 1: // Stage 2-1
                currentStageSchedule.Add(new StageLoopData(13, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(14, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(12, LightingType.Default));
                currentStageSchedule.Add(new StageLoopData(15, LightingType.PitchBlack));
                currentStageSchedule.Add(new StageLoopData(44, LightingType.FaintLight));
                break;
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
            default:
                // デフォルト
                currentStageSchedule.Add(new StageLoopData(13, LightingType.Default));
                break;
        }
    }

    /// <summary>
    /// ループ回数に応じた設定（イベント＋照明）を適用する
    /// </summary>
    private void ApplyLoopSetting(int loopIndex)
    {
        if (currentStageSchedule.Count == 0) return;

        // インデックス範囲チェック: ゲーム開始がCycle 1なので、Indexは Cycle-1
        int index = loopIndex - 1;

        if (index < 0) index = 0;
        
        // 5周目以降はどうするか？ 仕様では「Loop 5」までしかない。
        // とりあえず最後の要素を使い続ける（Loop 5のまま）
        if (index >= currentStageSchedule.Count)
        {
            index = currentStageSchedule.Count - 1;
        }

        StageLoopData data = currentStageSchedule[index];

        Debug.Log($"🔄 [HorrorEventManager] Applying Loop {loopIndex} Setting (AryIndex {index}). Event: {data.eventID}, Light: {data.lighting}");

        // ★ ループ切り替え時にステージ状態をリセットする
        
        // 0. 前のループで生成された一時オブジェクト（ゾンビ、ボール）を削除
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // ★ ループ切替時にログ済み履歴をリセット
        currentLoopLoggedEvents.Clear();
        Debug.Log("🔄 [HorrorEventManager] Duplicate Log History Cleared.");

        // 1. 全イベントオブジェクトを初期状態（基本非表示）に戻す
        InitializeObjectStates();
        // 2. 現在のループのイベント(data.eventID)以外を無効化する
        bool triggerFound = PruneUnusedTriggers(data.eventID);
        PruneUnusedObjects(data.eventID);

        // ★ トリガーが見つからない（配置していない）場合、ループ開始と同時にイベントを即時実行する
             if (!triggerFound)
        {
             Debug.Log($"⚡ [HorrorEventManager] イベント {data.eventID} のトリガーが見つからないため、即時実行します。");
             if (eventActionMap.ContainsKey(data.eventID))
             {
                 eventActionMap[data.eventID]?.Invoke();
             }
             else
             {
                 Debug.LogWarning($"⚠ イベント {data.eventID} のアクションが登録されていません。");
             }
        }

        // ★ Event 34 (特定の場所から鳴る音) の場合、ループに入った時点でログ取得扱いにする (ユーザー要望)
        if (data.eventID == 34)
        {
            Debug.Log("📝 [HorrorEventManager] Note: Event 34 is auto-logged upon loop entry.");
            LogEvent(34);
        }

        // 3. 照明設定
        SetLighting(data);

        // ★ 4. 全てのドアに対してイベント制限を更新
        DoorController[] allDoors = FindObjectsByType<DoorController>(FindObjectsSortMode.None);
        foreach (var door in allDoors)
        {
            if (door != null)
            {
                // イベントIDによる制限更新
                door.UpdateEventRestriction(data.eventID);
                
                // ★ 周回またぎでロックを解除（手動操作ロックのリセット）
                door.ResetLock();
            }
        }

        // ★ 5. DoorLockTriggerのリセット（通り過ぎ検知を初期化）
        DoorLockTrigger[] allTriggers = FindObjectsByType<DoorLockTrigger>(FindObjectsSortMode.None);
        foreach (var trig in allTriggers)
        {
            if (trig != null)
            {
                trig.ResetTrigger();
            }
        }

    }

    private void SetLighting(StageLoopData data)
    {
        // specified lighting settings applied to target list
        if (targetLights != null)
        {
            // ★ まず初期状態にリセット
            if (initialLightSettings != null)
            {
                for (int i = 0; i < targetLights.Count; i++)
                {
                    if (targetLights[i] == null) continue;

                    if (i < initialLightSettings.Count)
                    {
                        targetLights[i].color = initialLightSettings[i].color;
                        targetLights[i].intensity = initialLightSettings[i].intensity;
                    }
                }
            }

            // ★ 一括設定があれば適用 (優先順位: 低)
            if (data.useGlobalLightSetting)
            {
                for (int i = 0; i < targetLights.Count; i++)
                {
                    if (targetLights[i] == null) continue;
                    targetLights[i].color = data.globalLightSetting.color;
                    targetLights[i].intensity = data.globalLightSetting.intensity;
                }
            }

            // ★ その上で、現在の設定があれば上書き適用
            if (data.lightSettings != null)
            {
                for (int i = 0; i < targetLights.Count; i++)
                {
                    if (targetLights[i] == null) continue;

                    if (i < data.lightSettings.Count)
                    {
                        targetLights[i].color = data.lightSettings[i].color;
                        targetLights[i].intensity = data.lightSettings[i].intensity;
                    }
                }
            }
        }

        // 既存の照明設定 (RenderSettingsなど)
        LightingType type = data.lighting;

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

    // 13: 蜘蛛イベント
    public void TriggerSpiderWall()
    {
        if (spiderWallRootObject != null) spiderWallRootObject.SetActive(true); // Rootを表示

        if (spiderWallEvent != null)
        {
            Debug.Log("🕷️ [HorrorEventManager] 蜘蛛イベント(13)発生");
            spiderWallEvent.gameObject.SetActive(true);
            spiderWallEvent.PrepareEvent(); // 準備（壁汚れ消しなど）
            spiderWallEvent.ActivateEvent();
        }
        else
        {
            Debug.LogError("13: SpiderWallEventが設定されていません。");
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
        // 14はプレハブ生成なのでSetActive不要
        GameObject zombie = Instantiate(zombiePrefab, zombieSpawnPoint.position, zombieSpawnPoint.rotation);
        spawnedObjects.Add(zombie); // 追跡リストに追加
        
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
        }

        // ★ 3D音響設定（距離減衰）
        source.spatialBlend = 1.0f;
        source.minDistance = 1.0f;  // 1m以内は最大音量
        source.maxDistance = 10.0f; // 10m離れると聞こえなくなる（または最小）
        source.rolloffMode = AudioRolloffMode.Logarithmic;

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
            Debug.LogError("32: 血痕オブジェクト(bloodSplashObject)が設定されていません。Inspectorを確認してください。");
        }
    }

    // 45: ラジオから音がする
    public void TriggerRadio()
    {
        if (radioController != null)
        {
            radioController.gameObject.SetActive(true); // 初期状態でFalseにされているため有効化
            radioController.PlayRadioSequence();
        }
        else
        {
            Debug.LogError("45: ラジオコントローラーが設定されていません。");
        }
    }
    
    // 54: 物が落ちる (AudioSource/Object制御)
    public void TriggerFallEvent()
    {
        if (objectToFallTarget != null)
        {
            objectToFallTarget.gameObject.SetActive(true); // 念のためActive化
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
            spawnedObjects.Add(ball); // 追跡リストに追加
            
            // RollingBallコンポーネントを取得して転がす
            RollingBall56 rbScript = ball.GetComponent<RollingBall56>();
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
            handprintEventTarget.gameObject.SetActive(true);
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
        if (wallEyesRootObject != null) wallEyesRootObject.SetActive(true); // Rootを表示

        if (wallEyesEventTarget != null)
        {
            wallEyesEventTarget.gameObject.SetActive(true);
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
        if (bearMoveRootObject != null) bearMoveRootObject.SetActive(true); // Rootを表示

        if (bearMoveEventTarget != null)
        {
            bearMoveEventTarget.gameObject.SetActive(true);
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
        if (vanishingWomanRootObject != null) vanishingWomanRootObject.SetActive(true); // Rootを表示

        if (vanishingWomanEventTarget != null)
        {
            vanishingWomanEventTarget.gameObject.SetActive(true);
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
            zombieChaseEventTarget.gameObject.SetActive(true);
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
             // AudioSourceはComponentなのでGameObjectをActiveにする
            thunderAudioSource.gameObject.SetActive(true);
            thunderAudioSource.PlayOneShot(thunderSound, thunderVolume);
            
            // ★ 雷の点滅開始
            StartCoroutine(ThunderFlashSequence());
        }
        else
        {
            Debug.LogError("46: 雷のAudioSourceまたはAudioClipが設定されていません。");
        }
    }

    // ★ 雷の点滅コルーチン
    private System.Collections.IEnumerator ThunderFlashSequence()
    {
        if (thunderLights == null || thunderLights.Length == 0) yield break;

        // 元の強さを保存して、いったん微弱な光にする（真っ暗にはしない）
        float[] originalIntensities = new float[thunderLights.Length];
        for (int i = 0; i < thunderLights.Length; i++)
        {
            if (thunderLights[i] != null)
            {
                originalIntensities[i] = thunderLights[i].intensity;
                thunderLights[i].enabled = true;
                thunderLights[i].intensity = originalIntensities[i] * 0.2f; // 20%の明るさで待機
            }
        }

        // ビカビカさせる
        for (int i = 0; i < flashCount; i++)
        {
            // 点灯 (Flash)
            for (int j = 0; j < thunderLights.Length; j++)
            {
                if (thunderLights[j] != null)
                {
                    // 元の明るさ ～ 倍率の明るさ でランダム
                    thunderLights[j].intensity = UnityEngine.Random.Range(originalIntensities[j], originalIntensities[j] * thunderIntensityMultiplier);
                }
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.15f));

            // 減光 (Dim)
            for (int j = 0; j < thunderLights.Length; j++)
            {
                if (thunderLights[j] != null)
                {
                    thunderLights[j].intensity = originalIntensities[j] * 0.2f; // 20%に戻す
                }
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.2f));
        }

        // 最後は完全に元の状態に戻す
        for (int i = 0; i < thunderLights.Length; i++)
        {
            if (thunderLights[i] != null)
            {
                thunderLights[i].enabled = true;
                thunderLights[i].intensity = originalIntensities[i];
            }
        }
    }

    // 42: 笑い声
    public void TriggerLaugh()
    {
        if (laughAudioSource != null && laughSound != null)
        {
            Debug.Log("😈 笑い声が聞こえます...");
            laughAudioSource.gameObject.SetActive(true);
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
        if (clusterWallRootObject != null) clusterWallRootObject.SetActive(true); // Rootを表示

        if (clusterWallEvent != null)
        {
            clusterWallEvent.gameObject.SetActive(true);
            clusterWallEvent.TriggerEvent();
        }
    }

    // 44: 接近する人影
    public void TriggerApproachingPerson()
    {
        Debug.Log("🎃 [HorrorEventManager] TriggerApproachingPerson が呼ばれました");
        if (approachingPersonEvent != null)
        {
            approachingPersonEvent.gameObject.SetActive(true);
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
            soundFromLocationSource.dopplerLevel = 0f;
            soundFromLocationSource.spread = 0f;
            
            // ★ リバーブやバイパス設定を確認
            soundFromLocationSource.bypassListenerEffects = false;
            soundFromLocationSource.bypassEffects = false;

            // ★ 範囲設定 (Linearに変更して減衰を明確にする)
            soundFromLocationSource.minDistance = 1.0f;  
            soundFromLocationSource.maxDistance = 15.0f;   
            soundFromLocationSource.rolloffMode = AudioRolloffMode.Linear; // Linearの方が変化が分かりやすい

            if (!soundFromLocationSource.isPlaying)
            {
                soundFromLocationSource.gameObject.SetActive(true);
                soundFromLocationSource.Play();
                
                string camInfo = (Camera.main != null) ? Camera.main.transform.position.ToString() : "No Camera";
                Debug.Log($"🔊 [Event34] Audio Started. Source Pos: {soundFromLocationSource.transform.position}, Player(Cam) Pos: {camInfo}");
                
                if (Camera.main != null && Vector3.Distance(soundFromLocationSource.transform.position, Camera.main.transform.position) < 0.1f)
                {
                    Debug.LogWarning("⚠ [Event34] AudioSource is at the same position as Camera! It might be attached to the player or not positioned.");
                }
            }
        }
    }

    // 51: 通行人
    public void TriggerWalkingPerson()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerWalkingPerson が呼ばれました");
        if (walkingPersonEvent != null)
        {
            walkingPersonEvent.gameObject.SetActive(true);
            walkingPersonEvent.TriggerEvent();

            // ★ 足音再生（フェードイン・アウト制御）
            if (walkingPersonAudioSource != null && walkingPersonSound != null)
            {
                walkingPersonAudioSource.gameObject.SetActive(true);
                StartCoroutine(WalkingPersonSoundSequence());
            }
        }
        else
        {
             Debug.LogError("❌ [HorrorEventManager] walkingPersonEvent が割り当てられていません！Inspectorで設定してください。");
        }
    }

    private System.Collections.IEnumerator WalkingPersonSoundSequence()
    {
        walkingPersonAudioSource.clip = walkingPersonSound;
        walkingPersonAudioSource.volume = 0f;
        walkingPersonAudioSource.loop = true; // 途中で切れないようにループ設定
        walkingPersonAudioSource.Play();

        float timer = 0f;
        float fadeInDuration = 2.0f;
        float fadeOutStartTime = 5.0f;
        float fadeOutDuration = 2.0f;

        Debug.Log("👣 足音フェードイン開始");

        // 0秒〜5秒のループ
        while (timer < fadeOutStartTime + fadeOutDuration) // フェードアウト完了まで回す
        {
            timer += Time.deltaTime;

            if (timer <= fadeInDuration)
            {
                // フェードイン (0 -> 1)
                walkingPersonAudioSource.volume = Mathf.Clamp01(timer / fadeInDuration);
            }
            else if (timer >= fadeOutStartTime)
            {
                // フェードアウト開始 (1 -> 0)
                // fadeOutStartTimeからの経過時間
                float fadeOutTime = timer - fadeOutStartTime;
                walkingPersonAudioSource.volume = Mathf.Clamp01(1.0f - (fadeOutTime / fadeOutDuration));
                
                if (walkingPersonAudioSource.volume <= 0f) break; // 完全に消えたら終了
            }
            else
            {
                // 2秒〜5秒の間は最大音量維持
                walkingPersonAudioSource.volume = 1.0f;
            }

            yield return null;
        }

        walkingPersonAudioSource.Stop();
        walkingPersonAudioSource.volume = 1.0f; // 次回のために戻しておく（必要なら）
        // walkingPersonAudioSource.gameObject.SetActive(false); // 表示は維持するかもしれないのでコメントアウト、あるいは非表示にしてもよい
        Debug.Log("👣 足音終了");
    }

    // 52: ドアの隙間から女
    public void TriggerDoorGap()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerDoorGap が呼ばれました");
        if (doorGapEvent != null)
        {
            doorGapEvent.gameObject.SetActive(true);
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
            mirrorGhostEvent.gameObject.SetActive(true);
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


    // 12: 死体の腐敗（別バージョン）
    public void TriggerCorpseRot()
    {
        Debug.Log("🌑 [HorrorEventManager] TriggerCorpseRot (12) が呼ばれました");
        if (corpseRotTarget != null)
        {
            corpseRotTarget.SetActive(true);
        }
        else
        {
             Debug.LogError("❌ [HorrorEventManager] corpseRotTarget (Event 12) is not assigned!");
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

        // ファイル名: 被験者ID_04_HorrorEvent_log.csv
        string fileName = $"{subjectID}_04_stage2_HorrorEvent_log.csv";
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
        // ★ 追加: 既にログ済みなら何もしない
        if (currentLoopLoggedEvents.Contains(eventType))
        {
            if (debugMode) Debug.Log($"ℹ [HorrorEventManager] Event {eventType} log skipped (Duplicate).");
            return;
        }

        if (eventLogWriter != null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            eventLogWriter.WriteLine($"{timestamp},{eventType}");
            eventLogWriter.Flush(); // 即時書き込み
            
            // ★ 履歴に追加
            currentLoopLoggedEvents.Add(eventType);
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

    // ============================
    // ★ 進行判定 (ログがないと進めない)
    // ============================
    public bool CanProceedToNextLoop()
    {
        // スケジュールがない場合は進行可（または不可? 安全策で可）
        // if (currentStageSchedule.Count == 0) return true;
        
        Debug.Log($"🔍 [CanProceed] Checking... Cycle: {cycleCount}, ScheduleCount: {currentStageSchedule.Count}");

        // 現在のサイクルに対応するインデックスを取得
        // CycleCountは1始まり。インデックスは0始まり。
        int index = cycleCount - 1;
        if (index < 0) index = 0;

        // ループ5以降の挙動に合わせてインデックスを調整
        if (index >= currentStageSchedule.Count)
        {
            index = currentStageSchedule.Count - 1;
        }
        
        if (currentStageSchedule.Count == 0) return true;

        int currentEventID = currentStageSchedule[index].eventID;

        // 現在のイベントがログに記録されているか確認
        if (currentLoopLoggedEvents.Contains(currentEventID))
        {
            Debug.Log($"🔓 [CanProceed] Allowed. Event {currentEventID} is logged.");
            return true;
        }

        // ログがない場合は進行不可
        Debug.Log($"⛔ [HorrorEventManager] 進行不可: イベント {currentEventID} のログが記録されていません。 Logged Events: {string.Join(",", currentLoopLoggedEvents)}");
        return false;
    }
}
