

using Unity.Netcode;

public class PlayerContextData : INetworkSerializable
{
    public string Password = "";
    public int Gold;

    public string NowPartyLeader = "";

    public int SkillPoint;

    public int ForgeLevel;
    public int CurrentForgeExp;

    public static PlayerContextData CreateDefault()
    {
        var data = new PlayerContextData
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
        serializer.SerializeValue(ref Password);
        serializer.SerializeValue(ref Gold);
        serializer.SerializeValue(ref NowPartyLeader);
        serializer.SerializeValue(ref SkillPoint);
        serializer.SerializeValue(ref ForgeLevel);
        serializer.SerializeValue(ref CurrentForgeExp);
    }
}