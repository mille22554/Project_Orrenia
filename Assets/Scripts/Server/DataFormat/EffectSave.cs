using SQLite;

public class EffectSave
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public string Owner { get; set; }

    public int ID { get; set; }
    public string Value { get; set; }
    public int Times { get; set; }
}