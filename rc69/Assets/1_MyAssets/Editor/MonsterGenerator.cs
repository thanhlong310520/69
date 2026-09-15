using System;
using System.IO;
using System.Text.RegularExpressions;
using Raccoon.Data;
using UnityEditor;
using UnityEngine;

public static class MonsterGenerator
{
    private const string BotDefaultPath = "Assets/1_MyAssets/Prefabs/Bots/BotDefault.prefab";
    private const string EggRootFolder = "Assets/1_MyAssets/Datas/Egg";
    private const string MonsterPrefabFolder = "Assets/1_MyAssets/Prefabs/Monsters";
    private const string MonsterDataFolder = "Assets/1_MyAssets/Datas/Monsters";

    [MenuItem("Tools/Raccoon/Generate Monster Prefabs From Eggs")]
    public static void Generate()
    {
        var botDefault = AssetDatabase.LoadAssetAtPath<GameObject>(BotDefaultPath);
        if (botDefault == null)
        {
            Debug.LogError($"Khong tim thay BotDefault tai {BotDefaultPath}");
            return;
        }

        EnsureFolder(MonsterPrefabFolder);
        EnsureFolder(MonsterDataFolder);

        int createdPrefab = 0, createdData = 0, movedPrefab = 0, movedData = 0, linked = 0;

        var eggGuids = AssetDatabase.FindAssets("t:EggDataSO", new[] { EggRootFolder });
        foreach (var guid in eggGuids)
        {
            var eggPath = AssetDatabase.GUIDToAssetPath(guid);
            var egg = AssetDatabase.LoadAssetAtPath<EggDataSO>(eggPath);
            if (egg == null) continue;

            var eggFileName = Path.GetFileNameWithoutExtension(eggPath);
            if (!eggFileName.StartsWith("Egg", StringComparison.Ordinal))
            {
                Debug.LogWarning($"Bo qua {eggPath}: ten khong bat dau bang 'Egg'");
                continue;
            }

            var baseName = eggFileName.Substring(3);
            var monsterName = "Monster" + baseName;
            var zoneFolder = new DirectoryInfo(Path.GetDirectoryName(eggPath)).Name;

            var prefabFolder = $"{MonsterPrefabFolder}/{zoneFolder}";
            var dataFolder = $"{MonsterDataFolder}/{zoneFolder}";
            EnsureFolder(prefabFolder);
            EnsureFolder(dataFolder);

            var prefabPath = $"{prefabFolder}/{monsterName}.prefab";
            var soPath = $"{dataFolder}/{monsterName}.asset";
            var oldPrefabPath = $"{MonsterPrefabFolder}/{monsterName}.prefab";
            var oldSoPath = $"{MonsterDataFolder}/{monsterName}.asset";

            var monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (monsterPrefab == null && oldPrefabPath != prefabPath &&
                AssetDatabase.LoadAssetAtPath<GameObject>(oldPrefabPath) != null)
            {
                var moveError = AssetDatabase.MoveAsset(oldPrefabPath, prefabPath);
                if (!string.IsNullOrEmpty(moveError))
                    Debug.LogError($"Khong the di chuyen {oldPrefabPath} -> {prefabPath}: {moveError}");
                else
                    movedPrefab++;
                monsterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
            if (monsterPrefab == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(botDefault);
                instance.name = monsterName;
                monsterPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out var success);
                UnityEngine.Object.DestroyImmediate(instance);
                if (!success)
                {
                    Debug.LogError($"Khong the tao prefab variant cho {monsterName}");
                    continue;
                }
                createdPrefab++;
            }

            var monsterData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
            if (monsterData == null && oldSoPath != soPath &&
                AssetDatabase.LoadAssetAtPath<BotDataSO>(oldSoPath) != null)
            {
                var moveError = AssetDatabase.MoveAsset(oldSoPath, soPath);
                if (!string.IsNullOrEmpty(moveError))
                    Debug.LogError($"Khong the di chuyen {oldSoPath} -> {soPath}: {moveError}");
                else
                    movedData++;
                monsterData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
            }
            if (monsterData == null)
            {
                monsterData = ScriptableObject.CreateInstance<BotDataSO>();
                monsterData.itemName = SplitPascalCase(baseName);
                monsterData.itemType = ItemType.Brainrot;
                monsterData.rarityType = RarityType.Common;
                if (Enum.TryParse(zoneFolder, out ZoneType zone))
                    monsterData.colorType = zone;
                monsterData.prefab = monsterPrefab;
                AssetDatabase.CreateAsset(monsterData, soPath);
                createdData++;
            }
            else if (monsterData.prefab != monsterPrefab)
            {
                monsterData.prefab = monsterPrefab;
                EditorUtility.SetDirty(monsterData);
            }

            if (egg.monsterSO != (ItemDataSO)monsterData)
            {
                egg.monsterSO = monsterData;
                EditorUtility.SetDirty(egg);
                linked++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Hoan tat: prefab moi {createdPrefab}, BotDataSO moi {createdData}, " +
                  $"prefab da chuyen {movedPrefab}, BotDataSO da chuyen {movedData}, lien ket egg->monster {linked}.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static string SplitPascalCase(string s)
    {
        return Regex.Replace(s, "(?<!^)([A-Z])", " $1");
    }
}
