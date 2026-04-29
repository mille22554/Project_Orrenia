using SQLite;

public class EquipSave : IDBTable
{
    [PrimaryKey]
    public long UID { get; set; }

    [Indexed]
    public long Owner { get; set; }

    public static EquipSave Create(BagItemData data)
    {
        var saveData = new EquipSave
        {
            UID = data.UID,
            Owner = data.Owner
        };

        return saveData;
    }
}