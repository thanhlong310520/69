//#if UNITY_EDITOR

//using System.IO;
//using Raccoon.Data;
//using UnityEditor;
//using UnityEngine;

//namespace Raccoon.EditorTool
//{
//    public class CreateAndEditPlant : EditorWindow
//    {
//        private string modelPlantPath = "Assets/1_MyAssets/Models/Plants";
//        private string soundPlantPath = "Assets/1_MyAssets/Sounds/Plants";
//        private string iconPlantPath = "Assets/1_MyAssets/Images/Icon/Plants";
        
//        string pathPlantSO = "";
//        string pathPlantPrefab = "";
        
//        TextAsset jsonFile;
        
//        [MenuItem("Raccoon/Create And Edit Plant Data", priority = 2)]
//        static void Init()
//        {
//            CreateAndEditPlant window = (CreateAndEditPlant)GetWindow(typeof(CreateAndEditPlant), false, "Create and edit plant model");
//            window.Show();
//        }
        
//        private void OnGUI()
//        {
//            // Bots folder path
//            GUILayout.BeginHorizontal();
//            GUILayout.Label("Plants Folder Path", GUILayout.Width(120));
//            modelPlantPath = GUILayout.TextField(modelPlantPath);
//            if (GUILayout.Button("Select", GUILayout.Width(60)))
//            {
//                string selected = EditorUtility.OpenFolderPanel("Select Plant Folder", "Assets", "");
//                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
//                {
//                    modelPlantPath = "Assets" + selected.Substring(Application.dataPath.Length);
//                }
//            }
//            GUILayout.EndHorizontal();

//            GUILayout.Space(20);
//            GUILayout.Label("DataSO Assign Prefab", EditorStyles.boldLabel);
//            GUILayout.BeginHorizontal();
//            GUILayout.Label("DataSO Folder Path", GUILayout.Width(120));
//            pathPlantSO = GUILayout.TextField(pathPlantSO);
//            if (GUILayout.Button("Select", GUILayout.Width(60)))
//            {
//                string selected = EditorUtility.OpenFolderPanel("Select DataSO Folder", "Assets/1_MyAssets", "");
//                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
//                {
//                    pathPlantSO = "Assets" + selected.Substring(Application.dataPath.Length);
//                }
//            }

//            GUILayout.EndHorizontal();

//            GUILayout.BeginHorizontal();
//            GUILayout.Label("Prefab Folder Path", GUILayout.Width(120));
//            pathPlantPrefab = GUILayout.TextField(pathPlantPrefab);
//            if (GUILayout.Button("Select", GUILayout.Width(60)))
//            {
//                string selected = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets/1_MyAssets", "");
//                if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
//                {
//                    pathPlantPrefab = "Assets" + selected.Substring(Application.dataPath.Length);
//                }
//            }
//            GUILayout.EndHorizontal();
//            // if (GUILayout.Button("Set Bot Data SO", GUILayout.Height(30)))
//            // {
//            //     SetPrefabToDataSO(pathPlantSO, pathPlantPrefab);
//            // }

//            GUILayout.Space(20);
//            GUILayout.Label("DataSO Config", EditorStyles.boldLabel);
//            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);

//            if (GUILayout.Button("Import Config Bot", GUILayout.Height(30)))
//            {
//                ImportConfigPlantData(pathPlantSO);
//            }

//            if (GUILayout.Button("Rename dataSO", GUILayout.Height(30)))
//            {
//                RenameFileDataSO(pathPlantSO);
//            }
            
//            if (GUILayout.Button("Assign prefab to dataSO", GUILayout.Height(30)))
//            {
//                AssignPrefabToDataSO(pathPlantSO, pathPlantPrefab);
//            }
            
            
//            // if (GUILayout.Button("Rename Icon", GUILayout.Height(30)))
//            // {
//            //     RenameFileIconDataSO();
//            // }
            
//            if (GUILayout.Button("Set Icon DataSO", GUILayout.Height(30)))
//            {
//                SetIconToData(pathPlantSO);
//            }
//        }
        
