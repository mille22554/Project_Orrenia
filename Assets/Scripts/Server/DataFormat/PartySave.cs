using SQLite;

public class PartySave : IDBTable
{
    [PrimaryKey]
    public long UID { get; set; }

    public int Area { get; set; }
    public int Deep { get; set; }

    public static PartySave Create(PartyData data)
    {
        var saveData = new PartySave
        {
            UID = data.UID,
            Area = data.Area,
            Deep = data.Deep,
        };

        return saveData;
    }

    public static PartyData GetData(PartySave save)
    {
        var data = new PartyData
        {
            UID = save.UID,
            Area = save.Area,
            Deep = save.Deep,
        };

        return data;
    }
}