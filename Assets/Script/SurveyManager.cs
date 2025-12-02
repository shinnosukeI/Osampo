using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

        // ラベルをクリック可能にする処理
        if (optionsToggleGroup != null)
        {
            // ToggleGroupに所属するすべてのToggleを取得
            Toggle[] toggles = optionsToggleGroup.GetComponentsInChildren<Toggle>();
            
            foreach (var toggle in toggles)
            {
                // Toggleの子要素にあるTextを探す (uGUI Text)
                Text labelText = toggle.GetComponentInChildren<Text>();
                if (labelText != null)
                {
                    CreateClickableOverlay(labelText.gameObject, toggle);
                }
                else
                {
                    // TextMeshProの場合も考慮
                    TMP_Text tmpText = toggle.GetComponentInChildren<TMP_Text>();
                    if (tmpText != null)
                    {
                        CreateClickableOverlay(tmpText.gameObject, toggle);
                    }
                }
            }
        }
    }

    private void CreateClickableOverlay(GameObject parent, Toggle toggle)
    {
        // クリック判定用の透明なImageを持つ子オブジェクトを作成
        GameObject overlay = new GameObject("ClickableOverlay");
        overlay.transform.SetParent(parent.transform, false);

        // RectTransformを親いっぱいに広げる
        RectTransform rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 透明なImageを追加 (Raycast Targetになる)
        Image image = overlay.AddComponent<Image>();
        image.color = Color.clear;

        // ClickableLabelを追加
        ClickableLabel clickable = overlay.AddComponent<ClickableLabel>();
        clickable.Setup(toggle);
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