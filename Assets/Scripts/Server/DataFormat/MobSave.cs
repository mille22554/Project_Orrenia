using SQLite;

public class MobSave : IDBTable
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public long PartyUID { get; set; }
    public int ID { get; set; }

    public static MobSave Create(MobData data)
    {
        var saveData = new MobSave
        {
            PartyUID = data.PartyUID,
            ID = data.ID,
        };

        return saveData;
    }

    public static MobData GetData(MobSave save)
    {
        var data = new MobData
        {
            UID = save.UID,
            PartyUID = save.PartyUID,
            ID = save.ID,
        };

        return data;
    }
}