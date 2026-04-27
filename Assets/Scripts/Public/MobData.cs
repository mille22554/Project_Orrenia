using System.Collections.Generic;
using Unity.Netcode;

public class MobData : INetworkSerializable
{
    public int MobID;
    public CharacterData CharacterData;
    public List<DropItem> DropItems;

    public static MobData CreateDefault()
    {
        var data = new MobData
        {
            CharacterData = CharacterData.CreateDefault(),
            DropItems = new(),
        };

        return data;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var dropItems = DropItems.ToArray();

        serializer.SerializeValue(ref MobID);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref dropItems);
    }
}

public class DropItem : INetworkSerializable
{
    public int Item;
    public int Prop;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Item);
        serializer.SerializeValue(ref Prop);
    }
}