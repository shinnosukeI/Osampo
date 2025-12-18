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

    /// <summary>
    /// 心拍数リストを受け取り、グラフを描画する
    /// </summary>
    public void ShowGraph(List<int> bpmList)
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

            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, 
                                    circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;
        }
    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        
        Image image = gameObject.GetComponent<Image>();
        image.sprite = dotSprite;
        image.color = graphColor;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        // アンカーを左下に設定
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        // ピボットを中心にする
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(dotSize, dotSize);
        
        // スケールと回転をリセット (念のため)
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        
        return gameObject;
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
