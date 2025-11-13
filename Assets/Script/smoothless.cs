using UnityEditor;
using UnityEngine;

public class SetAllSmoothnessToZero
{
    [MenuItem("Tools/Set All Smoothness to 0")]
    static void SetSmoothnessToZero()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", 0f);
                EditorUtility.SetDirty(mat);
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Smoothnessを0に設定したマテリアル数: {count}");
    }
}