#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Raccoon.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Raccoon.EditorTool
{
    public class CreateAndEditBotModel : EditorWindow
    {
        private string brainrotModelFolderPath = "Assets/1_MyAssets/Models/Brainrot";
        private string soundBotPath = "Assets/1_MyAssets/Sounds/Bots";
        private string iconBotPath = "Assets/1_MyAssets/Images/Icon/Bots";
        private string iconBotPathNew = "Assets/1_MyAssets/Images/Icon/Brainrot";

        string pathBotSO = "";
        string pathBotPrefab = "";
        int indexApply = 0;

        TextAsset jsonFile;

        private AnimatorController baseController;
        // private Material baseMaterial;

        [MenuItem("Raccoon/Create And Edit Bot Data", priority = 1)]
        static void Init()
        {
            CreateAndEditBotModel window = (CreateAndEditBotModel)GetWindow(typeof(CreateAndEditBotModel), false, "Create and edit bot model");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Index apply from", GUILayout.Width(120));
            indexApply = int.Parse(GUILayout.TextField(indexApply.ToString()));
            GUILayout.EndHorizontal();

            GUILayout.Label("Bot Animator Override Generator", EditorStyles.boldLabel);

            // Base Animator Controller field
            baseController = (AnimatorController)EditorGUILayout.ObjectField(
                "Base Animator Controller",
                baseController,
                typeof(AnimatorController),
                false);

            // Bots folder path
            GUILayout.BeginHorizontal();
            GUILayout.Label("Bots Folder Path", GUILayout.Width(120));
            brainrotModelFolderPath = GUILayout.TextField(brainrotModelFolderPath);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Bots Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                {
                    brainrotModelFolderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
            GUILayout.EndHorizontal();
            // Create button
            if (GUILayout.Button("Create Animator Overrides", GUILayout.Height(30)))
            {
                if (baseController == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a Base Animator Controller", "OK");
                    return;
                }
                if (!AssetDatabase.IsValidFolder(brainrotModelFolderPath))
                {
                    EditorUtility.DisplayDialog("Error", "Invalid Bots folder path", "OK");
                    return;
                }

                GenerateOverrides();
            }

            GUILayout.Space(20);
            GUILayout.Label("Material Assignment", EditorStyles.boldLabel);
            // Material field
            // baseMaterial = (Material)EditorGUILayout.ObjectField("Base Material", baseMaterial, typeof(Material), false);

            if (GUILayout.Button("Init Model", GUILayout.Height(30)))
            {
                SetupModelProperty();
            }

            GUILayout.Space(20);
            GUILayout.Label("DataSO Assign Prefab", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("DataSO Folder Path", GUILayout.Width(120));
            pathBotSO = GUILayout.TextField(pathBotSO);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select DataSO Folder", "Assets/1_MyAssets", "");
                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                {
                    pathBotSO = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Prefab Folder Path", GUILayout.Width(120));
            pathBotPrefab = GUILayout.TextField(pathBotPrefab);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                string selected = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets/1_MyAssets", "");
                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                {
                    pathBotPrefab = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Set Bot Data SO", GUILayout.Height(30)))
            {
                SetPrefabToDataSO(pathBotSO, pathBotPrefab);
            }

            if (GUILayout.Button("Set Model To Prefab Bot", GUILayout.Height(30)))
            {
                SetModelToPrefab(pathBotPrefab);
            }
            
            // if (GUILayout.Button("Add outline To Prefab Bot", GUILayout.Height(30)))
            // {
            //     AddOutlineToPrefab(pathBotPrefab);
            // }

            GUILayout.Space(20);
            GUILayout.Label("DataSO Config", EditorStyles.boldLabel);
            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);

            if (GUILayout.Button("Import Config Bot", GUILayout.Height(30)))
            {
                ImportConfigBotData(jsonFile, pathBotSO);
            }

            if (GUILayout.Button("Rename dataSO", GUILayout.Height(30)))
            {
                RenameFileDataSO(pathBotSO);
            }

            if (GUILayout.Button("Set Sound DataSO", GUILayout.Height(30)))
            {
                SetSoundToData(pathBotSO);
            }

            if (GUILayout.Button("Set Icon DataSO", GUILayout.Height(30)))
            {
                SetIconToData(pathBotSO);
            }
            
            // if (GUILayout.Button("Update DataSO (rarity health)", GUILayout.Height(30)))
            // {
            //     UpdateDataSO(pathBotSO);
            // }
            
            // if (GUILayout.Button("Rename icon brainrot", GUILayout.Height(30)))
            // {
            //     RenameIconBrainrot();
            // }
        }
/// <summary>
/// 2 1
/// 3 4
/// 19 6
/// 20 7
/// 4 9
/// 25 10
/// 7 11
/// 10 12
/// 12 13
/// 49 14
/// 21 17
/// 13 19
/// 9 20
/// 42 21
/// 14 22
/// 18 25
/// 29 26
/// 38 27
/// 39 30
/// 51 31
/// 11 35
/// 33 37
/// 44 38
/// 43 39
/// </summary>
        private void RenameIconBrainrot()
        {
            var iconBotPathNew = "Assets/1_MyAssets/Images/Icon/Brainrot";
            var pathSpriteBase = $"{iconBotPathNew}/base";
            var pathSpriteGold = $"{iconBotPathNew}/gold";
            var pathSpriteDiamond = $"{iconBotPathNew}/diamond";
            var pathSpriteWhite = $"{iconBotPathNew}/white";

            // look for sprite with the same name
            string[] spriteGuids = AssetDatabase.FindAssets($"t:Sprite",
                new[] { pathSpriteBase, pathSpriteGold, pathSpriteDiamond, pathSpriteWhite });
            var count = 0;
            Debug.Log("spriteGuids " + spriteGuids.Length);
            foreach (var spritePath in spriteGuids)
            {
                //rename old to change new
                string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
                Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
                if (!int.TryParse(icon.name.Split('.')[0], out int num1))
                    continue;

                count++;
                AssetDatabase.RenameAsset(sprite, $"{icon.name}.old");
                Debug.Log(icon.name + " rename old "+ count);
            }

            Dictionary<int, int> map = new Dictionary<int, int>()
            {
                { 2, 1 },
                { 3, 4 },
                { 19, 6 },
                { 20, 7 },
                { 4, 9 },
                { 25, 10 },
                { 7, 11 },
                { 10, 12 },
                { 12, 13 },
                { 49, 14 },
                { 21, 17 },
                { 13, 19 },
                { 9, 20 },
                { 42, 21 },
                { 14, 22 },
                { 18, 25 },
                { 29, 26 },
                { 38, 27 },
                { 39, 30 },
                { 51, 31 },
                { 11, 35 },
                { 33, 37 },
                { 44, 38 },
                { 43, 39 },
            };
            count = 0;
            string[] spriteGuidsNew = AssetDatabase.FindAssets($"t:Sprite", new[] { pathSpriteBase,pathSpriteGold ,pathSpriteDiamond, pathSpriteWhite});
            Debug.Log("spriteGuidsNew " + spriteGuids.Length);
            foreach (var spritePath in spriteGuidsNew)
            {
                string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
                Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
                if (!int.TryParse(icon.name.Split('.')[0], out int num1))
                    continue;
                var newName = map[num1];
                
                AssetDatabase.RenameAsset(sprite, newName.ToString());
                count++;
                Debug.Log(num1 + " remap: " + newName + " -- " + count);

            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"Remap icon", "OK");
        }

        private void SetSoundToData(string pathSO)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });

            foreach (string soGuid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                BotDataSO botData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
                if (!int.TryParse(botData.name.Split(' ')[1], out int dtId)) continue;
                // try to find an .mp3 with the same name
                string[] audioGuids = AssetDatabase.FindAssets($"t:AudioClip", new[] { soundBotPath });
                foreach (var audio in audioGuids)
                {
                    string audioPath = AssetDatabase.GUIDToAssetPath(audio);
                    AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                    // split by '.' and parse first part
                    if (!int.TryParse(clip.name.Split('.')[0], out int num1))
                        continue;
                    if (num1 != dtId) continue;
                    botData.audioClip = clip;

                    EditorUtility.SetDirty(botData);
                    Debug.Log($"✅ Assigned {clip.name} → {botData.GetId}");
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorUtility.DisplayDialog("Success", $"Imported sound to bots!", "OK");
        }

        private void SetIconToData(string pathSO)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });
            string[] spriteGuidsLock = AssetDatabase.FindAssets($"t:Sprite", new[] { $"{iconBotPathNew}/white" });
            foreach (string soGuid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                BotDataSO botData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
                if (!int.TryParse(botData.name.Split(' ')[1], out int dtId)) continue;
                var pathSprite = $"{iconBotPathNew}/base";
                //if (botData.colorType == ZoneType.Golden)
                //    pathSprite = $"{iconBotPathNew}/gold";
                //if (botData.colorType == ZoneType.Diamond)
                //    pathSprite = $"{iconBotPathNew}/diamond";
                //if (botData.colorType == ZoneType.Rainbow)
                //    pathSprite = $"{iconBotPathNew}/base";

                // look for sprite with the same name
                string[] spriteGuids = AssetDatabase.FindAssets($"t:Sprite", new[] { pathSprite });

                foreach (var spritePath in spriteGuids)
                {
                    string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
                    // split by '.' and parse first part
                    if (!int.TryParse(icon.name.Split('.')[0], out int num1))
                        continue;
                    if (num1 != dtId) continue;
                    botData.spriteIcon = icon;

                    EditorUtility.SetDirty(botData);
                    Debug.Log($"✅ Assigned icon {icon.name} → {botData.GetId}");
                }

                foreach (var spritePath in spriteGuidsLock)
                {
                    string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
                    // split by '.' and parse first part
                    if (!int.TryParse(icon.name.Split('.')[0], out int num1))
                        continue;
                    if (num1 != dtId) continue;
                    botData.spriteIconLock = icon;

                    EditorUtility.SetDirty(botData);
                    Debug.Log($"✅ Assigned lock {icon.name} → {botData.GetId}");
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            EditorUtility.DisplayDialog("Success", $"Imported icon to bots!", "OK");

        }

        private void ImportConfigBotData(TextAsset jsonFile, string pathSO)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });
            string json = jsonFile.text;

            // Wrap array manually if needed
            if (json.Trim().StartsWith("["))
            {
                json = "{ \"items\": " + json + "}";
            }

            BotDataJsonList dataList = JsonUtility.FromJson<BotDataJsonList>(json);
            foreach (string soGuid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                BotDataSO botData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
                if (!int.TryParse(botData.name.Split(' ')[1], out int id)) continue;

                foreach (var item in dataList.items)
                {
                    if (item.id != id) continue;

                    //bot.id = item.id;
                    botData.itemName = item.name;
                    botData.rarityType = ConverRarityFromText(item.rarity);
                    botData.cost = item.sell * 2;
                    botData.income = item.income;
                    botData.health = item.health;

                    EditorUtility.SetDirty(botData);
                    Debug.Log($"Assigned prefab config to {botData.name}");
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", $"Imported {dataList.items.Length} bots!", "OK");
        }

        private void SetPrefabToDataSO(string pathSO, string pathPrefab)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { pathPrefab });

            foreach (string soGuid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                BotDataSO botData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
                if (botData == null) continue;

                if (!int.TryParse(botData.name.Split(' ')[1], out int dtId)) continue;

                if (dtId <= indexApply) continue;

                // try to find matching prefab by name
                foreach (string prefabGuid in prefabGuids)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null || !int.TryParse(prefab.name.Split('.')[0], out int idPrefab)) continue;
                    if (idPrefab == dtId)
                    {
                        botData.prefab = prefab;
                        //botData.bot_name = prefab.name.Replace(dtId.ToString(), "").Replace(".Bot ", "");
                        EditorUtility.SetDirty(botData);
                        string newName = $"{dtId}.Bot {botData.itemName}";
                        string renameResult = AssetDatabase.RenameAsset(prefabPath, newName);
                        Debug.Log($"Assigned prefab {prefab.name} to {botData.name} and change name {renameResult}");
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        private void SetModelToPrefab(string pathPrefab)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { pathPrefab });
            string[] botFolders = AssetDatabase.GetSubFolders(brainrotModelFolderPath);

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null) continue;

                // split by '.' and parse first part
                if (!int.TryParse(prefabAsset.name.Split('.')[0], out int num1))
                    continue;

                if (num1 <= indexApply) continue;

                foreach (var botFolder in botFolders)
                {
                    string botName = Path.GetFileName(botFolder);

                    if (!int.TryParse(botName.Split('.')[0], out int num2))
                        continue;

                    if (num1 != num2) continue;

                    string[] fbxFiles = Directory.GetFiles(botFolder, "*.fbx", SearchOption.TopDirectoryOnly);
                    string[] existingOverrides = Directory.GetFiles(botFolder, "*.overrideController", SearchOption.TopDirectoryOnly);
                    if(existingOverrides.Length < 0)
                    {
                        Debug.Log($"Skipped {botName}, override donot exists in folder.");
                        continue;
                    }

                    var animator = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(existingOverrides[0].Replace(Application.dataPath, "Assets"));
                    foreach (var fbxFile in fbxFiles)
                    {
                        if (fbxFile.ToLower().Contains($"character"))
                        {
                            string assetPath = fbxFile.Replace(Application.dataPath, "Assets"); // convert absolute path to relative
                            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
                            Transform modelParent = prefabContents.transform.Find("Model");
                            if (modelParent == null)
                            {
                                Debug.LogWarning($"Prefab {prefabAsset.name} does not have a 'Model' child.");
                                PrefabUtility.UnloadPrefabContents(prefabContents);
                                continue;
                            }

                            // clear existing children in "Model"
                            for (int i = modelParent.childCount - 1; i >= 0; i--)
                            {
                                Object.DestroyImmediate(modelParent.GetChild(i).gameObject, true);
                            }

                            // add new model
                            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                            modelInstance.transform.SetParent(modelParent, false);
                            var anim = modelInstance.GetComponent<Animator>();
                            if (anim == null)
                                anim = modelInstance.AddComponent<Animator>();

                            anim.runtimeAnimatorController = animator;
                            //add 

                            // ✅ save changes to prefab asset
                            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                            PrefabUtility.UnloadPrefabContents(prefabContents);

                            Debug.Log($"✅ Assigned {model.name} to prefab {prefabAsset.name} under Model/");
                        }
                    }
                }
            }
        }

        private void SetupModelProperty()
        {
            string[] botFolders = AssetDatabase.GetSubFolders(brainrotModelFolderPath);
            foreach (var botFolder in botFolders)
            {
                string botName = Path.GetFileName(botFolder);
                // split by '.' and parse first part
                if (!int.TryParse(botName.Split('.')[0], out int num1))
                    continue;

                if (num1 <= indexApply) continue;
                // Find clips from FBX
                string[] fbxFiles = Directory.GetFiles(botFolder, "*.fbx", SearchOption.TopDirectoryOnly);
                foreach (var fbxPath in fbxFiles)
                {
                    string assetPath = fbxPath.Replace("\\", "/");
                    var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    importer.globalScale = 6.5f;
                    importer.importBlendShapes = false;
                    importer.importVisibility = false;
                    importer.importCameras = false;
                    importer.importLights = false;
                    importer.weldVertices = false;
                    importer.meshCompression = ModelImporterMeshCompression.Low;
                    importer.isReadable = true;
                    // if (fbxPath.ToLower().Contains($"character"))
                    // {
                    //     var so = new SerializedObject(importer);
                    //     var mats = so.FindProperty("m_Materials");
                    //     if (mats != null && mats.isArray)
                    //     {
                    //         for (int i = 0; i < mats.arraySize; i++)
                    //         {
                    //             var matProp = mats.GetArrayElementAtIndex(i);
                    //             var matName = matProp.FindPropertyRelative("name").stringValue;
                    //
                    //             if (!string.IsNullOrEmpty(matName))
                    //             {
                    //                 var id = new AssetImporter.SourceAssetIdentifier(typeof(Material), matName);
                    //                 importer.AddRemap(id, baseMaterial);
                    //
                    //                 Debug.Log($"Remapped slot '{matName}' in {Path.GetFileName(assetPath)} → {baseMaterial.name}");
                    //             }
                    //         }
                    //     }
                    // }

                    // Get all clip settings
                    var clips = importer.defaultClipAnimations;
                    if (clips != null && clips.Length > 0)
                    {
                        for (int i = 0; i < clips.Length; i++)
                        {
                            clips[i].loopTime = true;   // ⏺ make it loop
                            clips[i].loopPose = true;   // optional: match first/last frame
                        }

                        importer.clipAnimations = clips;
                    }

                    AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);
                    AssetDatabase.ImportAsset(importer.assetPath, ImportAssetOptions.ForceUpdate);
                }
            }
            EditorUtility.DisplayDialog("Done", "Remapped FBX materials to Base Material!", "OK");
        }

        private void GenerateOverrides()
        {
            string[] botFolders = AssetDatabase.GetSubFolders(brainrotModelFolderPath);

            foreach (var botFolder in botFolders)
            {
                string botName = Path.GetFileName(botFolder);

                // ✅ check if any override file already exists in this folder
                string[] existingOverrides = Directory.GetFiles(botFolder, "*.overrideController", SearchOption.TopDirectoryOnly);
                if (existingOverrides.Length > 0)
                {
                    Debug.Log($"Skipped {botName}, override already exists in folder.");
                    continue;
                }

                // Find clips from FBX
                string[] fbxFiles = Directory.GetFiles(botFolder, "*.fbx", SearchOption.TopDirectoryOnly);

                AnimationClip idleClip = null;
                AnimationClip walkClip = null;

                foreach (var fbxPath in fbxFiles)
                {
                    var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                    var clips = importer.defaultClipAnimations;
                    foreach (var clip in clips)
                    {
                        string lower = clip.name.ToLower();
                        if (lower.Contains("idle") && idleClip == null)
                        {
                            idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbxPath);
                            Debug.Log($"🟢 Found idle clip in {Path.GetFileName(fbxPath)}: {clip.name}");
                        }
                        else if (lower.Contains("walk") && walkClip == null)
                        {
                            walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbxPath);
                            Debug.Log($"🟢 Found walk clip in {Path.GetFileName(fbxPath)}: {clip.name}");
                        }
                        else if (lower.Contains("run") && walkClip == null)
                        {
                            walkClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fbxPath);
                            Debug.Log($"🟢 Found walk clip in {Path.GetFileName(fbxPath)}: {clip.name}");
                        }
                    }
                }

                if (idleClip == null || walkClip == null)
                {
                    Debug.LogWarning($"Bot {botName} missing idle or walk animation in FBX files!");
                    //continue;
                }

                // Create override
                AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);

                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);

                for (int i = 0; i < overrides.Count; i++)
                {
                    string stateName = overrides[i].Key.name.ToLower();
                    if (stateName.Contains("idle"))
                        overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, idleClip);
                    else if (stateName.Contains("walk"))
                        overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, walkClip);
                }

                overrideController.ApplyOverrides(overrides);

                // Save with default naming if no override exists
                string savePath = botFolder + "/" + botName + "_Override.overrideController";
                AssetDatabase.CreateAsset(overrideController, savePath);
                if (idleClip != null && walkClip != null)
                    Debug.Log($"Created override for {botName} at {savePath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Done", "Bot Animator Overrides created!", "OK");
        }

        private void RenameFileDataSO(string pathSO)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });

            foreach (string guid in soGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                string extentName = "";

                if (assetPath.ToLower().Contains("golden")) extentName = "golden";
                else if (assetPath.ToLower().Contains("diamond")) extentName = "diamond";
                else if (assetPath.ToLower().Contains("rainbow")) extentName = "rainbow";
                else continue;

                // Example: rename by adding "_SO" suffix
                string newName = extentName + fileName;
                if (newName != fileName)
                {
                    AssetDatabase.RenameAsset(assetPath, newName);
                    Debug.Log($"✅ Renamed {fileName} → {newName}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void AddOutlineToPrefab(string pathPrefab)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { pathPrefab });
            string[] botFolders = AssetDatabase.GetSubFolders(brainrotModelFolderPath);

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabAsset == null) continue;

                // split by '.' and parse first part
                if (!int.TryParse(prefabAsset.name.Split('.')[0], out int num1))
                    continue;

                if (num1 <= indexApply) continue;

                GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
                Transform model = prefabContents.transform.Find("Model").GetChild(0);
                // var outline = model.AddComponent<cakeslice.Outline>();
                // ✅ save changes to prefab asset
                PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabContents);

                Debug.Log($"✅ Add Outline to {model.name} at prefab {prefabAsset.name} under Model/");

            }
        }

        private RarityType ConverRarityFromText(string txt)
        {
            switch (txt)
            {
                case "Rare": return RarityType.Rare;
                case "Epic": return RarityType.Epic;
                case "Legendary": return RarityType.Legendary;
                case "Mythic": return RarityType.Mythic;
                case "Godly": return RarityType.Godly;
                case "Secret": return RarityType.Secret;
                case "Limited": return RarityType.Limited;
            }
            return RarityType.Common;
        }

        private void UpdateDataSO(string pathSO)
        {
            string[] soGuids = AssetDatabase.FindAssets("t:BotDataSO", new[] { pathSO });

            foreach (string guid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(guid);
                BotDataSO botData = AssetDatabase.LoadAssetAtPath<BotDataSO>(soPath);
                if (botData == null) continue;

                if (!int.TryParse(botData.name.Split(' ')[1], out int dtId)) continue;
                //change all rarity common to rare
                if (botData.rarityType == RarityType.Common)
                    botData.rarityType = RarityType.Rare;
                // if (soPath.Contains("golden") || soPath.Contains("diamond") || soPath.Contains("rainbow"))
                // {
                //     var soNormal = soPath
                //         .Replace("/Bots_golden", "").Replace("golden", "")
                //         .Replace("/Bots_diamond", "").Replace("diamond", "")
                //         .Replace("/Bots_rainbow", "").Replace("rainbow", "");
                //     Debug.Log("SO Normal: " + soNormal);
                //     BotDataSO botNormal = AssetDatabase.LoadAssetAtPath<BotDataSO>(soNormal);
                //     botData.rarityType = botNormal.rarityType;
                // }

                botData.health = GetBaseHPByRarity(botData.rarityType) +
                                 Mathf.RoundToInt(botData.income * Mathf.PI * 6);
                EditorUtility.SetDirty(botData);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static int GetBaseHPByRarity(RarityType rarity)
        {
            return rarity switch
            {
                RarityType.Epic => 150,
                RarityType.Legendary => 450,
                RarityType.Mythic => 1050,
                RarityType.Godly => 1950,
                RarityType.Secret => 3150,
                RarityType.Limited => 4450,
                _ => 100
            };
        }
    }

    [System.Serializable]
    public class BotDataJson
    {
        public int id;
        public string name;
        public string rarity;
        public int sell;
        public int income;
        public int health;
    }

    [System.Serializable]
    public class BotDataJsonList
    {
        public BotDataJson[] items;
    }
}
#endif