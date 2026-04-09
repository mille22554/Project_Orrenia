using System.IO;
using UnityEngine;

public static class GameData_Server
{
    public static string SaveDataPath
    {
        get
        {
            var saveDataFolderPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SaveData");
            if (!Directory.Exists(saveDataFolderPath))
                Directory.CreateDirectory(saveDataFolderPath);

            return Path.Combine(saveDataFolderPath, "savedata.json");
        }
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

    public static string version = "0.0.16";

    public const int tpCost = 10000;

    public static SaveDataFormat SaveData;

    public static PlayerContextData NowPlayerData => SaveData.Datas.PlayerData;
    public static CharacterData NowCharacterData => SaveData.Datas.CharacterData;
    public static EnemyData NowEnemyData => SaveData.Datas.EnemyData;
    public static BagData NowBagData => SaveData.Datas.BagData;
}

public static class GameSkill
{
    public static SkillData bite = new()
    {
        Name = "爪擊",
        Description = "使用銳利的爪子攻擊目標",
        // damage = ability => (int)(ability.ATK * 0.8 + 0.2 * (ability.ATK + ability.STR * 2 + ability.AGI)),
        DamageType = EDamageType.Physics,
        Cost = 3,
    };
}

public static class EffectType
{
    public static class Buff
    {
        public const string HP_UP = "最大生命增加";
        public const string MP_UP = "最大魔力增加";
        public const string ATK_UP = "物理傷害增加";
        public const string MATK_UP = "魔法傷害增加";
        public const string DEF_UP = "物理防禦增加";
        public const string MDEF_UP = "魔法防禦增加";
        public const string ACC_UP = "命中增加";
        public const string EVA_UP = "迴避增加";
        public const string CRIT_UP = "爆擊率增加";
        public const string CRIT_Damage_UP = "爆擊傷害增加";
        public const string SPD_UP = "速度增加";
        public const string HP_Regen = "生命回復";
        public const string MP_Regen = "魔力回復";
        public const string Reflect = "反盾";
        public const string Invincible = "無敵";
        public const string Shield = "護盾";
        public const string Provoke = "嘲諷";
        public const string Invisible = "潛行";
        public const string HP_Leech = "生命吸取";
        public const string MP_Leech = "魔力吸取";
        public const string Counter = "反擊";
        public const string Berserk = "狂暴";
        public const string Revive = "復活";
    }

    public static class Debuff
    {
        public const string HP_Down = "最大生命減少";
        public const string MP_Down = "最大魔力減少";
        public const string ATK_Down = "物理傷害減少";
        public const string MATK_Down = "魔法傷害減少";
        public const string DEF_Down = "物理防禦減少";
        public const string MDEF_Down = "魔法防禦減少";
        public const string ACC_Down = "命中減少";
        public const string EVA_Down = "迴避減少";
        public const string CRIT_Down = "爆擊率減少";
        public const string CRIT_Damage_Down = "爆擊傷害減少";
        public const string SPD_Down = "速度減少";
        public const string HP_Drain = "生命流失";
        public const string MP_Drain = "魔力流失";
        public const string Vulnerable = "易傷";
        public const string Death_Pending = "即死";
        public const string Paralysis = "麻痺";
        public const string Stun = "暈眩";
        public const string Silence = "沉默";
        public const string Confusion = "混亂";
        public const string Exhausted = "筋疲力竭";
    }
}