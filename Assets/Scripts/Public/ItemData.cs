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
    public ESkillID Skill;
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
    None,
    Sword,
    Hammer,
    Spear,
    Book,
    Rapier,
    Dagger,
    Axe,
    Aegis,
    Bow,
    Staff,
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