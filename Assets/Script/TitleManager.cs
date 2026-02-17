using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI; // Button用

public class TitleManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip titleBGM; // インスペクタで設定するBGM

    [Header("UI References")]
    [SerializeField] private Button startButton; // Startボタンの参照
    [SerializeField] private Button settingsButton; // Settingsボタンの参照
    [SerializeField] private GameObject settingsPanel; // Settingsパネルの参照
    [SerializeField] private GameManager gameManager;

    [Header("Debug Settings")]
    [SerializeField] private bool enableNoHeartRateMode = false;

    void Start()
    {
        // 0. テストモード設定の適用
        GameManager.IsNoHeartRateMode = enableNoHeartRateMode;
        if (enableNoHeartRateMode)
        {
            Debug.LogWarning("【TitleManager】心拍計なしテストモードが有効です。センサー接続確認はスキップされます。");
        }

        // 1. カーソルを強制的に表示・ロック解除
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 2. EventSystemの存在確認とInputModuleの修正
        EventSystem es = FindFirstObjectByType<EventSystem>();
        if (es == null)
        {
            Debug.LogWarning("TitleManager: EventSystem not found. Creating one dynamically.");
            GameObject eventSystemGO = new GameObject("EventSystem");
            es = eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<InputSystemUIInputModule>();
        }
        else
        {
            // 既存のEventSystemがある場合、InputModuleを確認・入替
            if (es.GetComponent<StandaloneInputModule>() != null)
            {
                Debug.LogWarning("TitleManager: StandaloneInputModule detected. Replacing with InputSystemUIInputModule.");
                Destroy(es.GetComponent<StandaloneInputModule>());
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            else if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                // 何もついてない場合
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }

        // 3. GameManagerの取得
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogWarning("TitleManager: GameManager not found in Start!");
            }
        }

        // 4. Startボタンのセットアップ
        if (startButton == null)
        {
            // 名前で探してみる (もしインスペクタで設定されていなければ)
            GameObject btnObj = GameObject.Find("StartButton");
            if (btnObj != null) startButton = btnObj.GetComponent<Button>();
        }

        if (startButton != null)
        {
            // 既存のリスナーを削除して重複を防ぐ（念のため）
            startButton.onClick.RemoveAllListeners();
            
            // 新しいリスナーを追加
            startButton.onClick.AddListener(OnStartButtonClicked);
            Debug.Log("TitleManager: Start button listener assigned.");
        }
        else
        {
            Debug.LogError("TitleManager: Start button not found!");
        }

        // 5. Settingsボタンのセットアップ
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }
        
        // パネルは最初は非表示
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // SoundManager経由でBGMを再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(titleBGM);
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("TitleManager: Start button clicked!");
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayTitleStartSE();
        }

        // Fallback: GameManagerがnullなら再取得を試みる
        if (gameManager == null)
        {
            Debug.Log("TitleManager: GameManager is null. Attempting to find it...");
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager != null)
        {
            // 新しいゲームを始めるので、前回のデータをリセット
            gameManager.ResetData();
            gameManager.LoadRestScene1();
        }
        else
        {
            Debug.LogError("TitleManager: GameManager is null, cannot load SurveyScene.");
        }
    }

    private void OnSettingsButtonClicked()
    {
        Debug.Log("TitleManager: Settings button clicked!");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCommonButtonSE();
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            // パネルを開く際に、Backボタンのアニメーションをリセットする
            SettingsPanelController controller = settingsPanel.GetComponent<SettingsPanelController>();
            if (controller != null)
            {
                controller.ResetBackButtonAnimation();
            }

            // Configボタンのアニメーション・選択状態を強制リセットする
            if (settingsButton != null)
            {
                // 1. 選択解除（Selected状態だとNormalアニメーションに戻らない場合があるため）
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }

                // 2. Animatorリセット
                Animator btnAnimator = settingsButton.GetComponent<Animator>();
                if (btnAnimator != null)
                {
                    btnAnimator.Rebind();
                    btnAnimator.Update(0f);
                }

                // 3. スケール強制リセット（Animatorの反映待ちを防ぐ）
                settingsButton.transform.localScale = Vector3.one;
            }
        }
        else
        {
            Debug.LogWarning("TitleManager: SettingsPanel is not assigned!");
        }
    }
}