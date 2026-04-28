using SQLite;

public class SkillSave
{
    [PrimaryKey]
    public long UID { get; set; }

    [Indexed]
    public string Owner { get; set; }

    public int ID { get; set; }
    public int CurrentCD { get; set; }
}