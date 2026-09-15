#if UNITY_EDITOR
using System.Collections.Generic;
using Raccoon.Data;
using UnityEditor;
using UnityEngine;

namespace Raccoon.EditorTool
{
    // Fixes ItemDataSO.prefab references that go stale whenever their target .prefab is
    // re-saved (PrefabUtility.SaveAsPrefabAsset regenerates internal local file IDs, so any
    // asset still holding the old fileID shows "Missing" even though the guid is fine).
    // Covers both Egg data (EggDataSO) and Monster data (BotDataSO), since both derive
    // from ItemDataSO and both live under a Datas/<X>/<Zone>/ <-> Prefabs/<X>/<Zone>/ pair.
    public static class RelinkEggPrefabs
    {
        private static readonly (string dataRoot, string prefabRoot)[] Roots =
        {
            ("Assets/1_MyAssets/Datas/Egg", "Assets/1_MyAssets/Prefabs/Egg"),
            ("Assets/1_MyAssets/Datas/Monsters", "Assets/1_MyAssets/Prefabs/Monsters"),
        };

        [MenuItem("Raccoon/Egg/Relink Egg Data Prefabs")]
        public static void Run()
        {
            int fixedCount = 0;
            int alreadyOk = 0;
            var missing = new List<string>();

            foreach (var (dataRoot, prefabRoot) in Roots)
            {
                var guids = AssetDatabase.FindAssets("t:ItemDataSO", new[] { dataRoot });
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var prefabPath = assetPath
                        .Replace(dataRoot, prefabRoot)
                        .Replace(".asset", ".prefab");

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        missing.Add($"{assetPath} -> expected {prefabPath}");
                        continue;
                    }

                    var so = new SerializedObject(AssetDatabase.LoadAssetAtPath<ItemDataSO>(assetPath));
                    var prefabProp = so.FindProperty("prefab");

                    if (prefabProp.objectReferenceValue == prefab)
                    {
                        alreadyOk++;
                        continue;
                    }

                    prefabProp.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(so.targetObject);
                    fixedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[RelinkEggPrefabs] Fixed: {fixedCount}. Already OK: {alreadyOk}. Missing prefab file: {missing.Count}" +
                (missing.Count > 0 ? $"\n{string.Join("\n", missing)}" : ""));
        }
    }
}
#endif
