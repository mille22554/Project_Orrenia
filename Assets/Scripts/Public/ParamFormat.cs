using Unity.Netcode;

public class ParamFormat : INetworkSerializable
{
    public decimal Constant;
    public FullAbilityBase Ability = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Constant);
        serializer.SerializeValue(ref Ability);
    }
}