#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Raccoon.EditorTool
{
    public static class EggModelOrganizer
    {
        private const string ModelsRoot = "Assets/1_MyAssets/Models/1NewAsset/Egg";
        private const string PrefabsRoot = "Assets/1_MyAssets/Prefabs/Egg";
        private const string BaseEggPrefabPath = "Assets/1_MyAssets/Prefabs/Egg/_EggDefault.prefab";

        private static readonly string[] Zones =
        {
            "Forest", "Lake", "Desert", "Jungle", "Snow", "Volcano", "AbyssOcean", "Prehistoric"
        };

        // Model file name (normalized) -> prefab key (normalized), for names that no longer match after renames.
        private static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "royalsphinx", "kiwissimosphinx" },
            { "camel", "frigocamel" },
        };

        [MenuItem("Raccoon/Egg/Organize And Attach Models")]
        public static void Run()
        {
            RenameModelHolder();

            var prefabLookup = BuildPrefabLookup();

            var fbxFiles = Directory.GetFiles(ModelsRoot, "*.fbx", SearchOption.TopDirectoryOnly);
            int attached = 0;
            var skipped = new List<string>();

            foreach (var fbxPath in fbxFiles)
            {
                var assetPath = fbxPath.Replace("\\", "/");
                var fbxName = Path.GetFileNameWithoutExtension(assetPath);
                var key = Normalize(fbxName);
                if (Aliases.TryGetValue(key, out var aliasKey)) key = aliasKey;

                if (!prefabLookup.TryGetValue(key, out var info))
                {
                    skipped.Add(fbxName);
                    continue;
                }

                var newPath = MoveModelIntoZoneFolder(assetPath, info.zone);
                var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(newPath);
                if (fbxAsset == null)
                {
                    Debug.LogError($"[EggModelOrganizer] Could not load model asset at {newPath}");
                    continue;
                }

                AttachModelToPrefab(info.prefabPath, fbxAsset);
                attached++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EggModelOrganizer] Done. Attached: {attached}. Skipped (no matching prefab): {skipped.Count} [{string.Join(", ", skipped)}]");
        }

        private static Dictionary<string, (string zone, string prefabPath)> BuildPrefabLookup()
        {
            var lookup = new Dictionary<string, (string zone, string prefabPath)>();
            foreach (var zone in Zones)
            {
                var dir = $"{PrefabsRoot}/{zone}";
                if (!Directory.Exists(dir)) continue;

                foreach (var prefabPath in Directory.GetFiles(dir, "*.prefab"))
                {
                    var name = Path.GetFileNameWithoutExtension(prefabPath);
                    var trimmed = name.StartsWith("Egg") ? name.Substring(3) : name;
                    var key = Normalize(trimmed);
                    lookup[key] = (zone, prefabPath.Replace("\\", "/"));
                }
            }
            return lookup;
        }

        private static string MoveModelIntoZoneFolder(string assetPath, string zone)
        {
            var zoneDir = $"{ModelsRoot}/{zone}";
            if (!AssetDatabase.IsValidFolder(zoneDir))
            {
                AssetDatabase.CreateFolder(ModelsRoot, zone);
            }

            var newPath = $"{zoneDir}/{Path.GetFileName(assetPath)}";
            if (assetPath == newPath) return assetPath;

            var error = AssetDatabase.MoveAsset(assetPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[EggModelOrganizer] Failed moving {assetPath} -> {newPath}: {error}");
                return assetPath;
            }
            return newPath;
        }

        private static void RenameModelHolder()
        {
            var root = PrefabUtility.LoadPrefabContents(BaseEggPrefabPath);
            try
            {
                var modelT = root.transform.Find("Model");
                if (modelT != null)
                {
                    modelT.gameObject.name = "HolderModel";
                    PrefabUtility.SaveAsPrefabAsset(root, BaseEggPrefabPath);
                    Debug.Log("[EggModelOrganizer] Renamed 'Model' -> 'HolderModel' in _EggDefault.prefab");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void AttachModelToPrefab(string prefabPath, GameObject fbxAsset)
        {
            var root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var holder = root.transform.Find("HolderModel");
                if (holder == null)
                {
                    Debug.LogError($"[EggModelOrganizer] '{prefabPath}' has no HolderModel child, skipping.");
                    return;
                }

                for (int i = holder.childCount - 1; i >= 0; i--)
                {
                    Object.DestroyImmediate(holder.GetChild(i).gameObject);
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, holder);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static string Normalize(string s)
        {
            return new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
#endif
