#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Raccoon.EditorTool
{
    public class FBXSyncTool : EditorWindow
    {
        private string oldRoot = "";
        private string newRoot = "";

        [MenuItem("Raccoon/FBX Sync Tool")]
        public static void ShowWindow()
        {
            GetWindow<FBXSyncTool>("FBX Sync Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("FBX Sync Tool (Auto Scan Subfolders)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // --- OLD ROOT PICKER ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Old Root Folder", GUILayout.Width(110));
            EditorGUILayout.SelectableLabel(oldRoot, GUILayout.Height(18));
            if (GUILayout.Button("Select...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Old Root Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    oldRoot = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // --- NEW ROOT PICKER ---
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Root Folder", GUILayout.Width(110));
            EditorGUILayout.SelectableLabel(newRoot, GUILayout.Height(18));
            if (GUILayout.Button("Select...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("Select New Root Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                {
                    newRoot = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            GUI.enabled = !string.IsNullOrEmpty(oldRoot) && !string.IsNullOrEmpty(newRoot);

            if (GUILayout.Button("Copy FBX", GUILayout.Height(30)))
            {
                CopyFBX();
            }

            if (GUILayout.Button("Move Material", GUILayout.Height(30)))
            {
                MoveMaterial();
            }
            
            if (GUILayout.Button("Remap material FBX", GUILayout.Height(30)))
            {
                RemapAllMaterialsInOldFolder(oldRoot);
            }

            if (GUILayout.Button("Sync All FBX", GUILayout.Height(30)))
            {
                SyncAll();
            }

            GUI.enabled = true;
        }

        private void SyncAll()
        {
            if (!Directory.Exists(oldRoot) || !Directory.Exists(newRoot))
            {
                Debug.LogError("❌ One of the root folders does not exist.");
                return;
            }

            string[] newFBXs = Directory.GetFiles(newRoot, "*.fbx", SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in newFBXs)
            {
                string newFBXPath = file.Replace("\\", "/");
                string fileName = Path.GetFileName(newFBXPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(newFBXPath));

                // Find matching old folder: "1" matches "1.NameBot"
                string matchingOldFolder = FindOldFolder(folderName);
                if (matchingOldFolder == null)
                {
                    Debug.LogWarning($"⚠ No old folder matches: {newFBXPath}");
                    continue;
                }

                string oldFBXPath = $"{matchingOldFolder}/{fileName}";

                // Copy override FBX
                CopyFBXOverride(newFBXPath, oldFBXPath);

                // Move materials folder
                MoveAllMaterials(newFBXPath, matchingOldFolder);

                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"✅ Sync completed. Processed {count} FBX files.");
        }

        private void CopyFBX()
        {
            if (!Directory.Exists(oldRoot) || !Directory.Exists(newRoot))
            {
                Debug.LogError("❌ One of the root folders does not exist.");
                return;
            }

            string[] newFBXs = Directory.GetFiles(newRoot, "*.fbx", SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in newFBXs)
            {
                string newFBXPath = file.Replace("\\", "/");
                string fileName = Path.GetFileName(newFBXPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(newFBXPath));

                // Find matching old folder: "1" matches "1.NameBot"
                string matchingOldFolder = FindOldFolder(folderName);
                if (matchingOldFolder == null)
                {
                    Debug.LogWarning($"⚠ No old folder matches: {newFBXPath}");
                    continue;
                }

                string oldFBXPath = $"{matchingOldFolder}/{fileName}";

                // Copy override FBX
                CopyFBXOverride(newFBXPath, oldFBXPath);

                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"✅ Sync completed. Processed {count} FBX files.");
        }

        private void MoveMaterial()
        {
            if (!Directory.Exists(oldRoot) || !Directory.Exists(newRoot))
            {
                Debug.LogError("❌ One of the root folders does not exist.");
                return;
            }

            string[] newFBXs = Directory.GetFiles(newRoot, "*.fbx", SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in newFBXs)
            {
                string newFBXPath = file.Replace("\\", "/");
                string fileName = Path.GetFileName(newFBXPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(newFBXPath));

                // Find matching old folder: "1" matches "1.NameBot"
                string matchingOldFolder = FindOldFolder(folderName);
                if (matchingOldFolder == null)
                {
                    Debug.LogWarning($"⚠ No old folder matches: {newFBXPath}");
                    continue;
                }

                // Move materials folder
                MoveAllMaterials(newFBXPath, matchingOldFolder);

                count++;
            }

            AssetDatabase.Refresh();
            Debug.Log($"✅ Sync completed. Processed {count} FBX files.");
        }

        private string FindOldFolder(string newFolderName)
        {
            string[] oldFolders = Directory.GetDirectories(oldRoot, "*", SearchOption.AllDirectories);

            foreach (string f in oldFolders)
            {
                string folder = f.Replace("\\", "/");
                string name = Path.GetFileName(folder);

                // Example: new folder = "1", old folder = "1.NameBot"
                if (name.StartsWith(newFolderName))
                    return folder;
            }

            return null;
        }

        private void CopyFBXOverride(string src, string dst)
        {
            string targetDir = Path.GetDirectoryName(dst);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            if (File.Exists(dst))
                File.Delete(dst);

            File.Copy(src, dst, true);

            Debug.Log($"✔ FBX Overridden:\n→ {dst}");
        }

        private void MoveAllMaterials(string newFolderRoot, string oldFolder)
        {
            if (File.Exists(newFolderRoot))
            {
                newFolderRoot = Path.GetDirectoryName(newFolderRoot);
                Debug.Log($"Corrected newFolderRoot to folder: {newFolderRoot}");
            }

            // Find all .mat files under the new folder
            string[] matFiles = Directory.GetFiles(newFolderRoot, "*.mat", SearchOption.AllDirectories);

            if (matFiles.Length == 0)
            {
                Debug.Log("No materials found in: " + newFolderRoot);
                return;
            }

            foreach (var matPath in matFiles)
            {
                string fileName = Path.GetFileName(matPath);
                string targetPath = Path.Combine(oldFolder, fileName);

                Debug.Log($"Moving mat: {matPath} → {targetPath}");

                AssetDatabase.MoveAsset(matPath, targetPath);
            }

            AssetDatabase.Refresh();
        }

        private void RemapAllMaterialsInOldFolder(string oldFolderRoot)
        {
            // Fix if user selected a file instead of a folder
            if (File.Exists(oldFolderRoot))
                oldFolderRoot = Path.GetDirectoryName(oldFolderRoot);

            if (!Directory.Exists(oldFolderRoot))
            {
                Debug.LogError("Old folder does not exist: " + oldFolderRoot);
                return;
            }
            // ------------------------
            // Load all FBX files inside old folder
            // ------------------------
            string[] fbxFiles = Directory.GetFiles(oldFolderRoot, "*.fbx", SearchOption.AllDirectories);
            foreach (string fbxPath in fbxFiles)
            {
                if (!fbxPath.Contains("Character")) continue;
                var fbxFolder = Path.GetDirectoryName(fbxPath);
                string[] matFiles = Directory.GetFiles(fbxFolder, "*.mat", SearchOption.AllDirectories);
                
                Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();

                foreach (var matPath in matFiles)
                {
                    string unityMatPath = ToUnityPath(matPath);
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(unityMatPath);
                    if (mat != null)
                    {
                        materialLookup.TryAdd(mat.name, mat);
                    }
                }

                Debug.Log($"[OLD] Found {materialLookup.Count} materials");
                
                string unityPath = ToUnityPath(fbxPath);

                Debug.Log("Remapping OLD FBX: " + unityPath);

                var importer = AssetImporter.GetAtPath(unityPath) as ModelImporter;
                if (importer == null)
                    continue;

                SerializedObject so = new SerializedObject(importer);
                SerializedProperty matsArray = so.FindProperty("m_Materials");

                bool changed = false;

                // ------------------------
                // Assign correct material to each slot
                // ------------------------
                for (int i = 0; i < matsArray.arraySize; i++)
                {
                    SerializedProperty matElement = matsArray.GetArrayElementAtIndex(i);
                    SerializedProperty nameProp = matElement.FindPropertyRelative("name");

                    string matName = nameProp.stringValue;

                    if (materialLookup.TryGetValue(matName, out Material replacement))
                    {
                        var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), matName);
                        importer.AddRemap(id, replacement);

                        Debug.Log($"[OLD] Map: {matName} → {AssetDatabase.GetAssetPath(replacement)}");
                        changed = true;
                    }
                    else
                    {
                        Debug.LogWarning($"[OLD] Material NOT found: {matName}");
                    }
                }

                if (changed)
                {
                    so.ApplyModifiedProperties();
                    importer.SaveAndReimport();
                }
            }

            Debug.Log("Finished remapping OLD folder FBXs!");
        }

        private string ToUnityPath(string path)
        {
            return path.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        }

    }
}
#endif
