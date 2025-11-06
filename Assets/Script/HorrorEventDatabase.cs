using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HorrorEventDatabase", menuName = "HorrorGame/Horror Event Database")]
public class HorrorEventDatabase : ScriptableObject
{
    [SerializeField]
    private List<HorrorEventData> horrorEvents = new List<HorrorEventData>(); // ← null防止の初期化

    private Dictionary<int, HorrorEventData> eventDictionary = new Dictionary<int, HorrorEventData>();

    public void Initialize()
    {
        eventDictionary.Clear();

        foreach (var evt in horrorEvents)
        {
            if (evt == null) continue; // ← これで「None」要素を無視
            if (!eventDictionary.ContainsKey(evt.eventType))
                eventDictionary.Add(evt.eventType, evt);
        }
    }

    public HorrorEventData GetEventData(int eventType)
    {
        eventDictionary.TryGetValue(eventType, out HorrorEventData data);
        return data;
    }

    public List<HorrorEventData> GetEventsByCategory(int category)
    {
        List<HorrorEventData> result = new List<HorrorEventData>();
        foreach (var evt in horrorEvents)
        {
            if (evt != null && evt.category == category)
                result.Add(evt);
        }
        return result;
    }
}
