using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Thay map FBX cũ (Map_RC69_Final) bằng map mới (Map RC 69 ver 3) trong scene đang mở.
/// - Remap material của FBX mới về MatMap giống FBX cũ.
/// - Giữ layer, active, material override, component gắn thêm (MeshCollider...) theo đường dẫn node.
/// - Chuyển NGUYÊN các object gắn thêm vào map (SaveZone, Slot, StartPos, Base_San (1)...) sang map mới, không tạo mới.
/// - Nối lại mọi tham chiếu từ scene vào bên trong map (ChangeEnvironment...).
/// - Đổi mesh của MeshFilter/MeshCollider đang trỏ vào FBX cũ sang FBX mới.
/// - Vùng mới (Vung_9*): layer Ground + MeshCollider + thêm slot vào ChangeEnvironment (material tạm = material hiện tại).
/// Tất cả nằm trong 1 bước Undo. Scene KHÔNG tự lưu, kiểm tra xong hãy Ctrl+S.
/// </summary>
public static class MapReplacer
{
    const string OldMapPath = "Assets/1_MyAssets/Models/1NewAsset/Map_RC69_Final.fbx";
    const string NewMapPath = "Assets/1_MyAssets/Models/1NewAsset/Map RC 69 ver 3.fbx";
    const string NewZonePrefix = "Vung_9";
    const int GroundLayer = 6;

    static readonly HashSet<string> SkipProps = new HashSet<string>
    {
        "m_GameObject", "m_PrefabInstance", "m_PrefabAsset", "m_CorrespondingSourceObject", "m_Father", "m_Children"
    };

    class NodeInfo
    {
        public int layer;
        public bool active;
        public string tag;
        public Material[] materialOverride;
    }

    class AddedObj
    {
        public Transform t;
        public string parentPath;
        public Vector3 pos; public Quaternion rot; public Vector3 scale;
    }

    class RefInfo
    {
        public Component owner;
        public string propertyPath;
        public string targetPath;
        public Type targetType;
        public bool isGameObject;
        public int componentIndex;
    }

    // ------------------------------------------------------------------ MENU

    [MenuItem("Tools/Map/Replace Map_RC69_Final -> Map RC 69 ver 3")]
    static void ReplaceMenu()
    {
        if (!EditorUtility.DisplayDialog("Replace Map",
                "Thay Map_RC69_Final bằng Map RC 69 ver 3 trong scene đang mở?\n\nCó thể Undo (Ctrl+Z). Scene sẽ không tự lưu.",
                "Thay", "Huỷ"))
            return;
        try { Replace(); }
        catch (Exception e) { Debug.LogException(e); EditorUtility.DisplayDialog("Replace Map", "Lỗi: " + e.Message + "\nXem Console.", "OK"); }
    }

    [MenuItem("Tools/Map/Validate Map")]
    static void ValidateMenu() => Validate(true);

    // ------------------------------------------------------------------ REPLACE

