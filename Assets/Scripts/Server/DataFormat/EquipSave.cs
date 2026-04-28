using SQLite;

public class EquipSave
{
    [PrimaryKey]
    public long UID { get; set; }

    [Indexed]
    public string Owner { get; set; }
}