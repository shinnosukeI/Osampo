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

        if (bpmList == null || bpmList.Count < 2)
        {
            return;
        }

        // sizeDeltaではなくrect.width/heightを使う (Anchor設定に依存せず正しいサイズを取得するため)
        float graphHeight = graphContainer.rect.height;
        float graphWidth = graphContainer.rect.width;

        // Y軸の範囲設定
        int maxBpm = bpmList.Max();
        int minBpm = bpmList.Min();
        
        float yMax = maxBpm + 10f;
        float yMin = Mathf.Max(0, minBpm - 10f);
        float yDifference = yMax - yMin;
        if (yDifference <= 0) yDifference = 1f;

        // X軸の間隔
        float xSize = graphWidth / (bpmList.Count - 1);

        GameObject lastCircleGameObject = null;

        for (int i = 0; i < bpmList.Count; i++)
        {
            float xPosition = i * xSize;
            float yPosition = ((bpmList[i] - yMin) / yDifference) * graphHeight;
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
            if (events != null)
            {
                foreach (var evt in events)
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
