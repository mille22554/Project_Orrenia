using System;
using Unity.Netcode;

public class CharacterData : INetworkSerializable
{
    public long UID;
    public string Name = "";
    public ECharacterRole Role;
    public int Level;
    public int CurrentExp;
    public int CurrentHP;
    public int CurrentMP;
    public int CurrentSTA;
    public decimal CurrentTP;

    public static CharacterData CreateDefault(long UID)
    {
        return new CharacterData
        {
            UID = UID,
            Level = 1,
            CurrentExp = 0,
            CurrentTP = 0,
        };
    }

    public static void ChangeHP(CharacterData characterData, decimal value)
    {
        characterData.CurrentHP += Math.Max(1, Math.Abs((int)value)) * Math.Sign(value);
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
    }
}

public class AbilityBase : INetworkSerializable
{
    public long UID;
    public int STR_Point;
    public int DEX_Point;
    public int INT_Point;
    public int VIT_Point;
    public int AGI_Point;
    public int LUK_Point;

    public static AbilityBase CreateDefault(long UID)
    {
        return new AbilityBase
        {
            UID = UID,
            STR_Point = 1,
            DEX_Point = 1,
            INT_Point = 1,
            VIT_Point = 1,
            AGI_Point = 1,
            LUK_Point = 1,
        };
    }

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
    public int MP;
    public int STA;
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