//        private void SetIconToData(string pathSO)
//        {
//            string[] soGuids = AssetDatabase.FindAssets("t:PlantDataSO", new[] { pathSO });
//            string[] spriteGuidsLock = AssetDatabase.FindAssets($"t:Sprite", new[] { $"{iconPlantPath}/white" });
//            foreach (string soGuid in soGuids)
//            {
//                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
//                var botData = AssetDatabase.LoadAssetAtPath<PlantDataSO>(soPath);
//                if (!int.TryParse(botData.name.Split(' ')[1], out int dtId)) continue;
//                var pathSprite = $"{iconPlantPath}/base";
//                if (botData.colorType == ColorType.Golden)
//                    pathSprite = $"{iconPlantPath}/gold";
//                if (botData.colorType == ColorType.Diamond)
//                    pathSprite = $"{iconPlantPath}/diamond";
//                if (botData.colorType == ColorType.Rainbow)
//                    pathSprite = $"{iconPlantPath}/base";

//                // look for sprite with the same name
//                string[] spriteGuids = AssetDatabase.FindAssets($"t:Sprite", new[] { pathSprite });

//                foreach (var spritePath in spriteGuids)
//                {
//                    string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
//                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
//                    // split by '.' and parse first part
//                    if (!int.TryParse(icon.name.Split('.')[0], out int num1))
//                        continue;
//                    if (num1 != dtId) continue;
//                    botData.spriteIcon = icon;

//                    EditorUtility.SetDirty(botData);
//                    Debug.Log($"✅ Assigned icon {icon.name} → {botData.GetId}");
//                }

//                foreach (var spritePath in spriteGuidsLock)
//                {
//                    string sprite = AssetDatabase.GUIDToAssetPath(spritePath);
//                    Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(sprite);
//                    // split by '.' and parse first part
//                    if (!int.TryParse(icon.name.Split('.')[0], out int num1))
//                        continue;
//                    if (num1 != dtId) continue;
//                    botData.spriteIconLock = icon;

//                    EditorUtility.SetDirty(botData);
//                    Debug.Log($"✅ Assigned lock {icon.name} → {botData.GetId}");
//                }
//                AssetDatabase.SaveAssets();
//                AssetDatabase.Refresh();
//            }
//            EditorUtility.DisplayDialog("Success", $"Imported icon to bots!", "OK");

//        }

//        private void AssignSoundToPrefab(string s)
//        {
            
//        }

//        private void ImportConfigPlantData(string pathDataRoot)
//        {
//            int count = 0;
//            for (int i = 0; i < 4; i++)
//            {
//                var pathSO = pathDataRoot + "/" + ConvertIntToSubFolder(i);
//                var pathJson = pathDataRoot + "/plant_config_" + ConvertIntToSubFolder(i).ToLower() + ".json";
//                var jsonFile = AssetDatabase.LoadAssetAtPath(pathJson, typeof(TextAsset)) as TextAsset;
//                string[] soGuids = AssetDatabase.FindAssets("t:PlantDataSO", new[] { pathSO });
//                string json = jsonFile.text;

//                // Wrap array manually if needed
//                if (json.Trim().StartsWith("["))
//                {
//                    json = "{ \"items\": " + json + "}";
//                }

//                PlantDataJsonList dataList = JsonUtility.FromJson<PlantDataJsonList>(json);
//                foreach (string soGuid in soGuids)
//                {
//                    string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
//                    PlantDataSO plantDataSo = AssetDatabase.LoadAssetAtPath<PlantDataSO>(soPath);
//                    if (!int.TryParse(plantDataSo.name.Split(' ')[1], out int id)) continue;

//                    foreach (var item in dataList.items)
//                    {
//                        if (item.id != id) continue;

//                        //bot.id = item.id;
//                        plantDataSo.itemName = item.name;
//                        plantDataSo.rarityType = ConverRarityFromText(item.rarity);
//                        plantDataSo.cost = item.cost;
//                        plantDataSo.damage = item.damage;
//                        plantDataSo.attack_speed = item.attack_speed;
//                        plantDataSo.time_grow = GetGrowTimeSeconds(plantDataSo.rarityType, item.damage);
//                        EditorUtility.SetDirty(plantDataSo);
//                        Debug.Log($"Assigned prefab config to {plantDataSo.name}");
//                        count++;
//                    }
//                }
//            }
            
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();

