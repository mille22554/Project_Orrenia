using SQLite;

public class CharaterSave
{
    [PrimaryKey]
    public long UID { get; set; }

    public string Name { get; set; }
    public ECharacterRole Role { get; set; }
    public int Level { get; set; }
    public int CurrentExp { get; set; }
    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }
    public int CurrentSTA { get; set; }
    public string CurrentTP { get; set; }

    public static CharaterSave Create(long UID, CharacterData data)
    {
        var saveData = new CharaterSave
        {
            UID = UID,
            Name = data.Name,
            Role = data.Role,
            Level = data.Level,
            CurrentExp = data.CurrentExp,
            CurrentHP = data.CurrentHP,
            CurrentMP = data.CurrentMP,
            CurrentSTA = data.CurrentSTA,
            CurrentTP = data.CurrentTP.ToString(),
        };

        return saveData;
    }

    public static CharacterData GetData(CharaterSave save)
    {
        var data = new CharacterData
        {
            Name = save.Name,
            Role = save.Role,
            Level = save.Level,
            CurrentExp = save.CurrentExp,
            CurrentHP = save.CurrentHP,
            CurrentMP = save.CurrentMP,
            CurrentSTA = save.CurrentSTA,
            CurrentTP = decimal.Parse(save.CurrentTP),
        };

        return data;
    }
}

public class CharaterAbilitySave
{
    [PrimaryKey]
    public long UID { get; set; }

    public int STR_Point { get; set; }
    public int DEX_Point { get; set; }
    public int INT_Point { get; set; }
    public int VIT_Point { get; set; }
    public int AGI_Point { get; set; }
    public int LUK_Point { get; set; }

    public static CharaterAbilitySave Create(long UID, AbilityBase data)
    {
        var saveData = new CharaterAbilitySave
        {
            UID = UID,
            STR_Point = data.STR_Point,
            DEX_Point = data.DEX_Point,
            INT_Point = data.INT_Point,
            VIT_Point = data.VIT_Point,
            AGI_Point = data.AGI_Point,
            LUK_Point = data.LUK_Point,
        };

        return saveData;
    }

    public static AbilityBase GetData(CharaterAbilitySave save)
    {
        var data = new AbilityBase
        {
            STR_Point = save.STR_Point,
            DEX_Point = save.DEX_Point,
            INT_Point = save.INT_Point,
            VIT_Point = save.VIT_Point,
            AGI_Point = save.AGI_Point,
            LUK_Point = save.LUK_Point,
        };

        return data;
    }
}