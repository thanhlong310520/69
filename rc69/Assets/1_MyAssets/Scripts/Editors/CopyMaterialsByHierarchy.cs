#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CopyMaterialsByHierarchy : EditorWindow
{
    GameObject rootA;
    GameObject rootB;
    private int count = 0;

    [MenuItem("Tools/Copy Materials (Ignore Root)")]
    static void Open()
    {
        GetWindow<CopyMaterialsByHierarchy>("Copy Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Copy Materials A → B (Ignore Root)", EditorStyles.boldLabel);

        rootA = (GameObject)EditorGUILayout.ObjectField("Root A", rootA, typeof(GameObject), true);
        rootB = (GameObject)EditorGUILayout.ObjectField("Root B", rootB, typeof(GameObject), true);

        if (GUILayout.Button("Copy Materials"))
        {
            if (!rootA || !rootB)
            {
                Debug.LogError("Assign both Root A and Root B");
                return;
            }

            Copy();
            Debug.Log("✅ Done copying materials " + count);
            count = 0;
        }
    }

    void Copy()
    {
        var renderersA = rootA.GetComponentsInChildren<Renderer>(true);

        foreach (var ra in renderersA)
        {
            Transform ta = ra.transform;

            string path = GetPathIgnoringRoot(ta, rootA.transform);
            if (string.IsNullOrEmpty(path))
                continue;

            Transform tb = rootB.transform.Find(path);
            if (tb == null)
                continue;

            var rb = tb.GetComponent<Renderer>();
            if (rb == null)
                continue;

            Undo.RecordObject(rb, "Copy Materials");
            rb.sharedMaterials = ra.sharedMaterials;
            count++;
        }
    }

    // 🔥 KEY METHOD
    string GetPathIgnoringRoot(Transform target, Transform root)
    {
        // stop at first child under root
        List<string> parts = new List<string>();

        while (target.parent != null && target.parent != root)
        {
            parts.Insert(0, target.name);
            target = target.parent;
        }

        // include first child under root
        if (target.parent == root)
            parts.Insert(0, target.name);

        return string.Join("/", parts);
    }
}
#endif