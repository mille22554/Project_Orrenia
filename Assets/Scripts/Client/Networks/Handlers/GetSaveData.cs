using Newtonsoft.Json;

public class GetSaveData : IApiHandler<GetSaveDataResponse>
{
    public GetSaveDataResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetSaveDataResponse>(response.ToString());
    }
}

public class GetSaveDataRequest : RequestBase<GetSaveDataResponse>
{
    public override string Cmd =>"GetSaveData";
}

public class GetSaveDataResponse
{
    public Datas SaveData;
    public PartyData PartyData;
    public FullAbilityBase FullAbility;
    public int AbilityPoint;
    public int Exp;
}
