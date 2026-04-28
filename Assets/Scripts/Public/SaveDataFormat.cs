using System.Collections.Generic;
using Unity.Netcode;

public class PlayerSaveDataFormat
{
    public string version = "";
    public Datas Datas = new();
}

public class Datas : INetworkSerializable
{
    public PlayerContextData PlayerData = new();
    public CharacterData CharacterData = new();
    public PartyData PartyData = new();

    public static Datas CreateDefault()
    {
        var datas = new Datas
        {
            PlayerData = PlayerContextData.CreateDefault(),
            CharacterData = CharacterData.CreateDefault(),
            PartyData = PartyData.CreateDefault(),
        };

        return datas;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerData);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref PartyData);
    }
}

public class PartySaveDataFormat
{
    public long PartyID;
    public string Leader = "";
    public List<string> Members = new();
    public int Area;
    public int Deep;
    public List<MobData> Enemies = new();
}

public class EnemyData
{
    public List<MobData> Enemies = new();
}