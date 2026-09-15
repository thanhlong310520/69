using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MonsterMaterialExtractor
{
    private const string MonsterModelsRoot = "Assets/1_MyAssets/Models/1NewAsset/Monster";

    [MenuItem("Tools/Raccoon/Extract Monster Materials")]
    public static void ExtractAll()
    {
        int extracted = 0, skipped = 0;

        var fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { MonsterModelsRoot });
        foreach (var guid in fbxGuids)
        {
            var fbxPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!(AssetImporter.GetAtPath(fbxPath) is ModelImporter importer))
                continue;

            var embeddedMaterials = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<Material>()
                .ToArray();
            if (embeddedMaterials.Length == 0)
            {
                skipped++;
                continue;
            }

            var modelFolder = Path.GetDirectoryName(fbxPath)?.Replace("\\", "/");
            var modelName = GetMonsterName(modelFolder);
            var materialsFolder = $"{modelFolder}/Materials";
            EnsureFolder(materialsFolder);

            var changed = false;
            foreach (var mat in embeddedMaterials)
            {
                var matFileName = embeddedMaterials.Length == 1 ? modelName : $"{modelName}_{mat.name}";
                var matPath = $"{materialsFolder}/{matFileName}.mat";

                var ownMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (ownMaterial == null)
                {
                    ownMaterial = new Material(mat);
                    AssetDatabase.CreateAsset(ownMaterial, matPath);
                }

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(mat), ownMaterial);
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
                extracted++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Extract Monster Materials hoan tat: {extracted} model da co material rieng, {skipped} model khong co material embedded.");
    }

    private static string GetMonsterName(string modelFolder)
    {
        var folderName = new DirectoryInfo(modelFolder).Name;
        var dotIndex = folderName.IndexOf('.');
        if (dotIndex >= 0) folderName = folderName.Substring(dotIndex + 1);

        var parts = folderName.Split(new[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
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
}
