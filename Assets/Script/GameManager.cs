using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("RestScene1");
    }

    public void SetBaseHeartRate(float bpm)
    {
        BaseHeartRate = bpm;
        Debug.Log($"GameManager: 平常時心拍数を {BaseHeartRate} に設定しました。");
    }

    //stage1に移動
    public void LoadStage1()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1");
    }

    //stage2に移動
    public void LoadStage2()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Stage2");
    }

    public void LoadSurveyScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SurveyScene");
    }

    public void LoadRestScene1()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("RestScene1");
    }
    
    public void LoadConfinementWalk()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ConfinementWalk");
    }
}
