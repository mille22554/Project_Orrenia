using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static GameItemData;


public static class GameData
{
    public static Dictionary<int, AreaData> AreaData;
}

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

    public static string version = "0.0.16";

    public const int tpCost = 10000;

    public static GameSaveData SaveData;

    public static PlayerContextData NowPlayerData => SaveData.Datas.PlayerData;
    public static CharacterData NowCharacterData => SaveData.Datas.CharacterData;
    public static EnemyData NowEnemyData => SaveData.Datas.EnemyData;
    public static BagData NowBagData => SaveData.Datas.BagData;
}

public static class EquipType
{
    public static class One_Hand_Weapon
    {
        public const string Sword = "單手劍";
        public const string Hammer = "單手錘";
        public const string Spear = "長槍";
        public const string Staff = "法杖";
        public const string Rapier = "刺劍";
        public const string Dagger = "短刀";
    }
    public static class Two_Hand_Weapon
    {
        public const string Axe = "雙手斧";
        public const string Aegis = "塔盾";
        public const string Bow = "弓";
        public const string Book = "魔導書";
        public const string Katana = "武士刀";
        public const string Tarot = "塔羅牌";
    }
    public const string Shield = "盾牌";
    public const string Helmet = "頭盔";
    public const string Armor = "護胸";
    public const string Greaves = "護腿";
    public const string Shoes = "鞋子";
    public const string Gloves = "手套";
    public const string Cape = "披風";
    public const string Ring = "戒指";
    public const string Pendant = "項鍊";
}

public static class UseType
{
    public const string Use = "消耗品";
}

public static class MaterialType
{
    public const string Material = "素材";
}

public static class ItemTypeCheck
{
    private static readonly HashSet<string> allEquipTypes;
    private static readonly HashSet<string> allUseTypes;
    private static readonly HashSet<string> allMaterialTypes;

    static ItemTypeCheck()
    {
        allEquipTypes = GetAllTypes(typeof(EquipType));
        allUseTypes = GetAllTypes(typeof(UseType));
        allMaterialTypes = GetAllTypes(typeof(MaterialType));
    }

    public static HashSet<string> GetAllTypes(Type type)
    {
        var result = new HashSet<string>();

        // 取得這個類別內所有 const string 欄位
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string));

        foreach (var field in fields)
        {
            var value = field.GetValue(null)?.ToString();
            if (value != null)
                result.Add(value);
        }

        // 🔁 遞迴子類別
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
        {
            foreach (var sub in GetAllTypes(nestedType))
                result.Add(sub);
        }

        return result;
    }

    public static bool IsEquipType(string type)
    {
        return allEquipTypes.Contains(type);
    }

    public static bool IsUseType(string type)
    {
        return allUseTypes.Contains(type);
    }

    public static bool IsMaterialType(string type)
    {
        return allMaterialTypes.Contains(type);
    }
}

public static class GameShopItem
{
    public static List<ItemBaseData> list = new()
    {
        Equip.WoodenSword,
        Equip.WoodenShield,
        Equip.WoodenHelmet,
        Equip.ClothArmor,
        Equip.ClothGreaves,
        Equip.ClothBoots,
        Use.SmallHpPotion,
        Use.SmallMpPotion,
        Use.SmallSTAPotion,
        Use.BerserkPotion,
    };
}

public static class GameSkill
{
    public static SkillData bite = new()
    {
        name = "爪擊",
        description = "使用銳利的爪子攻擊目標",
        damage = ability => (int)(ability.ATK * 0.8 + 0.2 * (ability.ATK + ability.STR * 2 + ability.AGI)),
        damageType = DamageType.physics,
        cost = 3,
    };
}

public static class DamageType
{
    public const string physics = "物理傷害";
    public const string magic = "魔法傷害";
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