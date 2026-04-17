using System.Collections.Generic;

public class EffectData
{
    public string Name;
    public EEffectID ID;
    public List<ParamFormat> Value;
    public int Times;
}

public enum EEffectID
{
    HP_UP = 1,
    MP_UP,
    ATK_UP,
    MATK_UP,
    DEF_UP,
    MDEF_UP,
    ACC_UP,
    EVA_UP,
    CRIT_UP,
    CRIT_Damage_UP,
    SPD_UP,
    HP_Regen,
    MP_Regen,
    Reflect,
    Invincible,
    Shield,
    Provoke,
    Invisible,
    HP_Leech,
    MP_Leech,
    Counter,
    Berserk,
    Revive,
    HP_Down,
    MP_Down,
    ATK_Down,
    MATK_Down,
    DEF_Down,
    MDEF_Down,
    ACC_Down,
    EVA_Down,
    CRIT_Down,
    CRIT_Damage_Down,
    SPD_Down,
    HP_Drain,
    MP_Drain,
    Vulnerable,
    Death_Pending,
    Paralysis,
    Stun,
    Silence,
    Confusion,
    Exhausted,
    Poisoning,
    DEX_UP,
}