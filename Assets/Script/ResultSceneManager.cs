using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems; // EventSystem操作用
using UnityEngine.SceneManagement; // Fallback用

public class ResultSceneManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI restBpmText;
    [SerializeField] private TextMeshProUGUI stage1Rank1Text;
    [SerializeField] private TextMeshProUGUI stage1Rank2Text;
    [SerializeField] private TextMeshProUGUI stage1Rank3Text;
    [SerializeField] private TextMeshProUGUI stage1AverageText; 
    [SerializeField] private TextMeshProUGUI stage2Rank1Text;
    [SerializeField] private TextMeshProUGUI stage2AverageText; 

    [Header("System")]
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        // 1. EventSystemの存在確認と自動生成 (UI操作不能を防ぐ)
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning("ResultSceneManager: EventSystem not found. Creating one dynamically.");
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
        }

        // 3. GameManagerの取得 (ロバスト性向上)
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("ResultSceneManager: GameManager not found in scene!");
            }
        }

        DisplayResults();
    }

    private void DisplayResults()
    {
        // 1. 平常時心拍数
        if (restBpmText != null)
        {
            restBpmText.text = $"{GameManager.SavedRestBPM}";
        }

        // 2. Stage1 (99_BPMTestScene1) のランキング
        List<int> stage1Ranks = CalculateTopRanks(GameManager.SavedStage1BPMList, 3);
        
        if (stage1Rank1Text != null) stage1Rank1Text.text = stage1Ranks.Count > 0 ? $"{stage1Ranks[0]}" : "-";
        if (stage1Rank2Text != null) stage1Rank2Text.text = stage1Ranks.Count > 1 ? $"{stage1Ranks[1]}" : "-";
        if (stage1Rank3Text != null) stage1Rank3Text.text = stage1Ranks.Count > 2 ? $"{stage1Ranks[2]}" : "-";

        // Stage1 平均
        if (stage1AverageText != null)
        {
            float avg1 = CalculateAverage(GameManager.SavedStage1BPMList);
            stage1AverageText.text = $"{avg1}";
        }

        // 3. Stage2 (99_BPMTestScene2) のランキング (1位のみ)
        List<int> stage2Ranks = CalculateTopRanks(GameManager.SavedStage2BPMList, 1);

        if (stage2Rank1Text != null) stage2Rank1Text.text = stage2Ranks.Count > 0 ? $"{stage2Ranks[0]}" : "-";

        // Stage2 平均
        if (stage2AverageText != null)
        {
            float avg2 = CalculateAverage(GameManager.SavedStage2BPMList);
            stage2AverageText.text = $"{avg2}";
        }
    }

    /// <summary>
    /// 平均値を計算する (小数点第1位まで)
    /// </summary>
    private float CalculateAverage(List<int> bpmList)
    {
        if (bpmList == null || bpmList.Count == 0) return 0f;
        
        double avg = bpmList.Average();
        return (float)System.Math.Round(avg, 1, System.MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// 心拍数リストから上位ランクを計算する
    /// ルール: 連続した同じ値は1つとみなす。同率の値は重複しない。
    /// </summary>
    private List<int> CalculateTopRanks(List<int> rawBpmList, int count)
    {
        if (rawBpmList == null || rawBpmList.Count == 0) return new List<int>();

        // ステップ1: 連続した重複を除去 (例: 100, 100, 95, 100 -> 100, 95, 100)
        // ※ ユーザー要望「連続した心拍数は一つの値として見てほしい」
        List<int> compressedList = new List<int>();
        if (rawBpmList.Count > 0)
        {
            compressedList.Add(rawBpmList[0]);
            for (int i = 1; i < rawBpmList.Count; i++)
            {
                if (rawBpmList[i] != rawBpmList[i - 1])
                {
                    compressedList.Add(rawBpmList[i]);
                }
            }
        }

        // ステップ2: ユニークな値を取得して降順ソート
        // ※ ユーザー要望「同率の値は重複しないでほしい」
        //    (例: 98が複数回あっても、2位と3位に98が入るのはNG -> Distinctで解決)
        var rankedList = compressedList.Distinct().OrderByDescending(x => x).Take(count).ToList();

        return rankedList;
    }

    // Exitボタンから呼ばれる
    public void OnExitButtonClicked()
    {
        // 誤爆防止: ResultScene以外では実行しない
        if (SceneManager.GetActiveScene().name != "ResultScene")
        {
            Debug.LogWarning($"ResultSceneManager: Ignoring Exit button click in {SceneManager.GetActiveScene().name}");
            return;
        }

        Debug.Log("ResultSceneManager: Exit button clicked.");

        // GameManager経由で遷移を試みる
        if (gameManager != null)
        {
            gameManager.LoadConfinementWalk(); 
        }
        else
        {
            // Fallback: GameManagerが見つからない場合、静かに再取得を試みる
            gameManager = FindFirstObjectByType<GameManager>();

            if (gameManager != null)
            {
                Debug.Log("ResultSceneManager: Found GameManager via fallback.");
                gameManager.LoadConfinementWalk();
            }
            else
            {
                // Fallback: それでもダメなら直接シーン遷移
                Debug.LogError("ResultSceneManager: GameManager not found! Loading Title scene directly.");
                SceneManager.LoadScene("ConfinementWalk");
            }
        }
    }
}
