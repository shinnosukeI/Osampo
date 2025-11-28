using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Fade System")]
    [SerializeField]
    private ScreenFader screenFader;

    // シーン遷移時のフェードタイプを保持
    private static FadeType nextFadeType = FadeType.Simple;

    // シーン名の定数定義
    public static class SceneNames
    {
        public const string Title = "ConfinementWalk";
        public const string Survey = "SurveyScene";
        public const string Rest1 = "RestScene1";
        public const string Rest2 = "RestScene2";
        public const string Stage1 = "Stage1";
        public const string Stage2 = "Stage2";
        public const string BPMTest1 = "99_BPMTestScene1";
        public const string BPMTest2 = "99_BPMTestScene2";
        public const string Result = "ResultScene"; 
    }

    void Awake()
    {
        if (screenFader == null)
        {
            screenFader = FindFirstObjectByType<ScreenFader>();
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンが切り替わったら、そのシーンにある新しいScreenFaderを探し直す
        screenFader = FindFirstObjectByType<ScreenFader>();

        if (screenFader != null)
        {
            // 記憶しておいたタイプでフェードイン開始
            screenFader.FadeIn(nextFadeType);
        }
    }

    /// <summary>
    /// フェードタイプを指定してシーン遷移 (Core Method)
    /// </summary>
    private void LoadSceneWithFade(string sceneName, FadeType type)
    {
        // 次のシーンのフェードインでも同じタイプを使うため記憶
        nextFadeType = type;

        if (screenFader != null)
        {
            screenFader.FadeOut(type, () =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // ========================================================================
    // ▼▼▼ Data Persistence (ResultScene用) ▼▼▼
    // ========================================================================
    // 平常時心拍数 (RestScene1で計測)
    public static float SavedRestBPM = 0f;
    
    // Stage1 (99_BPMTestScene1) の心拍数リスト
    public static List<int> SavedStage1BPMList = new List<int>();

    // Stage2 (99_BPMTestScene2) の心拍数リスト
    public static List<int> SavedStage2BPMList = new List<int>();

    // ========================================================================

    // 平常時心拍数 (Instance property - keep for compatibility if needed, or sync with static)
    public float BaseHeartRate { get; private set; }

    public void SetBaseHeartRate(float bpm)
    {
        BaseHeartRate = bpm;
        SavedRestBPM = bpm; // Static変数にも保存
        Debug.Log($"GameManager: 平常時心拍数を {BaseHeartRate} に設定しました。");
    }

    /// <summary>
    /// SurveyManagerからアンケート結果を受け取る関数
    /// </summary>
    public void ReceiveSurveyResult(int surveyResult)
    {
        Debug.Log($"アンケート結果 {surveyResult} を受け取りました。");
        LoadRestScene1();
    }

    // ========================================================================
    // ▼▼▼ Scene Loading Wrappers (Inspector / Public API) ▼▼▼
    // ========================================================================

    public void LoadStage1() => LoadSceneWithFade(SceneNames.Stage1, FadeType.Noise);
    public void LoadStage2() => LoadSceneWithFade(SceneNames.Stage2, FadeType.Noise);
    public void LoadSurveyScene() => LoadSceneWithFade(SceneNames.Survey, FadeType.Noise);
    public void LoadRestScene1() => LoadSceneWithFade(SceneNames.Rest1, FadeType.Simple);
    public void LoadRestScene2() => LoadSceneWithFade(SceneNames.Rest2, FadeType.Simple);
    public void LoadConfinementWalk() => LoadSceneWithFade(SceneNames.Title, FadeType.Noise);
    public void LoadResultScene() => LoadSceneWithFade(SceneNames.Result, FadeType.Simple); // 追加
    
    // Stage1の変わり身
    public void LoadBPMtest1() => LoadSceneWithFade(SceneNames.BPMTest1, FadeType.Noise);
    // Stage2の変わり身
    public void LoadBPMtest2() => LoadSceneWithFade(SceneNames.BPMTest2, FadeType.Noise);
    
    // 文字列でシーン名を指定して遷移する汎用メソッド
    public void LoadTargetScene(string sceneName)
    {
        LoadSceneWithFade(sceneName, FadeType.Noise);
    }

    // 現在のシーンをリロードするメソッド
    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadSceneWithFade(currentScene, FadeType.Simple);
    }
}
