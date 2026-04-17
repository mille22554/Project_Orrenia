using System.Collections.Generic;

public class CharacterData
{
    public string Name;
    public ECharacterRole Role;
    public int Level;
    public int CurrentExp;
    public decimal CurrentHP;
    public decimal CurrentMP;
    public decimal CurrentSTA;
    public decimal CurrentTP;
    public AbilityBase Ability;
    public List<long> Equips;
    public List<EffectData> Effects;
    public List<BagItemData> BagItems;
    public Dictionary<ESkillID, SkillData> Skills;

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
}

public enum ECharacterRole
{
    Player,
    OtherPlayer,
    NPC,
    Mob
}