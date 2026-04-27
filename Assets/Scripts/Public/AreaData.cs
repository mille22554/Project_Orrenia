using System.Collections.Generic;
using Unity.Netcode;

public class AreaData : INetworkSerializable
{
    public string Name = "";
    public List<int> MobList = new();
    public int MinMobLevel;
    public int MaxMobLevel;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var mobList = MobList.ToArray();

        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref mobList);
        serializer.SerializeValue(ref MinMobLevel);
        serializer.SerializeValue(ref MaxMobLevel);
    }
}