using System.Collections.Generic;
using Unity.Netcode;

public class CharacterData : INetworkSerializable
{
    public string Name = "";
    public ECharacterRole Role;
    public int Level;
    public int CurrentExp;
    public int CurrentHP;
    public int CurrentMP;
    public int CurrentSTA;
    public int CurrentTP;
    public AbilityBase Ability = new();
    public List<long> Equips = new();
    public List<EffectData> Effects = new();
    public List<BagItemData> BagItems = new();
    public Dictionary<ESkillID, SkillData> Skills = new();

    public static CharacterData CreateDefault()
    {
        return new CharacterData
        {
            Level = 1,
            CurrentExp = 0,
            CurrentTP = 0,

            Ability = new()
            {
                STR_Point = 1,
                DEX_Point = 1,
                INT_Point = 1,
                VIT_Point = 1,
                AGI_Point = 1,
                LUK_Point = 1
            },
            Equips = new(),
            Effects = new(),
            BagItems = new(),
            Skills = new(),
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Role);
        serializer.SerializeValue(ref Level);
        serializer.SerializeValue(ref CurrentExp);
        serializer.SerializeValue(ref CurrentHP);
        serializer.SerializeValue(ref CurrentMP);
        serializer.SerializeValue(ref CurrentSTA);
        serializer.SerializeValue(ref CurrentTP);
        serializer.SerializeValue(ref Ability);

        PublicFunc.SerializeList(serializer, ref Equips);
        PublicFunc.SerializeClassList(serializer, ref Effects);
        PublicFunc.SerializeClassList(serializer, ref BagItems);
        PublicFunc.SerializeEnum_ClassDictionary(serializer, ref Skills);
    }
}

public class AbilityBase : INetworkSerializable
{
    public int STR_Point;
    public int DEX_Point;
    public int INT_Point;
    public int VIT_Point;
    public int AGI_Point;
    public int LUK_Point;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref STR_Point);
        serializer.SerializeValue(ref DEX_Point);
        serializer.SerializeValue(ref INT_Point);
        serializer.SerializeValue(ref VIT_Point);
        serializer.SerializeValue(ref AGI_Point);
        serializer.SerializeValue(ref LUK_Point);
    }
}

public class FullAbilityBase : INetworkSerializable
{
    public decimal STR;
    public decimal DEX;
    public decimal INT;
    public decimal VIT;
    public decimal AGI;
    public decimal LUK;

    public decimal HP;
    public decimal MP;
    public decimal STA;
    public decimal ATK;
    public decimal MATK;
    public decimal DEF;
    public decimal MDEF;
    public decimal ACC;
    public decimal EVA;
    public decimal CRIT;
    public decimal SPD;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref STR);
        serializer.SerializeValue(ref DEX);
        serializer.SerializeValue(ref INT);
        serializer.SerializeValue(ref VIT);
        serializer.SerializeValue(ref AGI);
        serializer.SerializeValue(ref LUK);
        serializer.SerializeValue(ref HP);
        serializer.SerializeValue(ref MP);
        serializer.SerializeValue(ref STA);
        serializer.SerializeValue(ref ATK);
        serializer.SerializeValue(ref MATK);
        serializer.SerializeValue(ref DEF);
        serializer.SerializeValue(ref MDEF);
        serializer.SerializeValue(ref ACC);
        serializer.SerializeValue(ref EVA);
        serializer.SerializeValue(ref CRIT);
        serializer.SerializeValue(ref SPD);
    }
}

public enum ECharacterRole
{
    Player,
    OtherPlayer,
    NPC,
    Mob
}