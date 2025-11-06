using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HorrorEventCategory
{
    public string categoryName; // Inspectorでの見出し用 (例: "1. 異形の恐怖")
    public int categoryId;      // 1
    
    // このカテゴリに属するイベント（ScriptableObject）のリスト
    public List<HorrorEventData> events;
}

// データベース本体
[CreateAssetMenu(fileName = "HorrorEventDatabase", menuName = "Horror/Event Database")]
public class HorrorEventDatabase : ScriptableObject
{
    // カテゴリのリスト
    public List<HorrorEventCategory> categories;
}