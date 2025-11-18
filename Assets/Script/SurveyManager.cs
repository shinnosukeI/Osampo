using UnityEngine;
using UnityEngine.UI; // ToggleGroup や Button を使うために必要
using UnityEngine.SceneManagement;

/// <summary>
/// アンケート画面の選択(1～5)を管理し、GameManagerに送信するクラス
/// </summary>
public class SurveyManager : MonoBehaviour
{
    // ▼▼▼ 【名前の確認ポイント 1】 ▼▼▼
    // UnityのHierarchy上にある「Toggle Group」コンポーネントが
    // 付いているオブジェクト（例: OptionsGroup）をインスペクタから設定します。
    [SerializeField]
    private ToggleGroup optionsToggleGroup;

    // ▼▼▼ 【名前の確認ポイント 2】 ▼▼▼
    // Hierarchy上にある「Next」ボタン（例: NextButton）を
    // インスペクタから設定します。
    [SerializeField]
    private Button submitButton;

    // ▼▼▼ 【名前の確認ポイント 3】 ▼▼▼
    // Hierarchy上にある「GameManager」オブジェクトを
    // インスペクタから設定します。
    [SerializeField]
    private GameManager gameManager;

    // 選択された選択肢のID (1～5) を保持する変数
    // 初期値は -1 (または 0) にして「未選択」状態とします。
    private int selectedOptionId = -1;

    private void Start()
    {
        // GameManager がインスペクタで設定されていなければ探す
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager が見つかりません！");
            return;
        }

        // submitButton (Nextボタン) が押されたら SubmitSurvey() 関数を呼ぶように登録
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(SubmitSurvey);
        }
        else
        {
            // 【名前の確認ポイント 2】で設定したボタンが見つからない場合
            Debug.LogError("SubmitButton が SurveyManager に設定されていません！");
        }
    }

    /// <summary>
    /// Toggleから呼び出される関数。選択されたID(1～5)を保持する。
    /// </summary>
    /// <param name="optionId">選択肢のID (1～5)</param>
    public void SelectOption(int optionId)
    {
        selectedOptionId = optionId;
        Debug.Log($"選択肢 {selectedOptionId} が選ばれました。");
    }

    /// <summary>
    /// ボタンから呼び出される関数。GameManagerに値(1～5)を送信する。
    /// </summary>
    public void SubmitSurvey()
    {
        // 未選択（-1）の場合は送信しない
        if (selectedOptionId == -1)
        {
            Debug.LogWarning("まだ選択肢が選ばれていません。");
            // ここで「選択してください」などのUIを出すこともできます
            return;
        }

        // GameManager に 1～5 の値を送信
        if (gameManager != null)
        {
            gameManager.ReceiveSurveyResult(selectedOptionId);
            Debug.Log($"GameManager に {selectedOptionId} を送信しました。");
        }
        else
        {
            Debug.LogError("GameManager が見つかりません！");
        }

        // （任意）送信後にこのアンケート画面のCanvasを非表示にするなど
        // this.gameObject.SetActive(false);
    }
    
}