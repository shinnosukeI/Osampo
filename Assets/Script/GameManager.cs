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

    // 【修正5】staticに変更して、シーンが変わっても値を保持するようにする
    // これにより、LoadStage1でNoiseを指定→次のシーンでNoiseでフェードインが可能になる
    private static FadeType nextFadeType = FadeType.Simple;

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
        // ▼▼▼ 追加: シーンが切り替わったら、そのシーンにある新しいScreenFaderを探し直す ▼▼▼
        screenFader = FindFirstObjectByType<ScreenFader>();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        if (screenFader != null)
        {
            // 記憶しておいたタイプでフェードイン開始
            screenFader.FadeIn(nextFadeType);
        }
    }

    /// <summary>
    /// フェードタイプを指定してシーン遷移
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

    // 平常時心拍数
    public float BaseHeartRate { get; private set; }
    /// SurveyManagerからアンケート結果 (1～5) を受け取る関数
    /// </summary>
    /// <param name="surveyResult">受け取った選択肢ID (1, 2, 3, 4, 5 のいずれか)</param>
    public void ReceiveSurveyResult(int surveyResult)
    {
        Debug.Log($"アンケート結果 {surveyResult} を受け取りました。");

        switch (surveyResult)
        {
            case 1:
                // 異形・クリーチャー的恐怖
                break;
            case 2:
                // 人体・人形的恐怖
                break;
            case 3:
                // 生理的嫌悪・外傷的恐怖
                break;
            case 4:
                // 心理的・行動的恐怖
                break;
            case 5:
                // 超常的な恐怖
                break;
            default:
                Debug.LogWarning("不明なIDが送信されました。");
                break;
        }

        // ロード画面１に移動する
        LoadSceneWithFade("RestScene1", FadeType.Simple);
    }

    public void SetBaseHeartRate(float bpm)
    {
        BaseHeartRate = bpm;
        Debug.Log($"GameManager: 平常時心拍数を {BaseHeartRate} に設定しました。");
    }

    //stage1に移動
    public void LoadStage1()
    {
        LoadSceneWithFade("Stage1", FadeType.Noise);
    }

    //stage2に移動
    public void LoadStage2()
    {
        LoadSceneWithFade("Stage2", FadeType.Noise);
    }

    public void LoadSurveyScene()
    {
        LoadSceneWithFade("SurveyScene", FadeType.Noise);
    }

    public void LoadRestScene1()
    {
        LoadSceneWithFade("RestScene1", FadeType.Simple);
    }

    public void LoadRestScene2()
    {
        LoadSceneWithFade("RestScene2", FadeType.Simple);
    }

    public void LoadConfinementWalk()
    {
        LoadSceneWithFade("ConfinementWalk", FadeType.Noise);
    }

    public void LoadBPMtest1()
    {
        LoadSceneWithFade("99_BPMTestScene1", FadeType.Noise);
    }
    
    public void LoadBPMtest2()
    {
        LoadSceneWithFade("99_BPMTestScene2", FadeType.Noise);
    }
}
