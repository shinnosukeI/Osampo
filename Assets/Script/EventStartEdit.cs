using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventForceStarter))]
public class EventForceStarterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EventForceStarter starter = (EventForceStarter)target;

        if (GUILayout.Button("▶ 強制スタート実行"))
        {
            starter.ForceStart();
        }
    }
}