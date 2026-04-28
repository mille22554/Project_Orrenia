using SQLite;

public class MobSave
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public long PartyUID { get; set; }
    public int ID { get; set; }
}