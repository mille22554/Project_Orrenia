using SQLite;

public class PlayerSave_Other
{
    [PrimaryKey]
    public string Account { get; set; }

    public string Password { get; set; }
    public int Gold { get; set; }

    public string NowPartyLeader { get; set; }

    public int SkillPoint { get; set; }

    public int ForgeLevel { get; set; }
    public int CurrentForgeExp { get; set; }
}

public class PlayerSave_Charater
{
    [PrimaryKey]
    public string Account { get; set; }

    public string Name;
    public ECharacterRole Role;
    public int Level;
    public int CurrentExp;
    public decimal CurrentHP;
    public decimal CurrentMP;
    public decimal CurrentSTA;
    public decimal CurrentTP;
    public AbilityBase Ability;
    // public List<long> Equips;
    // public List<EffectData> Effects;
    // public List<BagItemData> BagItems;
    // public Dictionary<ESkillID, SkillData> Skills;
}

public class PlayerSave_Party
{
    [PrimaryKey]
    public string Account { get; set; }

    public int Area { get; set; }
    public int Deep { get; set; }
}