//            EditorUtility.DisplayDialog("Success", $"Imported {count} plants!", "OK");
//        }
        
//        private void RenameFileDataSO(string pathSO)
//        {
//            string[] soGuids = AssetDatabase.FindAssets("t:PlantDataSO", new[] { pathSO });

//            foreach (string guid in soGuids)
//            {
//                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
//                string fileName = Path.GetFileNameWithoutExtension(assetPath);
//                string extentName = "";

//                if (assetPath.ToLower().Contains("golden")) extentName = "golden";
//                else if (assetPath.ToLower().Contains("diamond")) extentName = "diamond";
//                else if (assetPath.ToLower().Contains("rainbow")) extentName = "rainbow";
//                else continue;

//                // Example: rename by adding "_SO" suffix
//                string newName = extentName + fileName;
//                if (newName != fileName)
//                {
//                    AssetDatabase.RenameAsset(assetPath, newName);
//                    Debug.Log($"✅ Renamed {fileName} → {newName}");
//                }
//            }

//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//        }
        
//        private void RenameFileIconDataSO()
//        {
//            string[] soGuids = AssetDatabase.FindAssets($"t:Sprite", new[] { $"{iconPlantPath}" });
            
//            foreach (string guid in soGuids)
//            {
//                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
//                string fileName = Path.GetFileNameWithoutExtension(assetPath);
//                string extentName = "";

//                // if (assetPath.ToLower().Contains("golden")) extentName = "golden";
//                // else if (assetPath.ToLower().Contains("diamond")) extentName = "diamond";
//                // else if (assetPath.ToLower().Contains("rainbow")) extentName = "rainbow";
//                // else continue;
//                //
//                // // Example: rename by adding "_SO" suffix
//                var newName = fileName.Replace("plant ", "").Replace(" diamond", "").Replace(" gold", "")
//                    .Replace(" white", "").Replace("a","");
//                if(!int.TryParse(newName, out int id)) continue;
//                // newName = id switch
//                // {
//                //     1 => "10a",
//                //     2 => "11a",
//                //     3 => "12a",
//                //     4 => "16a",
//                //     5 => "15a",
//                //     6 => "14a",
//                //     7 => "19a",
//                //     8 => "17a",
//                //     9 => "18a",
//                //     10 => "3a",
//                //     11 => "1a",
//                //     12 => "2a",
//                //     13 => "5a",
//                //     14 => "6a",
//                //     15 => "4a",
//                //     16 => "8a",
//                //     17 => "7a",
//                //     18 => "9a",
//                //     19 => "13a",
//                //     _ => newName
//                // };

//                if (newName == fileName) continue;
//                AssetDatabase.RenameAsset(assetPath, newName);
//                Debug.Log($"✅ Renamed {fileName} - {id} → {newName}");
//            }

//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//        }

//        private void AssignPrefabToDataSO(string pathSO, string pathPrefabRoot)
//        {
//            var count = 0;
//            // Get all SOs
//            string[] soGuids = AssetDatabase.FindAssets("t:PlantDataSO", new[] { pathSO });

//            foreach (string guid in soGuids)
//            {
//                string soPath = AssetDatabase.GUIDToAssetPath(guid);
//                PlantDataSO dataSO = AssetDatabase.LoadAssetAtPath<PlantDataSO>(soPath);
//                if (dataSO == null)
//                    continue;

//                // --- Parse index from SO name ---
//                // "PlantDataSO 3" → 3
//                int indexSO = ExtractNumber(dataSO.name);
//                if (indexSO < 0)
//                {
//                    Debug.LogWarning($"Cannot parse index from SO: {dataSO.name}");
//                    continue;
//                }
//                //set order of index
//                dataSO.order = indexSO;
//                // --- Find matching folder in prefab root ---
//                string[] folderGuids = AssetDatabase.FindAssets("", new[] { pathPrefabRoot });

//                string matchedFolderPath = null;
//                foreach (string fGuid in folderGuids)
//                {
//                    string folderPath = AssetDatabase.GUIDToAssetPath(fGuid);
//                    if (!AssetDatabase.IsValidFolder(folderPath)) continue;

