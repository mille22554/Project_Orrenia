using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

public class BagItemSave : IDBTable
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public long Owner { get; set; }
    [Indexed]
    public bool IsEquipped { get; set; }

    public int ID { get; set; }
    public string Ability { get; set; }
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
            UID = data.UID,
            Owner = data.Owner,
            IsEquipped = data.IsEquipped,
            ID = data.ID,
            Ability = JsonConvert.SerializeObject(data.Ability),
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
            IsEquipped = save.IsEquipped,
            ID = save.ID,
            Ability = JsonConvert.DeserializeObject<FullAbilityBase>(save.Ability),
            Quality = save.Quality,
            Materials = JsonConvert.DeserializeObject<List<int>>(save.Materials),
            Seed = save.Seed,
            Price = save.Price,
            Durability = save.Durability,
            Count = save.Count,
        };

        BagItemData.SetInfo(ItemDataCenter_Server.GetItemData(save.ID), data);

        return data;
    }
}