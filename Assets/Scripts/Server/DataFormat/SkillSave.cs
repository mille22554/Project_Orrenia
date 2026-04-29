using SQLite;

public class SkillSave : IDBTable
{
    [PrimaryKey]
    public long UID { get; set; }

    [Indexed]
    public long Owner { get; set; }

    public ESkillID ID { get; set; }
    public int CurrentCD { get; set; }

    public static SkillSave Create(SkillData data)
    {
        var saveData = new SkillSave
        {
            Owner = data.Owner,
            ID = data.ID,
            CurrentCD = data.CurrentCD
        };

        return saveData;
    }

    public static SkillData GetData(SkillSave save)
    {
        var data = new SkillData
        {
            Owner = save.Owner,
            ID = save.ID,
            CurrentCD = save.CurrentCD
        };

        return data;
    }
}