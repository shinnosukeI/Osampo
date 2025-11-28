using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SurveyManager : MonoBehaviour
{
    [SerializeField] private ToggleGroup optionsToggleGroup;
    [SerializeField] private Button submitButton;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject warningMessageObj;
    [SerializeField] private AudioClip surveyBGM;

    private int selectedOptionId = -1;

    private void Start()
    {
        if (warningMessageObj != null)
        {
            warningMessageObj.SetActive(false);
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager が見つかりません！");
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(surveyBGM);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitSurvey);
        }
        else
        {
            Debug.LogError("SubmitButton が SurveyManager に設定されていません！");
        }
    }

    public void SelectOption(int optionId)
    {
        selectedOptionId = optionId;
        Debug.Log($"選択肢 {selectedOptionId} が選ばれました。");

        // 一度でも選択したら、もう「未選択(Switch Off)」にはできないようにする
        if (optionsToggleGroup != null)
        {
            optionsToggleGroup.allowSwitchOff = false;
        }

        // 選択肢を選んだら、警告メッセージが出ていれば消す 
        if (warningMessageObj != null)
        {
            warningMessageObj.SetActive(false);
        }
    }

    public void SubmitSurvey()
    {
        // 未選択（-1）の場合
        if (selectedOptionId == -1)
        {
            Debug.LogWarning("まだ選択肢が選ばれていません。");

            // 警告メッセージを表示
            if (warningMessageObj != null)
            {
                warningMessageObj.SetActive(true);
            }
            return;
        }

        // 成功時のSE再生 (SoundManagerがあれば)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCommonButtonSE();
        }

        if (gameManager != null)
        {
            gameManager.ReceiveSurveyResult(selectedOptionId);
            Debug.Log($"GameManager に {selectedOptionId} を送信しました。");
        }
        else
        {
            // Startで取得失敗した場合の保険
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.ReceiveSurveyResult(selectedOptionId);
                Debug.Log($"再取得したGameManager に {selectedOptionId} を送信しました。");
            }
            else
            {
                Debug.LogError("GameManagerが存在しないため、遷移できません。");
            }
        }
    }
}