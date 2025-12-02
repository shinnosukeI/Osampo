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
    [SerializeField] private GameManager gameManager;

    void Start()
    {
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

        if (gameManager != null)
        {
            // 新しいゲームを始めるので、前回のデータをリセット
            gameManager.ResetData();
            gameManager.LoadSurveyScene();
        }
        else
        {
            Debug.LogError("TitleManager: GameManager is null, cannot load SurveyScene.");
        }
    }
}