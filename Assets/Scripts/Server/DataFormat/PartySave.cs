using SQLite;

public class PartySave
{
    [PrimaryKey]
    public long UID { get; set; }

    public int Area { get; set; }
    public int Deep { get; set; }

    public static PartySave Create(long UID, PartyData data)
    {
        var saveData = new PartySave
        {
            UID = UID,
            Area = data.Area,
            Deep = data.Deep,
        };

        return saveData;
    }

    public static PartyData GetData(PartySave save)
    {
        var data = new PartyData
        {
            Area = save.Area,
            Deep = save.Deep,
        };

        return data;
    }
}