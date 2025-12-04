using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems; // EventSystem操作用
using UnityEngine.SceneManagement; // Fallback用
using System.IO; // スクリーンショット保存用
using System.Collections; // コルーチン用

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
    [SerializeField] private TextMeshProUGUI fearEvaluationText; // 追加: 判定結果表示用

    [Header("System")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioClip resultBGM; // 追加: ResultSceneで流すBGM

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

        // 2. BGM再生 (SoundManager経由)
        if (SoundManager.Instance != null && resultBGM != null)
        {
            SoundManager.Instance.PlayBGM(resultBGM);
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

        // 4. スクリーンショット保存 (自動)
        StartCoroutine(SaveResultScreenshotSequence());
    }

    private void DisplayResults()
    {
        // 1. 平常時心拍数
        if (restBpmText != null)
        {
            restBpmText.text = $"{GameManager.SavedRestBPM}";
        }

        // 2. Stage1 (99_BPMTestScene1) のランキング
        // 変更: 単純な上位ではなく、ピーク（山）のトップ3を取得する
        List<int> stage1Ranks = CalculateTopPeaks(GameManager.SavedStage1BPMList, 3);
        
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
        // Stage2は現状維持（単純な最大値で良いか確認が必要だが、指示はStage1のみだったので既存ロジック）
        List<int> stage2Ranks = CalculateTopRanks(GameManager.SavedStage2BPMList, 1);

        if (stage2Rank1Text != null) stage2Rank1Text.text = stage2Ranks.Count > 0 ? $"{stage2Ranks[0]}" : "-";

        // Stage2 平均
        if (stage2AverageText != null)
        {
            float avg2 = CalculateAverage(GameManager.SavedStage2BPMList);
            stage2AverageText.text = $"{avg2}";
        }

        // 4. 自己申告恐怖と生理的恐怖反応の判定
        if (fearEvaluationText != null)
        {
            // Stage2のトップ値 (なければ0)
            int stage2Top = stage2Ranks.Count > 0 ? stage2Ranks[0] : 0;

            // Stage1のトップ3 (不足分は0埋めして比較用にリスト化)
            List<int> comparisonList = new List<int>(stage1Ranks);
            while (comparisonList.Count < 3) comparisonList.Add(0);

            // 比較対象の4つの値: Stage2Top, Stage1Rank1, Stage1Rank2, Stage1Rank3
            // これらを降順ソートして、Stage2Topが何番目に来るかを見る
            // ただし、Stage2TopがStage1の値と同じ場合は「より上位」とみなす（＝一致寄り）
            
            // 判定ロジック:
            // Stage2Top >= Stage1Rank1 -> 1位 (一致)
            // Stage2Top >= Stage1Rank2 -> 2位 (やや一致)
            // Stage2Top >= Stage1Rank3 -> 3位 (やや不一致)
            // Stage2Top < Stage1Rank3  -> 4位 (不一致)

            string evaluation = "判定不能";
            
            // データが無い場合は判定不能のままにするか、あるいは "-" にするか
            if (stage2Ranks.Count == 0 && stage1Ranks.Count == 0)
            {
                evaluation = "-";
            }
            else
            {
                int s1_1 = comparisonList[0];
                int s1_2 = comparisonList[1];
                int s1_3 = comparisonList[2];

                if (stage2Top >= s1_1)
                {
                    evaluation = "一致";
                }
                else if (stage2Top >= s1_2)
                {
                    evaluation = "やや一致";
                }
                else if (stage2Top >= s1_3)
                {
                    evaluation = "やや不一致";
                }
                else
                {
                    evaluation = "不一致";
                }
            }

            fearEvaluationText.text = evaluation;
            Debug.Log($"【ResultScene】判定結果: {evaluation} (Stage2Top: {stage2Top}, S1: {string.Join(",", comparisonList)})");
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
    /// 心拍数の「ピーク（極大値）」を探し、その上位を取得する
    /// ピークが足りない場合は、残りを単純な上位値で埋める
    /// </summary>
    private List<int> CalculateTopPeaks(List<int> rawBpmList, int count)
    {
        if (rawBpmList == null || rawBpmList.Count == 0) return new List<int>();

        // 1. まず連続重複を除去
        List<int> compressed = new List<int>();
        compressed.Add(rawBpmList[0]);
        for (int i = 1; i < rawBpmList.Count; i++)
        {
            if (rawBpmList[i] != rawBpmList[i - 1])
            {
                compressed.Add(rawBpmList[i]);
            }
        }

        // データが少なすぎる場合は単純な上位を返す
        if (compressed.Count < 3)
        {
            return compressed.Distinct().OrderByDescending(x => x).Take(count).ToList();
        }

        // 2. 極大値（ピーク）を探す
        HashSet<int> peaks = new HashSet<int>();
        for (int i = 1; i < compressed.Count - 1; i++)
        {
            int prev = compressed[i - 1];
            int current = compressed[i];
            int next = compressed[i + 1];

            // 山の頂点
            if (current > prev && current > next)
            {
                peaks.Add(current);
            }
        }

        // 3. 結果リストを作成
        // まずはピークを採用
        List<int> result = peaks.ToList();

        // 足りない分を非ピーク（全体）から補充
        if (result.Count < count)
        {
            // ピーク以外の値を降順ソートして取得
            var others = compressed.Where(x => !peaks.Contains(x))
                                   .Distinct()
                                   .OrderByDescending(x => x);
            
            foreach (var val in others)
            {
                if (result.Count >= count) break;
                result.Add(val);
            }
        }

        // 4. 最終的に降順ソートして上位count個を返す
        // (ピークと補充した値を混ぜてランキングにする)
        return result.OrderByDescending(x => x).Take(count).ToList();
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

        // SE再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayCommonButtonSE();
        }

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

    /// <summary>
    /// 結果画面のスクリーンショットを保存するコルーチン
    /// </summary>
    private IEnumerator SaveResultScreenshotSequence()
    {
        // フェードイン完了を待つために0.8秒待機
        yield return new WaitForSeconds(0.8f);
        // UI描画完了を待つ
        yield return new WaitForEndOfFrame();

        // 保存先ディレクトリ: Application.persistentDataPath/ScreenShots
        string directoryPath = Path.Combine(Application.persistentDataPath, "ScreenShots");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // ファイル名: [SubjectID]_Result_[Timestamp].png
        string subjectID = string.IsNullOrEmpty(GameManager.SubjectID) ? "NoID" : GameManager.SubjectID;
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{subjectID}_Result_{timestamp}.png";
        string fullPath = Path.Combine(directoryPath, fileName);

        // スクリーンショット撮影
        ScreenCapture.CaptureScreenshot(fullPath);

        Debug.Log($"【ResultScene】スクリーンショット保存完了: {fullPath}");
    }
}
