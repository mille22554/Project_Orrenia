public class SkillData
{
    public string Name;
    public ESkillID ID;
    public string Description;
    public EDamageType DamageType;
    public EEffectID Effect;
    public int WeaponType;
    public int Cost;
    public int CoolDown;
}

public enum EDamageType
{
    Physics = 1,
    Magic
}

public enum ESkillID
{
    爪擊 = 1,
}