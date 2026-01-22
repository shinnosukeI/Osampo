using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GraphRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Sprite dotSprite; // グラフの点の画像 (Noneでも可)
    [SerializeField] private Color graphColor = Color.white;
    [SerializeField] private float dotSize = 10f;
    [SerializeField] private float lineWidth = 2f;

    [Header("Container")]
    [SerializeField] private RectTransform graphContainer; // グラフを描画する親オブジェクト

    private void Awake()
    {
        // コンテナが未設定なら自分自身を使う
        if (graphContainer == null)
        {
            graphContainer = GetComponent<RectTransform>();
        }
    }
    [Header("Event Markers")]
    [SerializeField] private Color eventMarkerColor = Color.red;
    [SerializeField] private float eventMarkerSize = 15f; // 少し大きめ

    /// <summary>
    /// 心拍数リストを受け取り、グラフを描画する
    /// events: イベントが発生したインデックスとイベントIDのリスト (オプション)
    /// </summary>
    public void ShowGraph(List<int> bpmList, List<(int index, int eventId)> events = null)
    {
        // 既存のグラフをクリア
        foreach (Transform child in graphContainer)
        {
            Destroy(child.gameObject);
        }

        // 先頭の連続する60(デフォルト値)をトリミングするか判定
        int startIndex = 0;
        for (int i = 0; i < bpmList.Count; i++)
        {
            if (bpmList[i] != 60)
            {
                // 最初の非60の値を見つけた
                // その値と60の差が5以上(<=55 or >=65)なら、そこまでをデフォルト値とみなしてスキップ
                if (Mathf.Abs(bpmList[i] - 60) >= 5)
                {
                    startIndex = i;
                }
                break;
            }
        }

        // 描画用のリストを作成
        var drawList = bpmList.Skip(startIndex).ToList();

        if (drawList == null || drawList.Count < 2)
        {
            return;
        }

        // イベントインデックスの調整
        List<(int index, int eventId)> adjustedEvents = null;
        if (events != null)
        {
            adjustedEvents = new List<(int index, int eventId)>();
            foreach (var evt in events)
            {
                int newIndex = evt.index - startIndex;
                if (newIndex >= 0 && newIndex < drawList.Count)
                {
                    adjustedEvents.Add((newIndex, evt.eventId));
                }
            }
        }

        // sizeDeltaではなくrect.width/heightを使う (Anchor設定に依存せず正しいサイズを取得するため)
        float graphHeight = graphContainer.rect.height;
        float graphWidth = graphContainer.rect.width;

        // Y軸の範囲設定
        // MyMin/Maxの計算（0以下は除外するが、トリミング後に残った60は有効な値とする）
        var validBpmList = drawList.Where(x => x > 0).ToList();
        
        int maxBpm;
        int minBpm;

        if (validBpmList.Count > 0)
        {
            maxBpm = validBpmList.Max();
            minBpm = validBpmList.Min();
        }
        else
        {
            // 有効な値がない場合は元のリスト全体から算出
            maxBpm = drawList.Max();
            minBpm = drawList.Min();
        }
        
        // 変動を見やすくするためにパディングを小さくする (例: +/- 2)
        float padding = 2f;
        float yMax = maxBpm + padding;
        float yMin = Mathf.Max(0, minBpm - padding);
        
        float yDifference = yMax - yMin;
        
        // 差が小さすぎる場合（例：平坦なグラフ）は少し範囲を広げて見栄えを調整
        if (yDifference < 5f) 
        {
            float center = (yMax + yMin) / 2f;
            yMax = center + 2.5f;
            yMin = Mathf.Max(0, center - 2.5f);
            yDifference = yMax - yMin;
        }

        // X軸の間隔
        float xSize = graphWidth / (drawList.Count - 1);

        GameObject lastCircleGameObject = null;

        for (int i = 0; i < drawList.Count; i++)
        {
            float xPosition = i * xSize;
            float yPosition = ((drawList[i] - yMin) / yDifference) * graphHeight;
            Vector2 pos = new Vector2(xPosition, yPosition);

            // 通常のドット描画
            GameObject circleGameObject = CreateCircle(pos, false);
            
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                                    circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;

            // イベントマーカーのチェック
            if (adjustedEvents != null)
            {
                foreach (var evt in adjustedEvents)
                {
                    if (evt.index == i)
                    {
                        CreateEventMarker(pos, evt.eventId);
                    }
                }
            }
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition, bool isEvent)
    {
        GameObject gameObject = new GameObject(isEvent ? "event_marker" : "dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        
        Image image = gameObject.GetComponent<Image>();
        image.sprite = dotSprite;
        
        // 色とサイズを切り替え
        if (isEvent)
        {
            image.color = eventMarkerColor;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(eventMarkerSize, eventMarkerSize);
        }
        else
        {
            image.color = graphColor;
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(dotSize, dotSize);
        }

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        rectTransform.anchoredPosition = anchoredPosition;

        // イベントマーカーは前面に表示
        if (isEvent)
        {
            rectTransform.SetAsLastSibling();
        }
        
        return gameObject;
    }

    private void CreateEventMarker(Vector2 position, int eventId)
    {
        // マーカーを描画 (単純に色違いの大きなドットを表示)
        CreateCircle(position, true);

        // 必要であればここにテキスト表示などを追加可能
    }

    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("line", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        
        Image image = gameObject.GetComponent<Image>();
        image.color = new Color(graphColor.r, graphColor.g, graphColor.b, 0.5f);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f); // ピボット中心

        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        
        rectTransform.anchoredPosition = dotPositionA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
        rectTransform.localScale = Vector3.one; // スケールリセット
        
        rectTransform.SetAsFirstSibling();
    }

    private float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }
}
