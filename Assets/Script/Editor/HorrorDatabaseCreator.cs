using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
public class HorrorDatabaseCreator
{
    [MenuItem("HorrorGame/Generate Database")]
    public static void GenerateDatabase()
    {
        // アセット保存パス
        string folderPath = "Assets/Resources";
        string assetPath = Path.Combine(folderPath, "HorrorEventDatabase.asset");

        // フォルダ確認
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // データベース作成またはロード
        HorrorEventDatabase database = AssetDatabase.LoadAssetAtPath<HorrorEventDatabase>(assetPath);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<HorrorEventDatabase>();
            AssetDatabase.CreateAsset(database, assetPath);
        }

        // リストをクリアするために、既存のサブアセットを一掃する処理を入れてもいいが、
        // 今回はシンプルに追加・更新を行う
        // 既存データをクリアする場合は:
        // var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        // foreach(var sub in subAssets) if (sub is HorrorEventData) Object.DestroyImmediate(sub, true);

        // Access private list via Reflection if necessary, but since we are Editor, 
        // we can probably assume field is serialized. 
        // HorrorEventDatabase has 'private List<HorrorEventData> horrorEvents'.
        // We need to modify this list. Since it is private, we use SerializedObject.
        
        SerializedObject serializedDB = new SerializedObject(database);
        SerializedProperty listProp = serializedDB.FindProperty("horrorEvents");
        listProp.ClearArray();

        // 定義データ
        var eventDefinitions = new List<(int id, string name)>
        {
            (11, "ゴキブリ"),
            (12, "死体腐敗"),
            (13, "蜘蛛"),
            (14, "死体落下"),
            (15, "ゾンビ追尾"),
            (21, "テディベア"),
            (22, "マネキン"),
            (23, "髪の毛・頭蓋骨"),
            (24, "窓に手形"),
            (25, "壁に目玉"),
            (31, "滴る血液"),
            (32, "血痕"),
            (33, "血とガラス片"),
            (34, "肉の裂傷音"),
            (35, "集合体"),
            (41, "背後の足音"),
            (42, "笑い声"),
            (43, "鏡背後に人"),
            (44, "背後に人体"),
            (45, "ラジオ"),
            (46, "雷"),
            (51, "人影"),
            (52, "ドアの女"),
            (53, "消える女"),
            (54, "時計落下"),
            (55, "ガラス破壊"),
            (56, "ボール")
        };

        foreach (var def in eventDefinitions)
        {
            // カテゴリはIDの10の位と仮定
            int category = def.id / 10;

            // サブアセット作成
            HorrorEventData newData = ScriptableObject.CreateInstance<HorrorEventData>();
            newData.name = $"Event_{def.id}_{def.name}";
            newData.eventType = def.id;
            newData.eventName = def.name;
            newData.category = category;

            // データベースアセットに追加
            AssetDatabase.AddObjectToAsset(newData, database);

            // リストに追加
            int index = listProp.arraySize;
            listProp.InsertArrayElementAtIndex(index);
            listProp.GetArrayElementAtIndex(index).objectReferenceValue = newData;
        }

        serializedDB.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"HorrorEventDatabase generated at {assetPath} with {eventDefinitions.Count} events.");
    }
}
#endif
