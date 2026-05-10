using SQLite;

public class PlayerSave : IDBTable
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed(Unique = true)]
    public string Account { get; set; }
    [Indexed]
    public long PartyUID { get; set; }

    public string Password { get; set; }
    public int Gold { get; set; }


    public int SkillPoint { get; set; }

    public int ForgeLevel { get; set; }
    public int CurrentForgeExp { get; set; }

    public static PlayerSave Create(PlayerData data)
    {
        var saveData = new PlayerSave
        {
            UID = data.UID,
            Account = data.Account,
            Password = data.Password,
            Gold = data.Gold,
            PartyUID = data.PartyUID,
            SkillPoint = data.SkillPoint,
            ForgeLevel = data.ForgeLevel,
            CurrentForgeExp = data.CurrentForgeExp,
        };

        return saveData;
    }

    public static PlayerData GetData(PlayerSave save)
    {
        var data = new PlayerData
        {
            UID = save.UID,
            Account = save.Account,
            Password = save.Password,
            Gold = save.Gold,
            PartyUID = save.PartyUID,
            SkillPoint = save.SkillPoint,
            ForgeLevel = save.ForgeLevel,
            CurrentForgeExp = save.CurrentForgeExp,
        };

        return data;
    }
}