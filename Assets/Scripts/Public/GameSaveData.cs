using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

public class GameSaveData
{
    public string version;
    public Datas datas;

    public static GameSaveData CreateDefault()
    {
        var saveData = new GameSaveData
        {
            version = GameData.version,
            datas = Datas.CreateDefault()
        };

        return saveData;
    }
}

public class Datas
{
    public PlayerData playerData;
    public EnemyData enemyData;
    public BagData bagData;

    public static Datas CreateDefault()
    {
        var datas = new Datas
        {
            playerData = PlayerData.CreateDefault(),
            enemyData = new(),
            bagData = new()
        };

        return datas;
    }
}

public class PlayerData
{
    public string PlayerName;
    public int Level;
    public int CurrentExp;
    public int MaxExp;
    public int CurrentHp;
    public int CurrentMp;
    public int CurrentSTA;
    public int CurrentTp;

    public string Area;
    public int Deep;
    public int Gold;

    public int AbilityPoint;
    public AbilityBase ability;

    public EquipBase equips;

    public List<EffectData> effects;
    [JsonIgnore]
    public List<Action<bool>> effectActions = new();

    public int skillPoint;

    public int forgeLevel;
    public int currentForgeExp;
    public int maxForgeExp;

    public bool isGetBasicDagger;

    public static PlayerData CreateDefault()
    {
        var playerData = new PlayerData
        {
            Level = 1,
            CurrentExp = 0,
            MaxExp = 100,
            CurrentTp = 0,

            Area = GameArea.Home,
            Deep = 0,
            Gold = 0,

            AbilityPoint = 0,
            ability = new()
            {
                STR_Point = 1,
                DEX_Point = 1,
                INT_Point = 1,
                VIT_Point = 1,
                AGI_Point = 1,
                LUK_Point = 1
            },
            equips = new(),
            effects = new(),
            skillPoint = 0,

            forgeLevel = 1,
            currentForgeExp = 0,
            maxForgeExp = 100,

            isGetBasicDagger = false
        };

        return playerData;
    }
}

public class AbilityBase
{
    public int STR_Point;
    public int DEX_Point;
    public int INT_Point;
    public int VIT_Point;
    public int AGI_Point;
    public int LUK_Point;

    public int STR;
    public int DEX;
    public int INT;
    public int VIT;
    public int AGI;
    public int LUK;

    public int HP;
    public int MP;
    public int STA;
    public int ATK;
    public int MATK;
    public int DEF;
    public int MDEF;
    public int ACC;
    public int EVA;
    public int CRIT;
    public int SPD;
}

public class EquipBase
{
    public long Right_Hand;
    public long Left_Hand;
    public long Helmet;
    public long Armor;
    public long Greaves;
    public long Shoes;
    public long Gloves;
    public long Cape;
    public long Ring;
    public long Pendant;
}

public class EnemyData
{
    public List<MobData> enemies;

    public EnemyData()
    {
        enemies = new();
    }
}

public class MobData
{
    public string name;
    public int level;
    public int currentHp;
    public int currentMp;
    public int currentTp;

    public AbilityBase ability;

    public List<DropItem> dropItems;

    public List<EffectData> effects;

    public MobData()
    {
        ability = new();
        dropItems = new();
        effects = new();
    }
}

public class DropItem
{
    public ItemBaseData item;
    public int prop;
}

public class BagData
{
    public List<ItemData> items;

    public BagData()
    {
        items = new();
    }
}

public class ItemBaseData
{
    public int id;
    public string name;
    public string type;
    public string description;
    public AbilityBase ability;
    public int price;
    public int durability;
    public int count;

    public ItemBaseData()
    {
        ability = new();
    }

    public string GetAbilityString()
    {
        if (ability == null)
            return "";

        List<string> parts = new();

        // 用反射抓 AbilityBase 的欄位
        foreach (var field in typeof(AbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            int value = (int)field.GetValue(ability);
            if (value != 0)
            {
                string sign = value > 0 ? "+" : "";
                parts.Add($"{field.Name}{sign}{value}");
            }
        }

        return string.Join(", ", parts);
    }
    static readonly Dictionary<int, ItemBaseData> items = new();

    public static void BuildDatabase()
    {
        Type root = typeof(GameItem);

        // 取得所有 nested types (Equip, Armor...)
        var nestedTypes = root.GetNestedTypes(
            BindingFlags.Public | BindingFlags.NonPublic
        );

        foreach (var type in nestedTypes)
        {
            ScanType(type);
        }
    }

    static void ScanType(Type type)
    {
        var fields = type.GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static
        );

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(ItemBaseData))
            {
                var item = (ItemBaseData)field.GetValue(null);

                if (item == null)
                    continue;

                if (items.ContainsKey(item.id))
                {
                    throw new Exception($"Duplicate item id: {item.id}");
                }

                items[item.id] = item;
            }
        }
    }

    public static ItemBaseData Get(int id)
    {
        items.TryGetValue(id, out var item);
        return item;
    }
}

public class ItemData
{
    public long uid;
    public int itemID;
    public int durability;
    public int count;
}

public class EffectData
{
    public string type;
    public int value;
    public int times;
}

public class SkillData
{
    public string name;
    public string description;
    public Func<AbilityBase, int> damage;
    public string damageType;
    public string effect;
    public Action special;
    public string weaponType;
    public int cost;
    public int cooldown;
}