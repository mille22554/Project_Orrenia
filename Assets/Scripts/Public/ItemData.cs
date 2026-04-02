using System.Collections.Generic;
using System.Reflection;

public class BagItemData : ItemData
{
    public long UID;
    public int ItemID => ID;
}

public class ItemData
{
    public int ID;
    public string Name;
    public EItemKind Kind;
    public string Description;
    public FullAbilityBase Ability;
    public int Price;
    public int Durability;
    public int Count;

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
}

public class ItemKind
{
    public string Name;
    public EItemCategory Category;
}

public enum EItemCategory
{
    One_Hand,
    Two_Hand,
    Shield,
    Helmet,
    Armor,
    Greaves,
    Shoes,
    Gloves,
    Cape,
    Ring,
    Pendant,
    Use,
    Material
}

public enum EItemKind
{
    None = -1,
    Sword,
    Hammer,
    Spear,
    Staff,
    Rapier,
    Dagger,
    Axe,
    Aegis,
    Bow,
    Book,
    Katana,
    Tarot,
    Shield,
    Helmet,
    Armor,
    Greaves,
    Shoes,
    Gloves,
    Cape,
    Ring,
    Pendant,
    Use,
    Material,
}