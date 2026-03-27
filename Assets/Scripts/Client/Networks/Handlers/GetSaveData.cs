using Newtonsoft.Json;

public class GetSaveData : IApiHandler<GetSaveDataResponse>
{
    public GetSaveDataResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetSaveDataResponse>(response.ToString());
    }
}

public class GetSaveDataRequest : IRequestBase<GetSaveDataResponse>
{
    public string Cmd => "GetSaveData";
}

public class GetSaveDataResponse
{
    public SaveDataFormat SaveData;
    public FullAbilityBase FullAbility;
    public int AbilityPoint;
    public int Exp;
}
