using System.Collections.Generic;
using Unity.Netcode;

public class MobData : INetworkSerializable
{
    public long UID;
    public long PartyUID;
    public int ID;
    // public List<DropItem> DropItems = new();

    public static MobData CreateDefault(long partyUID, int ID)
    {
        var data = new MobData
        {
            PartyUID = partyUID,
            ID = ID,
            // DropItems = new(),
        };

        return data;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
        serializer.SerializeValue(ref PartyUID);
        serializer.SerializeValue(ref ID);

        // PublicFunc.SerializeClassList(serializer, ref DropItems);
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