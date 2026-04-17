using System.Collections.Generic;

public class SkillData
{
    public string Name;
    public ESkillID ID;
    public string Description;
    public ESkillType SkillType;
    public List<ParamFormat> Damage = new();
    public List<EffectData> Buffs = new();
    public List<DeBuffFormat> DeBuffs = new();
    public List<EItemKind> WeaponType = new();
    public int Cost;
    public int CoolDown;
    public int CurrentCD;
}

public enum ESkillType
{
    None,
    SinglePhysicsAttack,
    SingleMagicAttack,
    Passive,
    SingleBuff,
}

public enum ESkillID
{
    None,
    雙持 = 4,
    響尾 = 8,
}

public class DeBuffFormat
{
    public EffectData Effect;
    public int Prop;
}