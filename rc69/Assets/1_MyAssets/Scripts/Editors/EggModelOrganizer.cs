#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Models already sorted into 1NewAsset/Egg/<Zone>/N.Name.fbx: match within the same zone,
        // ignoring the "N." index prefix. Prefabs whose HolderModel already has a child are skipped.
        [MenuItem("Raccoon/Egg/Attach Models From Zone Folders")]
        public static void AttachFromZoneFolders()
        {
            var prefabLookup = BuildPrefabLookup();
            int attached = 0, alreadyHasModel = 0;
            var noPrefab = new List<string>();
            var matched = new HashSet<string>();

            foreach (var zoneDir in AssetDatabase.GetSubFolders(ModelsRoot))
            {
                var zone = Zones.FirstOrDefault(z => Normalize(z) == Normalize(Path.GetFileName(zoneDir)));
                if (zone == null) continue;

                foreach (var fbxPath in Directory.GetFiles(zoneDir, "*.fbx", SearchOption.TopDirectoryOnly))
                {
                    var assetPath = fbxPath.Replace("\\", "/");
                    var fbxName = Path.GetFileNameWithoutExtension(assetPath);
                    var key = Normalize(Regex.Replace(fbxName, @"^\d+\.", ""));
                    if (Aliases.TryGetValue(key, out var aliasKey)) key = aliasKey;

                    if (!prefabLookup.TryGetValue(key, out var info) || info.zone != zone)
                    {
                        noPrefab.Add(assetPath);
                        continue;
                    }
                    matched.Add(info.prefabPath);

                    var fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (fbxAsset == null)
                    {
                        Debug.LogError($"[EggModelOrganizer] Could not load model asset at {assetPath}");
                        continue;
                    }

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(info.prefabPath);
                    var holder = prefab != null ? prefab.transform.Find("HolderModel") : null;
                    if (holder != null && holder.childCount > 0)
                    {
                        alreadyHasModel++;
                        continue;
                    }

                    AttachModelToPrefab(info.prefabPath, fbxAsset);
                    attached++;
                    Debug.Log($"[EggModelOrganizer] {assetPath} -> {info.prefabPath}");
                }
            }

            AssetDatabase.SaveAssets();

            foreach (var p in noPrefab)
                Debug.LogWarning($"[EggModelOrganizer] No matching egg prefab for {p}");
            foreach (var p in prefabLookup.Values.Select(v => v.prefabPath).Where(p => !matched.Contains(p)))
                Debug.LogWarning($"[EggModelOrganizer] No model found for {p}");
            Debug.Log($"[EggModelOrganizer] Attached: {attached}. Already had model: {alreadyHasModel}. " +
                      $"Models without prefab: {noPrefab.Count}.");
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
                SetLayerRecursive(instance.transform, root.layer);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void SetLayerRecursive(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t)
                SetLayerRecursive(child, layer);
        }

        private static string Normalize(string s)
        {
            return new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }
    }
}
#endif
