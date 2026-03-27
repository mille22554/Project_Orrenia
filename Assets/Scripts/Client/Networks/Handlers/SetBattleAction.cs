using Newtonsoft.Json;

public class SetBattleAction : IApiHandler<SetBattleActionResponse>
{
    public SetBattleActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetBattleActionResponse>(response.ToString());
    }
}

public class SetBattleActionRequest : IRequestBase<SetBattleActionResponse>
{
    public string Cmd => "SetBattleAction";
    public BattleActionType BattleAction;
    public MobData AttackTarget;
}

public class SetBattleActionResponse
{
    public GameSaveData SaveData;
    public ActionResult ActionResult;
}
