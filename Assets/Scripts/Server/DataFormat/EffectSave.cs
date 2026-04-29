using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

public class EffectSave : IDBTable
{
    [PrimaryKey, AutoIncrement]
    public long UID { get; set; }

    [Indexed]
    public long Owner { get; set; }

    public EEffectID ID { get; set; }
    public string Value { get; set; }
    public int Times { get; set; }

    public static EffectSave Create(EffectData data)
    {
        var saveData = new EffectSave
        {
            Owner = data.Owner,
            ID = data.ID,
            Value = JsonConvert.SerializeObject(data.Value),
            Times = data.Times,
        };

        return saveData;
    }

    public static EffectData GetData(EffectSave save)
    {
        var data = new EffectData
        {
            Owner = save.Owner,
            ID = save.ID,
            Value = JsonConvert.DeserializeObject<List<ParamFormat>>(save.Value),
            Times = save.Times,
        };

        return data;
    }
}