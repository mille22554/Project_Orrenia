using System.Collections.Generic;
using Unity.Netcode;

public class PartyData : INetworkSerializable
{
    public string Leader;
    public List<string> Members = new();
    public int Area;
    public int Deep;
    public List<MobData> Enemies = new();

    public static PartyData CreateDefault()
    {
        return new PartyData
        {
            Area = 1,
            Deep = 0,
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var enemies = Enemies.ToArray();

        serializer.SerializeValue(ref Leader);
        serializer.SerializeValue(ref Area);
        serializer.SerializeValue(ref Deep);
        serializer.SerializeValue(ref enemies);

        for (var i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            serializer.SerializeValue(ref member);
        }
    }
}