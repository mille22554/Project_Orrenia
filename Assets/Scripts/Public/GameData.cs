using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class GameData
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

    public static string version = "0.0.16";

    public const int tpCost = 10000;

    public static GameSaveData gameData;

    public static PlayerData NowPlayerData => gameData.datas.playerData;
    public static EnemyData NowEnemyData => gameData.datas.enemyData;
    public static BagData NowBagData => gameData.datas.bagData;
}

public static class GameArea
{
    public const string Home = "迷宮都市 － 奧雷尼亞";
    public const string Floor1 = "第一層 露米亞草原";
}

public static class GameEnemy
{
    public static class Floor1
    {
        public const string mob1 = "史萊姆";
        public const string mob2 = "小白兔";
        public const string mob3 = "麻雀";
    }
}

public static class GameItem
{
    public static class Equip
    {
        public static ItemBaseData BasicDagger = new()
        {
            name = "初始短刀",
            id = 1,
            type = EquipType.One_Hand_Weapon.Dagger,
            description = "新手冒險者的標配，感覺拿著就能受到一點保佑。",
            price = 500,
            ability = new()
            {
                ATK = 10,
                LUK = 10
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenSword = new()
        {
            name = "木製長劍",
            id = 2,
            type = EquipType.One_Hand_Weapon.Sword,
            description = "訓練用的木劍，比空手好一點。",
            price = 500,
            ability = new()
            {
                ATK = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenShield = new()
        {
            name = "木盾",
            id = 3,
            type = EquipType.Shield,
            description = "訓練用的木盾，盾反甚麼的別想了。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData WoodenHelmet = new()
        {
            name = "木製頭盔",
            id = 4,
            type = EquipType.Helmet,
            description = "訓練用的木盔，戴起來沒有很舒服。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothArmor = new()
        {
            name = "布製護甲",
            id = 5,
            type = EquipType.Armor,
            description = "訓練用的布甲，木劍程度的傷害還是可以防住的。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothGreaves = new()
        {
            name = "布製護腿",
            id = 6,
            type = EquipType.Greaves,
            description = "訓練用的護腿，不用怕打到小腿。",
            price = 500,
            ability = new()
            {
                DEF = 1,
            },
            durability = 500,
            count = 1
        };

        public static ItemBaseData ClothBoots = new()
        {
            name = "布靴",
            id = 7,
            type = EquipType.Shoes,
            description = "訓練用的布靴，比赤腳好一點。",
            price = 500,
            ability = new()
            {
                SPD = 1,
            },
            durability = 500,
            count = 1
        };
    }

    public static class Use
    {
        public static ItemBaseData SmallHpPotion = new()
        {
            name = "小型生命藥水",
            id = 101,
            type = UseType.Use,
            description = "恢復少量生命。",
            price = 50,
            ability = new()
            {
                HP = 50
            },
            count = 1
        };

        public static ItemBaseData SmallMpPotion = new()
        {
            name = "小型魔力藥水",
            id = 102,
            type = UseType.Use,
            description = "恢復少量魔力。",
            price = 30,
            ability = new()
            {
                MP = 15
            },
            count = 1
        };

        public static ItemBaseData SmallSTAPotion = new()
        {
            name = "小型體力藥水",
            id = 103,
            type = UseType.Use,
            description = "恢復少量體力。",
            price = 50,
            ability = new()
            {
                STA = 10
            },
            count = 1
        };

        public static ItemBaseData BerserkPotion = new()
        {
            name = "狂暴藥水",
            id = 104,
            type = UseType.Use,
            description = "使全數值呈倍數上升，但會不受控。\n狂化－100回合",
            price = 1000,
            count = 1
        };
    }

    public static class Material
    {
        public static ItemBaseData SlimeGel = new()
        {
            name = "史萊姆凝膠",
            id = 201,
            type = MaterialType.Material,
            description = "史萊姆的一部分，軟軟的帶點彈力。",
            price = 10,
            ability = new()
            {
                VIT = 1
            },
            count = 1
        };

        public static ItemBaseData WhiteRabbitFur = new()
        {
            name = "白兔毛皮",
            id = 202,
            type = MaterialType.Material,
            description = "白兔的毛皮，感受到些微的庇護。",
            price = 10,
            ability = new()
            {
                LUK = 1
            },
            count = 1
        };

        public static ItemBaseData SparrowFeather = new()
        {
            name = "麻雀羽毛",
            id = 203,
            type = MaterialType.Material,
            description = "麻雀的羽毛，輕巧的可以隨風飛翔。",
            price = 10,
            ability = new()
            {
                SPD = 1
            },
            count = 1
        };
    }

    public static void CopyFields<T>(T source, T target)
    {
        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }
    }
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
        GameItem.Equip.WoodenSword,
        GameItem.Equip.WoodenShield,
        GameItem.Equip.WoodenHelmet,
        GameItem.Equip.ClothArmor,
        GameItem.Equip.ClothGreaves,
        GameItem.Equip.ClothBoots,
        GameItem.Use.SmallHpPotion,
        GameItem.Use.SmallMpPotion,
        GameItem.Use.SmallSTAPotion,
        GameItem.Use.BerserkPotion,
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