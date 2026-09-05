using MessagePack;
using Raccoon.Data;
using Raccoon.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Raccoon
{
    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class PlayerData
    {
        private static string file_loaded = "rc69";
        private static PlayerData player_data = null;
        public const string extension = ".raccoon";
        public bool onSound;
        public bool onMusic;
        public bool isOnAds = true;
        public string filename;
        public string version;
        public List<CharactorData> charactorDatas;
        public List<string> actionTutDone;

        public bool isVipIAP;

        public PlayerData() { }

        public PlayerData(string name)
        {
            filename = name;
            version = Application.version;
            onMusic = true;
            onSound = true;
            isOnAds = true;
            isVipIAP = false;
        }
        public List<CharactorData> GetCharactorDatas() => charactorDatas;

        public void FixData()
        {
            charactorDatas ??= new List<CharactorData>();
            actionTutDone ??= new List<string>();
        }

        #region SAVE / LOAD

        public static PlayerData Get
        {
            get { return player_data; }
        }

        public bool IsVersionValid()
        {
            return version == Application.version;
        }

        public void Save()
        {
            Debug.Log("Save Game");
            Save(file_loaded, this);
        }

        public static void Save(string filename, PlayerData data)
        {
            if (!string.IsNullOrEmpty(filename) && data != null)
            {
                data.filename = filename;
                data.version = Application.version;
                player_data = data;
                file_loaded = filename;

                SaveTool.SaveFile<PlayerData>(filename + extension, data);
                SetLastSave(filename);
            }
        }

        public static void NewGame()
        {
            NewGame(GetLastSave()); //default name
        }

        //You should reload the scene right after NewGame
        public static PlayerData NewGame(string filename)
        {
            file_loaded = filename;
            player_data = new PlayerData(filename);
            player_data.FixData();
            return player_data;
        }

        public static PlayerData Load(string filename)
        {
            if (player_data == null || file_loaded != filename)
            {
                player_data = SaveTool.LoadFile<PlayerData>(filename + extension);
                if (player_data != null)
                {
                    file_loaded = filename;
                    player_data.FixData();
                }
            }
            return player_data;
        }

        public static PlayerData LoadLast()
        {
            return AutoLoad(GetLastSave());
        }

        //Load if found, otherwise new game
        public static PlayerData AutoLoad(string filename)
        {
            if (player_data == null)
                player_data = Load(filename);
            if (player_data == null)
                player_data = NewGame(filename);

            Debug.Log("LoadData");
            return player_data;
        }

        public static void SetLastSave(string filename)
        {
            if (SaveTool.IsValidFilename(filename))
            {
                PlayerPrefs.SetString(PlayerPrefsKey.LAST_SAVE, filename);
            }
        }

        public static string GetLastSave()
        {
            string name = PlayerPrefs.GetString(PlayerPrefsKey.LAST_SAVE, "");
            if (string.IsNullOrEmpty(name))
                name = "rc69"; //Default name
            return name;
        }

        public static bool HasLastSave()
        {
            return HasSave(GetLastSave());
        }

        public static bool HasSave(string filename)
        {
            return SaveTool.DoesFileExist(filename + extension);
        }

        public static void Unload()
        {
            player_data = null;
            file_loaded = "";
        }

        public static void Delete(string filename)
        {
            if (file_loaded == filename)
            {
                player_data = new PlayerData(filename);
                player_data.FixData();
            }

            SaveTool.DeleteFile(filename + extension);
        }

        public static bool IsLoaded()
        {
            return player_data != null && !string.IsNullOrEmpty(file_loaded);
        }

        #endregion

        #region USE_DATA

        public void OnVip()
        {
            isVipIAP = true;
        }
        public void RemoveVip()
        {
            isVipIAP = false;
        }
        public void RemoveAds()
        {
            isOnAds = false;
        }
        public void OnAds()
        {
            isOnAds = true;
        }
        public CharactorData GetCharacterData(string id)
        {
            CharactorData result = Get.GetCharactorDatas().FirstOrDefault(c => c.id == id);
            if (result == null)
            {
                result = new CharactorData(id, id);
                Get.charactorDatas.Add(result);
            }
            return result;
        }

        public bool HaveActionTutDone(string action)
        {
            return actionTutDone.Contains(action);
        }
        public void AddActionTutDone(string action)
        {
            if(!actionTutDone.Contains(action)  && !string.IsNullOrEmpty(action))
                actionTutDone.Add(action);
        }
        #endregion

    }

    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class UserInfoData
    {
        public string user_id;
        public string user_name;
        public string avatar_name;
        public long totalExp;
        public string fashion_id;

        public UserInfoData() { }

        public UserInfoData(string id)
        {
            this.user_id = id;
        }
    }

    [MessagePackObject(keyAsPropertyName: true)]
    [System.Serializable]
    public class ItemInfo
    {
        public string id;
        public string item_id;
        public float scale_value;
        public float scale_size;
        public int quality;
        public float time_remain;
        public int item_type; 
        public static ItemInfo Null = new ItemInfo();

        public bool IsNull => string.IsNullOrEmpty(item_id);
        
        //null
        public ItemInfo()
        {
            this.item_id = string.Empty;
            this.quality = -1;
            this.time_remain = -1;
            scale_size = 1f;
            scale_value = 1f;
        }
        
        //brainrot
        public ItemInfo(string id, float scaleValue, float scaleSize)
        {
            GenerateUniqueId();
            this.item_id = id;
            this.scale_value = scaleValue;
            this.scale_size = scaleSize;
            this.quality = 0;
            this.time_remain = -1;
            this.item_type = (int)ItemTypeInfo.Brainrot;
        }
        
        //seed/plant in slot
        public ItemInfo(string id, float scaleValue, float scaleSize, float timeRemain, ItemTypeInfo type)
        {
            GenerateUniqueId();
            this.item_id = id;
            this.scale_value = scaleValue;
            this.scale_size = scaleSize;
            this.quality = 0;
            this.time_remain = timeRemain;
            this.item_type = (int)type;
        }

        //seed + extent item
        public ItemInfo(string id, int quality)
        {
            GenerateUniqueId();
            this.item_id = id;
            this.quality = quality;
            this.time_remain = -1;
            scale_size = 1f;
            scale_value = 1f;
            this.item_type = (int)ItemTypeInfo.Seed;
        }

        public int GetItemType()
        {
            if (IsNull) return (int)ItemTypeInfo.None;
            return this.item_type;
        }

        private void GenerateUniqueId()
        {
            if (!string.IsNullOrEmpty(id)) return;
            this.id = GameUtils.GenerateUniqueID();
        }

        public enum ItemTypeInfo
        {
            None = 0,
            Plant = 1,
            Seed = 2,
            Brainrot = 3,
            Other = 4
        }
    }

}