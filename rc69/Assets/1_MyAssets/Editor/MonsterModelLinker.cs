using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class MonsterModelLinker
{
    private const string ModelRootFolder = "Assets/1_MyAssets/Models/1NewAsset/Monster";
    private const string MonsterPrefabFolder = "Assets/1_MyAssets/Prefabs/Monsters";

    [MenuItem("Tools/Raccoon/Link Monster Models (bo qua prefab da co model)")]
    public static void LinkMissing() => Link(false);

    [MenuItem("Tools/Raccoon/Link Monster Models (ghi de tat ca)")]
    public static void LinkAll()
    {
        if (!EditorUtility.DisplayDialog("Link Monster Models",
                "Xoa model cu trong 'Model' cua tat ca prefab Monster va gan lai?", "OK", "Huy"))
            return;
        Link(true);
    }

    private static void Link(bool overwrite)
    {
        var models = CollectModels();
        int linked = 0, skipped = 0;
        var missing = new List<string>();

        foreach (var zoneFolder in AssetDatabase.GetSubFolders(MonsterPrefabFolder))
        {
            var zoneKey = Normalize(Path.GetFileName(zoneFolder));
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { zoneFolder }))
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var prefabName = Path.GetFileNameWithoutExtension(prefabPath);
                if (!prefabName.StartsWith("Monster")) continue;

                var key = zoneKey + "/" + Normalize(prefabName.Substring("Monster".Length));
                if (!models.TryGetValue(key, out var entry))
                {
                    missing.Add(prefabPath);
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                try
                {
                    var modelParent = root.transform.Find("Model");
                    if (modelParent == null)
                    {
                        Debug.LogWarning($"{prefabPath} khong co child 'Model'");
                        continue;
                    }
                    if (modelParent.childCount > 0 && !overwrite)
                    {
                        skipped++;
                        continue;
                    }

                    for (int i = modelParent.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(modelParent.GetChild(i).gameObject);

                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(entry.model, modelParent);
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    SetLayerRecursive(instance.transform, root.layer);

                    if (entry.controller != null)
                    {
                        var anim = instance.GetComponent<Animator>();
                        if (anim == null) anim = instance.AddComponent<Animator>();
                        anim.runtimeAnimatorController = entry.controller;
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    linked++;
                    Debug.Log($"Gan {AssetDatabase.GetAssetPath(entry.model)} -> {prefabPath}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        AssetDatabase.SaveAssets();
        foreach (var p in missing)
            Debug.LogWarning($"Khong tim thay model cho {p}");
        Debug.Log($"Link Monster Models: da gan {linked}, bo qua (da co model) {skipped}, thieu model {missing.Count}.");
    }

    private struct ModelEntry
    {
        public GameObject model;
        public RuntimeAnimatorController controller;
    }

    // Key: "<zone>/<ten>" da chuan hoa, vd "prehistoric/trex"
    private static Dictionary<string, ModelEntry> CollectModels()
    {
        var result = new Dictionary<string, ModelEntry>();
        foreach (var zoneFolder in AssetDatabase.GetSubFolders(ModelRootFolder))
        {
            var zoneKey = Normalize(Path.GetFileName(zoneFolder));

            // Model tinh: <zone>/N.Ten.fbx
            foreach (var file in Directory.GetFiles(zoneFolder, "*.fbx", SearchOption.TopDirectoryOnly))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(file.Replace("\\", "/"));
                if (model != null)
                    result[zoneKey + "/" + Normalize(Path.GetFileNameWithoutExtension(file))] = new ModelEntry { model = model };
            }

            // Model co animation: <zone>/N.Ten/*Character*.fbx + overrideController (uu tien hon model tinh)
            foreach (var folder in AssetDatabase.GetSubFolders(zoneFolder))
            {
                var character = Directory.GetFiles(folder, "*.fbx", SearchOption.TopDirectoryOnly)
                    .Where(f => Path.GetFileName(f).ToLower().Contains("character"))
                    .OrderBy(f => char.IsDigit(Path.GetFileName(f)[0]) ? 1 : 0) // uu tien ban khong co so thu tu
                    .FirstOrDefault();
                if (character == null) continue;

                var model = AssetDatabase.LoadAssetAtPath<GameObject>(character.Replace("\\", "/"));
                var overridePath = Directory.GetFiles(folder, "*.overrideController", SearchOption.TopDirectoryOnly).FirstOrDefault();
                var controller = overridePath != null
                    ? AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(overridePath.Replace("\\", "/"))
                    : null;
                if (model != null)
                    result[zoneKey + "/" + Normalize(Path.GetFileName(folder))] = new ModelEntry { model = model, controller = controller };
            }
        }
        return result;
    }

    // "54.Beluga Whale" -> "belugawhale", "T-Rex" -> "trex"
    private static string Normalize(string s)
    {
        s = Regex.Replace(s, @"^\d+\.", "");
        return Regex.Replace(s, "[^A-Za-z0-9]", "").ToLowerInvariant();
    }

    private static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayerRecursive(child, layer);
    }
}
