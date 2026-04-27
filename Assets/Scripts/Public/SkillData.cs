using System.Collections.Generic;
using Unity.Netcode;

public class SkillData : INetworkSerializable
{
    public string Name = "";
    public ESkillID ID;
    public string Description = "";
    public ESkillType SkillType;
    public List<ParamFormat> Damage = new();
    public List<EffectData> Buffs = new();
    public List<DeBuffFormat> DeBuffs = new();
    public List<EItemKind> WeaponType = new();
    public int Cost;
    public int CoolDown;
    public int CurrentCD;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var damage = Damage.ToArray();
        var buffs = Buffs.ToArray();
        var deBuffs = DeBuffs.ToArray();
        var weaponType = WeaponType.ToArray();

        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref ID);
        serializer.SerializeValue(ref Description);
        serializer.SerializeValue(ref SkillType);
        serializer.SerializeValue(ref damage);
        serializer.SerializeValue(ref buffs);
        serializer.SerializeValue(ref deBuffs);
        serializer.SerializeValue(ref weaponType);
        serializer.SerializeValue(ref Cost);
        serializer.SerializeValue(ref CoolDown);
        serializer.SerializeValue(ref CurrentCD);
    }
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

public class DeBuffFormat : INetworkSerializable
{
    public EffectData Effect;
    public int Prop;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Effect);
        serializer.SerializeValue(ref Prop);
    }
}