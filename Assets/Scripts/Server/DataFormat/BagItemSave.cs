using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

public class BagItemSave : IDBTable
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public long Owner { get; set; }

    public int ID { get; set; }
    public EQuality Quality { get; set; }
    public string Materials { get; set; }
    public int Seed { get; set; }
    public int Price { get; set; }
    public int Durability { get; set; }
    public int Count { get; set; }

    public static BagItemSave Create(BagItemData data)
    {
        var saveData = new BagItemSave
        {
            Owner = data.Owner,
            ID = data.ID,
            Quality = data.Quality,
            Materials = JsonConvert.SerializeObject(data.Materials),
            Seed = data.Seed,
            Price = data.Price,
            Durability = data.Durability,
            Count = data.Count,
        };

        return saveData;
    }

    public static BagItemData GetData(BagItemSave save)
    {
        var data = new BagItemData
        {
            UID = save.UID,
            Owner = save.Owner,
            ID = save.ID,
            Quality = save.Quality,
            Materials = JsonConvert.DeserializeObject<List<int>>(save.Materials),
            Seed = save.Seed,
            Price = save.Price,
            Durability = save.Durability,
            Count = save.Count,
        };

        return data;
    }
}