using UnityEditor;
using UnityEngine;

public static class SetPrefabLayerTool
{
    private const string ItemLayerName = "Item";
    private static readonly string[] TargetFolders =
    {
        "Assets/1_MyAssets/Prefabs/Egg",
        "Assets/1_MyAssets/Prefabs/Monsters",
    };

    [MenuItem("Tools/RC69/Set Egg And Monster Prefabs Layer To Item")]
    public static void Run()
    {
        int itemLayer = LayerMask.NameToLayer(ItemLayerName);
        if (itemLayer < 0)
        {
            Debug.LogError($"Layer '{ItemLayerName}' does not exist.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", TargetFolders);
        int changedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);

            bool changed = SetLayerRecursively(root.transform, itemLayer);
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                changedCount++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SetPrefabLayerTool] Checked {guids.Length} prefabs, updated {changedCount} to layer '{ItemLayerName}'.");
    }

    private static bool SetLayerRecursively(Transform t, int layer)
    {
        bool changed = false;
        if (t.gameObject.layer != layer)
        {
            t.gameObject.layer = layer;
            changed = true;
        }

        foreach (Transform child in t)
        {
            if (SetLayerRecursively(child, layer))
            {
                changed = true;
            }
        }

        return changed;
    }
}
