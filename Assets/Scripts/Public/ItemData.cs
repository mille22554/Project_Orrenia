using System.Collections.Generic;
using System.Reflection;

public class BagItemData : ItemData
{
    public long UID;
    public int ItemID => ID;
    public EQuality Quality = EQuality.Common;
    public List<int> Materials = new();
    public int Seed;
}

public class ItemData
{
    public int ID;
    public string Name;
    public EItemKind Kind;
    public string Description;
    public FullAbilityBase Ability;
    public List<EffectData> Effects;
    public int Price;
    public int Durability;
    public int Count;
}

public class ItemKind
{
    public string Name;
    public EItemCategory Category;
}

public class QualityData
{
    public string Name;
    public decimal Multi;
    public string Color;
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

public enum EQuality
{
    Legendary,
    Epic,
    Rare,
    Uncommon,
    Special,
    Common,
    Worn,
    Old,
    Decay,
    Broken,
    Junk
}