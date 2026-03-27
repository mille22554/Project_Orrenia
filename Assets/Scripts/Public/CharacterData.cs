using System.Collections.Generic;

public class CharacterData
{
    public string Name;
    public ECharacterRole Role;
    public int Level;
    public int CurrentExp;
    public int CurrentHP;
    public int CurrentMP;
    public int CurrentSTA;
    public decimal CurrentTP;
    public AbilityBase Ability;
    public List<long> Equips;
    public List<EffectData> Effects;

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
        };
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
}

public class FullAbilityBase
{
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

public enum ECharacterRole
{
    Player,
    OtherPlayer,
    NPC,
    Mob
}