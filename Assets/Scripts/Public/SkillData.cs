using System.Collections.Generic;

public class SkillData
{
    public string Name;
    public ESkillID ID;
    public string Description;
    public ESkillType SkillType;
    public List<DamageFormat> Damage = new();
    public EEffectID Effect;
    public List<EItemKind> WeaponType = new();
    public int Cost;
    public int CoolDown;
    public int CurrentCD;
}

public class DamageFormat
{
    public decimal Constant;
    public FullAbilityBase Ability;
}

public enum ESkillType
{
    None,
    PhysicsAttack,
    MagicAttack,
    Passive
}

public enum ESkillID
{
    None,
    DualWield = 4,
}