using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GameData_Server
{
    public static string PlayerSaveDataPath(string account)
    {
        var saveDataFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SaveData", "Players");
        if (!Directory.Exists(saveDataFolderPath))
            Directory.CreateDirectory(saveDataFolderPath);

        return Path.Combine(saveDataFolderPath, $"{account}_savedata.json");
    }
    public static string SaveDataBasePath()
    {
        var saveDataFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SaveData");
        if (!Directory.Exists(saveDataFolderPath))
            Directory.CreateDirectory(saveDataFolderPath);

        return Path.Combine(saveDataFolderPath, "SaveData.db");
    }

    public static string MobDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "MobData.json");
        }
    }
    public static string AreaDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "AreaData.json");
        }
    }
    public static string ItemDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "ItemData.json");
        }
    }
    public static string GameShopItemPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "GameShopItem.json");
        }
    }
    public static string ItemKindPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "ItemKind.json");
        }
    }
    public static string EffectDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "EffectData.json");
        }
    }
    public static string QualityDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "QualityData.json");
        }
    }
    public static string SkillDataPath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "SkillData.json");
        }
    }
    public static string SkillTypePath
    {
        get
        {
            var databaseFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DataBase");
            if (!Directory.Exists(databaseFolderPath))
            {
                Debug.LogError("資料庫丟失!");
            }

            return Path.Combine(databaseFolderPath, "SkillType.json");
        }
    }

    public static string version = "0.0.18";

    public const int tpCost = 10000;

    public static Dictionary<string, PlayerSaveDataFormat> NowPlayers = new();

    public static PlayerContextData GetPlayerData(string account) => NowPlayers[account].Datas.PlayerData;
    public static CharacterData GetCharacterData(string account) => NowPlayers[account].Datas.CharacterData;
    public static PartyData GetPartyData(string account) => NowPlayers[account].Datas.PartyData;
}