//                    int indexFolder = ExtractNumber(Path.GetFileName(folderPath));
                    
//                    if (indexFolder == indexSO)
//                    {
//                        matchedFolderPath = folderPath;
//                        break;
//                    }
//                }

//                if (matchedFolderPath == null)
//                {
//                    Debug.LogWarning($"No folder matched for SO index {indexSO}: {dataSO.name}");
//                    continue;
//                }

//                // --- Get prefabs in folder ---
//                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { matchedFolderPath });

//                GameObject selectedPrefab = null;
//                foreach (string pGuid in prefabGuids)
//                {
//                    string pPath = AssetDatabase.GUIDToAssetPath(pGuid);
//                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);

//                    if (prefab == null)
//                        continue;

//                    // skip bullet object
//                    if (prefab.name.ToLower().Contains("bullet"))
//                        continue;

//                    selectedPrefab = prefab;
//                    break;
//                }

//                if (selectedPrefab == null)
//                {
//                    Debug.LogWarning($"No valid prefab found for SO index {indexSO} in folder {matchedFolderPath}");
//                    continue;
//                }

//                // --- Assign ---
//                dataSO.prefab = selectedPrefab;
//                count++;
//                EditorUtility.SetDirty(dataSO);

//                Debug.Log($"Assigned prefab '{selectedPrefab.name}' → '{dataSO.name}'");
//            }

//            AssetDatabase.SaveAssets();
            
//            EditorUtility.DisplayDialog("Success", $"Assigned {count} plants!", "OK");
//        }


//        private static RarityType ConverRarityFromText(string txt)
//        {
//            switch (txt)
//            {
//                case "Rare": return RarityType.Rare;
//                case "Epic": return RarityType.Epic;
//                case "Legendary": return RarityType.Legendary;
//                case "Mythic": return RarityType.Mythic;
//                case "Godly": return RarityType.Godly;
//                case "Secret": return RarityType.Secret;
//            }
//            return RarityType.Common;
//        }

//        private string ConvertIntToSubFolder(int index)
//        {
//            return index switch
//            {
//                1 => "Golden",
//                2 => "Diamond",
//                3 => "Rainbow",
//                _ => "Normal"
//            };
//        }
        
//        private int GetBaseGrowTimeSeconds(RarityType rarity)
//        {
//            return rarity switch
//            {
//                RarityType.Rare => 20,          // 10–40
//                RarityType.Epic => 90,         // 1.5min
//                RarityType.Legendary => 240,    // 4 min
//                RarityType.Mythic => 480,       // 8 min
//                RarityType.Godly => 600,        // 10 min
//                RarityType.Secret => 900,      // 15 min
//                _ => 20
//            };
//        }
        
//        private float GetAvgDamageByRarity(RarityType rarity)
//        {
//            return rarity switch
//            {
//                RarityType.Rare => 40f,
//                RarityType.Epic => 80f,
//                RarityType.Legendary => 150f,
//                RarityType.Mythic => 400f,
//                RarityType.Godly => 1100f,
//                RarityType.Secret => 2000f,
//                _ => 50f
//            };
//        }

//        private int GetGrowTimeSeconds(RarityType rarity, int baseDamage)
//        {
//            int baseTime = GetBaseGrowTimeSeconds(rarity);
//            float avgDamage = GetAvgDamageByRarity(rarity);

//            float time = baseTime * (baseDamage / avgDamage);

//            // Hard clamp per rarity so Rare NEVER exceeds 40s
//            float min = baseTime * 0.4f;
//            float max = baseTime * 1.6f;

//            return Mathf.RoundToInt(Mathf.Clamp(time, min, max));
//        }
        
//        private int ExtractNumber(string text)
//        {
//            var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
//            return match.Success ? int.Parse(match.Value) : -1;
//        }

//    }
    
//    [System.Serializable]
//    public class PlantDataJson
//    {
//        public int id;
//        public string name;
//        public string rarity;
//        public int cost;
//        public int sell;
//        public int damage;
//        public float attack_speed;
//    }

//    [System.Serializable]
//    public class PlantDataJsonList
//    {
//        public PlantDataJson[] items;
//    }
//}
//#endif

