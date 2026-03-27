using Newtonsoft.Json;

public class GetBattleStatus : IApiHandler<GetBattleStatusResponse>
{
    public GetBattleStatusResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetBattleStatusResponse>(response.ToString());
    }
}

public class GetBattleStatusRequest : IRequestBase<GetBattleStatusResponse>
{
    public string Cmd => "GetBattleStatus";
}

public class GetBattleStatusResponse
{    
    public GameSaveData SaveData;
    public BattleResult BattleResult;
}
