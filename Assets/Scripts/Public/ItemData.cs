using System.Collections.Generic;
using Unity.Netcode;

public class BagItemData : ItemData
{
    public long UID;
    public int ItemID => ID;
    public EQuality Quality = EQuality.Common;
    public List<int> Materials = new();
    public int Seed;

    public new void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        base.NetworkSerialize(serializer);

        var materials = Materials.ToArray();

        serializer.SerializeValue(ref UID);
        serializer.SerializeValue(ref Quality);
        serializer.SerializeValue(ref materials);
        serializer.SerializeValue(ref Seed);
    }
}

public class ItemData : INetworkSerializable
{
    public int ID;
    public string Name = "";
    public EItemKind Kind;
    public string Description = "";
    public FullAbilityBase Ability = new();
    public List<EffectData> Effects = new();
    public ESkillID Skill;
    public Trait Trait = new();
    public int Price;
    public int Durability;
    public int Count;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var effects = Effects.ToArray();

        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Kind);
        serializer.SerializeValue(ref Description);
        serializer.SerializeValue(ref Ability);
        serializer.SerializeValue(ref effects);
        serializer.SerializeValue(ref Skill);
        serializer.SerializeValue(ref Trait);
        serializer.SerializeValue(ref Price);
        serializer.SerializeValue(ref Durability);
        serializer.SerializeValue(ref Count);
    }
}

public class ItemKind : INetworkSerializable
{
    public string Name = "";
    public EItemCategory Category;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Category);
    }
}

public class QualityData : INetworkSerializable
{
    public string Name = null;
    public decimal Multi;
    public string Color = null;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Multi);
        serializer.SerializeValue(ref Color);
    }
}

public class Trait : INetworkSerializable
{
    public int Poisoning;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Poisoning);
    }
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