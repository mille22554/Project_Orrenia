using System.Collections.Generic;

public class MobData
{
    public CharacterData CharacterData;

    public List<DropItem> DropItems;

    public List<EffectData> Effects;

    public static MobData CreateDefault()
    {
        var data = new MobData
        {
            CharacterData = CharacterData.CreateDefault(),
            DropItems = new(),
            Effects = new(),
        };

        return data;
    }
}

public class DropItem
{
    public int Item;
    public int Prop;
}