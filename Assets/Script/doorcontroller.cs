using UnityEngine;

public class DoorController : MonoBehaviour, IFocusable
{
    public float openAngle = 90f;
    public float speed = 2f;

    [Header("Manual Control")]
    [SerializeField] private bool disableManualControl = false; // ★チェックで手動無効

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Coroutine autoCloseCoroutine;

    public bool HasBeenInteracted { get; private set; } = false;

    public void ResetInteraction() => HasBeenInteracted = false;

    // ★ Awakeに変更: 他のスクリプトより先に初期位置を確保するため
    void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = Quaternion.Euler(transform.localEulerAngles + new Vector3(0f, openAngle, 0f));
    }

    void Start()
    {
        // Startは空でも良い、あるいは初期化ロジックがあれば書く
    }

    public bool SkipUpdate { get; set; } = false; // ★外部制御用: Updateをスキップするか

    void Update()
    {
        if (SkipUpdate) return; // ★スキップフラグが立っていたら何もしない（他のスクリプトで制御中）

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            isOpen ? openRotation : closedRotation,
            Time.deltaTime * speed
        );
    }

    public IFocusable FocusOverride { get; set; }

    public void OnFocus()
    {
        if (FocusOverride != null) { FocusOverride.OnFocus(); return; }
        if (!enabled) return;

        // ★手動操作禁止なら反応しない
        if (disableManualControl) return;

        ToggleDoor();
    }

    public void ToggleDoor()
    {
        // ★念のためここでもブロック（外部からToggle呼ばれても無効）
        if (disableManualControl) return;

        // ★ イベントロック確認
        if (!CheckEventLock()) return;

        HasBeenInteracted = true;
        isOpen = !isOpen;

        StopAutoCloseIfNeeded();

        if (isOpen)
            autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void OpenDoor()
    {
        // ★ イベントロック確認
        if (!CheckEventLock()) return;

        HasBeenInteracted = true;
        if (isOpen) return;
        isOpen = true;

        StopAutoCloseIfNeeded();
        autoCloseCoroutine = StartCoroutine(AutoClose());
    }

    public void CloseDoor()
    {
        HasBeenInteracted = true;
        if (!isOpen) return;
        isOpen = false;

        StopAutoCloseIfNeeded();
    }

    private void StopAutoCloseIfNeeded()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private System.Collections.IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(3f);
        isOpen = false;
        autoCloseCoroutine = null;
    }

    public Quaternion ClosedRotation => closedRotation;
    public Quaternion OpenRotation => openRotation;
    public bool IsOpen => isOpen;

    // （任意）外部から切り替えたい時用
    public bool DisableManualControl
    {
        get => disableManualControl;
        set => disableManualControl = value;
    }

    // ★ 追加: イベントログがないと開かない機能
    [Header("Event Lock Settings")]
    [SerializeField] private bool requireEventLogged = false; // ★ これをONにすると、その周のイベントを見ていないと開かない
    [SerializeField] private HorrorEventManager eventManager; // ★ マネージャー参照

    // ★ 外部からセットアップできるようにする
    public void SetupEventLock(HorrorEventManager manager, bool require)
    {
        eventManager = manager;
        requireEventLogged = require;
    }

    // ★ 追加: 特定のイベント中のみ有効（それ以外はフォーカス不可＝Script Disabled）にする
    [Header("Event Restriction")]
    [SerializeField] private int restrictedToEventID = 0; // 0なら制限なし

    public void UpdateEventRestriction(int currentEventID)
    {
        if (restrictedToEventID == 0) return; // 制限なし

        if (restrictedToEventID == currentEventID)
        {
            // 指定イベント中なので有効化
            this.enabled = true;
        }
        else
        {
            // 指定イベントではないので無効化（フォーカスも外れる）
            // 開いていたら閉じる
            if (isOpen)
            {
                isOpen = false;
                transform.localRotation = closedRotation;
                StopAutoCloseIfNeeded();
            }
            this.enabled = false;
        }
    }

    private bool CheckEventLock()
    {
        if (!requireEventLogged) return true; // 制限なしならOK

        if (eventManager == null)
        {
            // マネージャーが見つからない場合は自動検索してみる
            eventManager = FindAnyObjectByType<HorrorEventManager>();
        }

        if (eventManager != null)
        {
            if (!eventManager.CanProceedToNextLoop())
            {
                Debug.Log("🔒 [DoorController] イベント未確認のためドアを開けません。");
                ShowLockedMessage(); // ★ メッセージ表示
                return false;
            }
        }
        return true;
    }

    // ★ ロック時のメッセージ表示
    private void ShowLockedMessage()
    {
        // 簡易的に CrosshairCanvas のテキストを使用
        var canvas = GameObject.Find("CrosshairCanvas");
        if (canvas != null)
        {
            var textComp = canvas.GetComponentInChildren<UnityEngine.UI.Text>();
            if (textComp != null)
            {
                // コルーチンで表示制御
                StopCoroutine("ShowMessageCoroutine"); // 既存があれば止める (名前指定停止は推奨されないが簡易実装)
                StartCoroutine(ShowMessageCoroutine(textComp, "開かない・・・"));
            }
        }
    }

    private System.Collections.IEnumerator ShowMessageCoroutine(UnityEngine.UI.Text textUI, string message)
    {
        string originalText = "右クリックでアクション"; // デフォルト
        textUI.text = message;
        textUI.color = Color.red; // 赤色で強調
        
        yield return new WaitForSeconds(2.0f);

        // 元に戻す（ただし、他の要因でテキストが変わっている可能性もあるので注意）
        // 今回はシンプルにデフォルトに戻す
        textUI.text = originalText;
        textUI.color = Color.white;
    }
}