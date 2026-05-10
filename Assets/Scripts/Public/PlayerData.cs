using Unity.Netcode;

public class PlayerData : INetworkSerializable
{
    public long UID;
    public string Account = "";
    public string Password = "";
    public int Gold;
    public long PartyUID;
    public int SkillPoint;
    public int ForgeLevel;
    public int CurrentForgeExp;

    public static PlayerData CreateDefault()
    {
        var data = new PlayerData
        {
            Gold = 0,
            SkillPoint = 0,
            ForgeLevel = 1,
            CurrentForgeExp = 0,
        };

        return data;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref Password);
        serializer.SerializeValue(ref Gold);
        // serializer.SerializeValue(ref NowPartyLeader);
        serializer.SerializeValue(ref PartyUID);
        serializer.SerializeValue(ref SkillPoint);
        serializer.SerializeValue(ref ForgeLevel);
        serializer.SerializeValue(ref CurrentForgeExp);
    }
}