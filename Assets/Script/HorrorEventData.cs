using UnityEngine;

// 単一イベントデータ（イベントタイプ、名前、カテゴリなど）
[CreateAssetMenu(fileName = "HorrorEventData", menuName = "HorrorGame/Horror Event Data")]
public class HorrorEventData : ScriptableObject
{
    [Header("イベント基本情報")]
    public int eventType;   // イベントID (例: 54)
    public string eventName; // イベント名 (例: 物が落ちる)
    public int category;    // 分類 (例: 5)
}
