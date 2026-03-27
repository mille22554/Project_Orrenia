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
    public EBattleActionType BattleAction;
    public MobData AttackTarget;
}

public class SetBattleActionResponse
{
    public SaveDataFormat SaveData;
    public ActionResult ActionResult;
}
