using UnityEngine;

[CreateAssetMenu(fileName = "NewHorrorEvent", menuName = "Horror/New Horror Event")]
public class HorrorEventData : ScriptableObject
{
    [Header("イベント識別情報")]
    public int categoryId; // 1:異形, 2:人体, 3:生理的...
    
    public int eventType;  // 恐怖要素に対応する番号 (11, 12, 21...)
    
    [TextArea(3, 5)]
    public string description; // "ドアの下の隙間から大量のゴキブリ"

    [Header("実行するアセット")]
    public GameObject eventPrefab;
    public AudioClip eventSound;

    // (他にも必要なデータがあればここに追加します)
}