    static void Replace()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(NewMapPath) == null) throw new Exception("Không tìm thấy " + NewMapPath);

        var oldRoots = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(t => PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject)
                        && PrefabUtility.GetPrefabAssetPathOfNearestPrefabInstanceRoot(t.gameObject) == OldMapPath)
            .ToList();
        if (oldRoots.Count != 1) throw new Exception($"Cần đúng 1 instance của {OldMapPath} trong scene, đang có {oldRoots.Count}.");
        var oldRoot = oldRoots[0];
        var scene = oldRoot.gameObject.scene;
        var log = new StringBuilder("[MapReplacer]\n");

        // 1. Remap material FBX mới về MatMap (theo bảng remap của FBX cũ)
        RemapMaterials(log);
        var newAsset = AssetDatabase.LoadAssetAtPath<GameObject>(NewMapPath); // load sau khi reimport

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Replace Map");
        int undoGroup = Undo.GetCurrentGroup();

        // 2. Ghi lại thông tin map cũ
        var oldPathOf = new Dictionary<Transform, string>();
        var added = new List<AddedObj>();
        Collect(oldRoot, "", OldMapPath, oldPathOf, added);

        var nodeInfos = new Dictionary<string, NodeInfo>();
        foreach (var kv in oldPathOf)
        {
            var go = kv.Key.gameObject;
            var info = new NodeInfo { layer = go.layer, active = go.activeSelf, tag = go.tag };
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                var src = PrefabUtility.GetCorrespondingObjectFromSource(r);
                if (src == null || !src.sharedMaterials.SequenceEqual(r.sharedMaterials))
                    info.materialOverride = r.sharedMaterials;
            }
            nodeInfos[kv.Value] = info;
        }

        var addedComps = PrefabUtility.GetAddedComponents(oldRoot.gameObject)
            .Select(a => a.instanceComponent)
            .Where(c => c != null && oldPathOf.ContainsKey(c.transform))
            .ToList();

        var refs = CollectRefs(scene, oldPathOf);
        log.AppendLine($"Map cũ: {oldPathOf.Count} node, {added.Count} object gắn thêm, {addedComps.Count} component gắn thêm, {refs.Count} tham chiếu từ scene.");

        // 3. Tạo map mới cùng vị trí
        var newGo = (GameObject)PrefabUtility.InstantiatePrefab(newAsset, scene);
        Undo.RegisterCreatedObjectUndo(newGo, "Replace Map");
        var newRoot = newGo.transform;
        newRoot.SetParent(oldRoot.parent, false);
        newRoot.SetSiblingIndex(oldRoot.GetSiblingIndex() + 1);
        newRoot.localPosition = oldRoot.localPosition;
        newRoot.localRotation = oldRoot.localRotation;
        newRoot.localScale = oldRoot.localScale;

        var newByPath = new Dictionary<string, Transform>();
        var dummy = new List<AddedObj>();
        var newPathOf = new Dictionary<Transform, string>();
        Collect(newRoot, "", NewMapPath, newPathOf, dummy);
        foreach (var kv in newPathOf) newByPath[kv.Value] = kv.Key;

        // 4. Áp lại layer / active / tag / material override
        int applied = 0;
        foreach (var kv in nodeInfos)
        {
            if (!newByPath.TryGetValue(kv.Key, out var nt)) { if (kv.Key != "") log.AppendLine("  ! Map mới không có node: " + kv.Key); continue; }
            var go = nt.gameObject;
            Undo.RecordObject(go, "Replace Map");
            go.layer = kv.Value.layer;
            go.SetActive(kv.Value.active);
            try { go.tag = kv.Value.tag; } catch { }
            if (kv.Value.materialOverride != null)
            {
                var r = go.GetComponent<Renderer>();
                if (r != null && SubMeshCount(r) == kv.Value.materialOverride.Length)
                {
                    Undo.RecordObject(r, "Replace Map");
                    r.sharedMaterials = kv.Value.materialOverride;
                }
                else log.AppendLine($"  ! Bỏ qua material override ở {kv.Key} (số submesh khác)");
            }
            applied++;
        }
        log.AppendLine($"Áp thuộc tính cho {applied} node.");

        // 5. Copy component gắn thêm (MeshCollider...)
        foreach (var c in addedComps)
        {
            var path = oldPathOf[c.transform];
            if (!newByPath.TryGetValue(path, out var nt)) { log.AppendLine($"  ! Không copy được {c.GetType().Name} ở {path}"); continue; }
            var nc = nt.GetComponent(c.GetType());
            if (nc == null) nc = Undo.AddComponent(nt.gameObject, c.GetType());
            else Undo.RecordObject(nc, "Replace Map");
            EditorUtility.CopySerialized(c, nc);
            if (nc is MeshCollider mc)
            {
                var mf = nt.GetComponent<MeshFilter>();
                if (mf != null) mc.sharedMesh = mf.sharedMesh;
            }
        }
        log.AppendLine($"Copy {addedComps.Count} component gắn thêm.");

        // 6. Chuyển nguyên object gắn thêm sang map mới
        foreach (var a in added)
        {
            if (!newByPath.TryGetValue(a.parentPath, out var np))
            {
                log.AppendLine($"  ! Không thấy node cha '{a.parentPath}' cho {a.t.name}, gắn vào root map mới.");
                np = newRoot;
            }
            Undo.SetTransformParent(a.t, np, "Replace Map");
            Undo.RecordObject(a.t, "Replace Map");
            a.t.localPosition = a.pos; a.t.localRotation = a.rot; a.t.localScale = a.scale;
            log.AppendLine($"  -> {a.t.name}  =>  {(a.parentPath == "" ? "<root>" : a.parentPath)}");
        }

        // 7. Nối lại tham chiếu
        int rebound = 0;
        foreach (var grp in refs.GroupBy(r => r.owner))
        {
            if (grp.Key == null) continue;
            var so = new SerializedObject(grp.Key);
            foreach (var r in grp)
            {
                var prop = so.FindProperty(r.propertyPath);
                if (prop == null) continue;
                UnityEngine.Object target = null;
                if (newByPath.TryGetValue(r.targetPath, out var nt))
                {
                    if (r.isGameObject) target = nt.gameObject;
                    else
                    {
                        var comps = nt.GetComponents(r.targetType);
                        if (comps.Length > r.componentIndex) target = comps[r.componentIndex];
                    }
                }
                if (target == null) { log.AppendLine($"  ! Không nối được {grp.Key.name}.{r.propertyPath} -> {r.targetPath}"); continue; }
                prop.objectReferenceValue = target;
                rebound++;
            }
            so.ApplyModifiedProperties();
        }
        log.AppendLine($"Nối lại {rebound}/{refs.Count} tham chiếu.");

        // 8. Vùng mới Vung_9*: layer Ground + MeshCollider + slot ChangeEnvironment
        var newZoneRenderers = new List<Renderer>();
        foreach (var kv in newPathOf)
        {
            if (oldPathOf.ContainsValue(kv.Value)) continue;
            if (!kv.Key.name.StartsWith(NewZonePrefix)) continue;
            var mf = kv.Key.GetComponent<MeshFilter>();
            if (mf == null) continue;
            Undo.RecordObject(kv.Key.gameObject, "Replace Map");
            kv.Key.gameObject.layer = GroundLayer;
            var mc = kv.Key.GetComponent<MeshCollider>();
            if (mc == null) mc = Undo.AddComponent<MeshCollider>(kv.Key.gameObject);
            else Undo.RecordObject(mc, "Replace Map");
            mc.sharedMesh = mf.sharedMesh;
            mc.convex = false;
            var r = kv.Key.GetComponent<Renderer>();
            if (r != null) newZoneRenderers.Add(r);
        }
        log.AppendLine($"Vùng mới: {newZoneRenderers.Count} renderer {NewZonePrefix}* -> layer Ground + MeshCollider.");
        AddZoneSlots(newZoneRenderers, log);

        // 9. Xoá map cũ
        Undo.DestroyObjectImmediate(oldRoot.gameObject);

        // 10. Mesh trong scene còn trỏ vào FBX cũ -> FBX mới (Base_San (1)...)
        RemapSceneMeshes(scene, log);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = newGo;

        log.AppendLine("XONG. Việc còn lại: Bake lại NavMesh (CONTROLER/NavmeshSurface), kiểm tra rồi Ctrl+S.");
        Debug.Log(log.ToString());
        Validate(false);
        EditorUtility.DisplayDialog("Replace Map", "Đã thay map. Xem Console để biết chi tiết.\nNhớ Bake NavMesh và Ctrl+S.", "OK");
    }

    // ------------------------------------------------------------------ HELPERS

    static bool IsFromAsset(Transform t, string assetPath)
    {
        var src = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
        return src != null && AssetDatabase.GetAssetPath(src) == assetPath;
    }

    static void Collect(Transform t, string path, string assetPath, Dictionary<Transform, string> pathOf, List<AddedObj> added)
    {
        pathOf[t] = path;
        var seen = new Dictionary<string, int>();
        for (int i = 0; i < t.childCount; i++)
        {
            var c = t.GetChild(i);
            if (IsFromAsset(c, assetPath))
            {
                seen.TryGetValue(c.name, out int n);
                seen[c.name] = n + 1;
                var name = n == 0 ? c.name : c.name + "#" + n;
                Collect(c, path == "" ? name : path + "/" + name, assetPath, pathOf, added);
            }
            else
            {
                added.Add(new AddedObj { t = c, parentPath = path, pos = c.localPosition, rot = c.localRotation, scale = c.localScale });
            }
        }
    }

    static List<RefInfo> CollectRefs(UnityEngine.SceneManagement.Scene scene, Dictionary<Transform, string> oldPathOf)
    {
        var list = new List<RefInfo>();
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var comp in root.GetComponentsInChildren<Component>(true))
            {
                if (comp == null || comp is Transform) continue;
                if (oldPathOf.ContainsKey(comp.transform)) continue; // node của map cũ, xử lý riêng
                var so = new SerializedObject(comp);
                var it = so.GetIterator();
                while (it.Next(true))
                {
                    if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                    if (SkipProps.Contains(it.name)) continue;
                    var o = it.objectReferenceValue;
                    if (o == null) continue;
                    Transform tt = o is GameObject g ? g.transform : o is Component c ? c.transform : null;
                    if (tt == null || !oldPathOf.TryGetValue(tt, out var p)) continue;
                    var info = new RefInfo
                    {
                        owner = comp, propertyPath = it.propertyPath, targetPath = p,
                        isGameObject = o is GameObject, targetType = o.GetType()
                    };
                    if (!info.isGameObject)
                        info.componentIndex = Array.IndexOf(tt.GetComponents(o.GetType()), o);
                    list.Add(info);
                }
            }
        }
        return list;
    }

    static void RemapMaterials(StringBuilder log)
    {
        var oldImp = AssetImporter.GetAtPath(OldMapPath) as ModelImporter;
        var newImp = AssetImporter.GetAtPath(NewMapPath) as ModelImporter;
        if (oldImp == null || newImp == null) { log.AppendLine("  ! Không đọc được ModelImporter."); return; }
        var current = newImp.GetExternalObjectMap();
        int n = 0;
        foreach (var kv in oldImp.GetExternalObjectMap())
        {
            if (kv.Key.type != typeof(Material) || kv.Value == null) continue;
            if (current.TryGetValue(kv.Key, out var cur) && cur == kv.Value) continue;
            newImp.AddRemap(kv.Key, kv.Value);
            n++;
        }
        if (n > 0) newImp.SaveAndReimport();
        log.AppendLine($"Remap {n} material của FBX mới về MatMap.");
    }

    static void AddZoneSlots(List<Renderer> renderers, StringBuilder log)
    {
        if (renderers.Count == 0) return;
        var ce = UnityEngine.Object.FindObjectsByType<ChangeEnvironment>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (ce == null) { log.AppendLine("  ! Không thấy ChangeEnvironment."); return; }
        var so = new SerializedObject(ce);
        var datas = so.FindProperty("datas");
        int added = 0;
        for (int i = 0; i < datas.arraySize; i++)
        {
            var list = datas.GetArrayElementAtIndex(i).FindPropertyRelative("listRenderAndMat");
            foreach (var r in renderers)
            {
                bool exists = false;
                for (int j = 0; j < list.arraySize; j++)
                    if (list.GetArrayElementAtIndex(j).FindPropertyRelative("render").objectReferenceValue == r) { exists = true; break; }
                if (exists) continue;
                list.arraySize++;
                var el = list.GetArrayElementAtIndex(list.arraySize - 1);
                el.FindPropertyRelative("render").objectReferenceValue = r;
                var mats = el.FindPropertyRelative("materials");
                var shared = r.sharedMaterials;
                mats.arraySize = shared.Length;
                for (int k = 0; k < shared.Length; k++) mats.GetArrayElementAtIndex(k).objectReferenceValue = shared[k];
                added++;
            }
        }
        so.ApplyModifiedProperties();
        log.AppendLine($"ChangeEnvironment: thêm {added} slot cho vùng mới (material tạm = material Normal, art cần thay cho Lava/Night/Water/Radio/Heaven).");
    }

    static void RemapSceneMeshes(UnityEngine.SceneManagement.Scene scene, StringBuilder log)
    {
        var newMeshes = AssetDatabase.LoadAllAssetsAtPath(NewMapPath).OfType<Mesh>()
            .GroupBy(m => m.name).ToDictionary(g => g.Key, g => g.First());
        int n = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var mf in root.GetComponentsInChildren<MeshFilter>(true))
                if (IsOldMesh(mf.sharedMesh))
                {
                    if (newMeshes.TryGetValue(mf.sharedMesh.name, out var m)) { Undo.RecordObject(mf, "Replace Map"); mf.sharedMesh = m; n++; }
                    else log.AppendLine($"  ! Map mới không có mesh '{mf.sharedMesh.name}' ({mf.name})");
                }
            foreach (var mc in root.GetComponentsInChildren<MeshCollider>(true))
                if (IsOldMesh(mc.sharedMesh))
                {
                    var mf = mc.GetComponent<MeshFilter>();
                    Mesh m = mf != null && !IsOldMesh(mf.sharedMesh) ? mf.sharedMesh : null;
                    if (m == null) newMeshes.TryGetValue(mc.sharedMesh.name, out m);
                    if (m != null) { Undo.RecordObject(mc, "Replace Map"); mc.sharedMesh = m; n++; }
                    else log.AppendLine($"  ! Map mới không có mesh '{mc.sharedMesh.name}' ({mc.name})");
                }
        }
        log.AppendLine($"Đổi {n} tham chiếu mesh từ FBX cũ sang FBX mới.");
    }

    static bool IsOldMesh(Mesh m) => m != null && AssetDatabase.GetAssetPath(m) == OldMapPath;

    static int SubMeshCount(Renderer r)
    {
        var mf = r.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null) return mf.sharedMesh.subMeshCount;
        if (r is SkinnedMeshRenderer s && s.sharedMesh != null) return s.sharedMesh.subMeshCount;
        return r.sharedMaterials.Length;
    }

    // ------------------------------------------------------------------ VALIDATE

    static void Validate(bool dialog)
    {
        var sb = new StringBuilder("[MapReplacer] Validate\n");
        int errors = 0, warnings = 0;

        foreach (var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (IsOldMesh(mf.sharedMesh)) { sb.AppendLine($"  ERROR MeshFilter {mf.name} vẫn dùng mesh FBX cũ"); errors++; }
        foreach (var mc in UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (IsOldMesh(mc.sharedMesh)) { sb.AppendLine($"  ERROR MeshCollider {mc.name} vẫn dùng mesh FBX cũ"); errors++; }

        var ce = UnityEngine.Object.FindObjectsByType<ChangeEnvironment>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (ce == null) { sb.AppendLine("  ERROR không có ChangeEnvironment"); errors++; }
        else
        {
            var datas = new SerializedObject(ce).FindProperty("datas");
            for (int i = 0; i < datas.arraySize; i++)
            {
                var d = datas.GetArrayElementAtIndex(i);
                var type = d.FindPropertyRelative("type").enumDisplayNames[d.FindPropertyRelative("type").enumValueIndex];
                var list = d.FindPropertyRelative("listRenderAndMat");
                for (int j = 0; j < list.arraySize; j++)
                {
                    var el = list.GetArrayElementAtIndex(j);
                    var r = el.FindPropertyRelative("render").objectReferenceValue as Renderer;
                    int matCount = el.FindPropertyRelative("materials").arraySize;
                    if (r == null) { sb.AppendLine($"  ERROR {type}[{j}] render = None"); errors++; continue; }
                    int sub = SubMeshCount(r);
                    if (sub != matCount) { sb.AppendLine($"  WARN  {type}[{j}] {r.name}: {matCount} material / {sub} submesh"); warnings++; }
                }
            }
        }

        var newMap = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(t => PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject)
                                 && PrefabUtility.GetPrefabAssetPathOfNearestPrefabInstanceRoot(t.gameObject) == NewMapPath);
        if (newMap != null)
        {
            foreach (var mf in newMap.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!IsFromAsset(mf.transform, NewMapPath)) continue;
                if (mf.gameObject.layer == GroundLayer && mf.GetComponent<MeshCollider>() == null)
                { sb.AppendLine($"  WARN  {mf.name} layer Ground nhưng không có MeshCollider"); warnings++; }
                if (mf.name.StartsWith(NewZonePrefix) && mf.gameObject.layer != GroundLayer)
                { sb.AppendLine($"  WARN  {mf.name} chưa ở layer Ground"); warnings++; }
            }
        }
        else sb.AppendLine("  (Chưa có map mới trong scene)");

        sb.AppendLine($"=> {errors} lỗi, {warnings} cảnh báo.");
        if (errors > 0) Debug.LogError(sb.ToString());
        else if (warnings > 0) Debug.LogWarning(sb.ToString());
        else Debug.Log(sb.ToString());
        if (dialog) EditorUtility.DisplayDialog("Validate Map", $"{errors} lỗi, {warnings} cảnh báo. Xem Console.", "OK");
    }
}
