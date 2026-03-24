using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ItemData
{
    public long UID;
    public int ItemID;
    public int Durability;
    public int Count;
}

public class ItemBaseData
{
    public int ID;
    public string Name;
    public string Type;
    public string Description;
    public FullAbilityBase Ability;
    public int Price;
    public int Durability;
    public int Count;

    static readonly Dictionary<int, ItemBaseData> _items = new();

    public ItemBaseData()
    {
        Ability = new();
    }

    public string GetAbilityString()
    {
        if (Ability == null)
            return "";

        var parts = new List<string>();

        // 用反射抓 AbilityBase 的欄位
        foreach (var field in typeof(FullAbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = (int)field.GetValue(Ability);
            if (value != 0)
            {
                var sign = value > 0 ? "+" : "";
                parts.Add($"{field.Name}{sign}{value}");
            }
        }

        return string.Join(", ", parts);
    }

    public static void BuildDatabase()
    {
        var root = typeof(GameItemData);

        // 取得所有 nested types (Equip, Armor...)
        var nestedTypes = root.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var type in nestedTypes)
        {
            ScanType(type);
        }
    }

    static void ScanType(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(ItemBaseData))
            {
                var item = (ItemBaseData)field.GetValue(null);

                if (item == null)
                    continue;

                if (_items.ContainsKey(item.ID))
                {
                    throw new Exception($"Duplicate item id: {item.ID}");
                }

                _items[item.ID] = item;
            }
        }
    }

    public static ItemBaseData Get(int id)
    {
        _items.TryGetValue(id, out var item);
        return item;
    }
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