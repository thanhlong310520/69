#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Nami.EditorTool
{

    public class BuildGameEditor : EditorWindow
    {
        const string tooltip_Platform = "Choose platform to build game";
        Platform_Build platform;

        string keyAliasName = "rc48";
        string passKey = "raccoon123";

        [MenuItem("Raccoon/BuildGame", priority = 1)]
        static void Init()
        {
            BuildGameEditor window = (BuildGameEditor)GetWindow(typeof(BuildGameEditor), false, "Init Build");
            window.Show();

        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            platform = (Platform_Build)EditorGUILayout.EnumPopup(new GUIContent("Platform build", tooltip_Platform), platform);


            if (platform == Platform_Build.Android)
            {
                // Add new GameObject field
                if (GUILayout.Button("Set Pass Keystore"))
                {
                    OnClickSetPassKey();
                }
            }
            else
            {
                // Add new GameObject field
                if (GUILayout.Button("Do no thing"))
                {

                }
            }
            GUILayout.EndVertical();

        }

        private void OnClickSetPassKey()
        {
            PlayerSettings.Android.keystorePass = passKey;
            PlayerSettings.Android.keyaliasName = keyAliasName;
            PlayerSettings.Android.keyaliasPass = passKey;
            Debug.Log("Set key success");
        }

        public List<T> LoadAllSO<T>(string folderPath) where T : ScriptableObject
        {
            List<T> assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            return assets;
        }

        public enum Platform_Build
        {
            None = 0,
            Android,
            IOS,
        }

    }
}
#endif