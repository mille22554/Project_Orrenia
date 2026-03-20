using System;
using System.Collections.Generic;
using System.Reflection;

public class GameSaveData
{
    public string version;
    public Datas Datas;

    public static GameSaveData CreateDefault()
    {
        var saveData = new GameSaveData
        {
            version = GameData_Server.version,
            Datas = Datas.CreateDefault()
        };

        return saveData;
    }
}

public class Datas
{
    public PlayerContextData PlayerData;
    public CharacterData CharacterData;
    public EnemyData EnemyData;
    public BagData BagData;

    public static Datas CreateDefault()
    {
        var datas = new Datas
        {
            PlayerData = PlayerContextData.CreateDefault(),
            CharacterData = CharacterData.CreateDefault(),
            EnemyData = new(),
            BagData = new()
        };

        return datas;
    }
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
    public FullAbilityBase ability;
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
        Type root = typeof(GameItemData);

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
    public Func<FullAbilityBase, int> damage;
    public string damageType;
    public string effect;
    public Action special;
    public string weaponType;
    public int cost;
    public int cooldown;
}