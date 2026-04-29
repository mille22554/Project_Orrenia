using System.Collections.Generic;
using Unity.Netcode;

public class PartyData : INetworkSerializable
{
    public long UID;
    public int Area;
    public int Deep;

    public static PartyData CreateDefault(long UID)
    {
        return new PartyData
        {
            UID = UID,
            Area = 1,
            Deep = 0,
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
        serializer.SerializeValue(ref Area);
        serializer.SerializeValue(ref Deep);
    }
}