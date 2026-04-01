using Newtonsoft.Json;

public class SetAdventureAction : IApiHandler<SetAdventureActionResponse>
{
    public SetAdventureActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetAdventureActionResponse>(response.ToString());
    }
}

public class SetAdventureActionRequest : IRequestBase<SetAdventureActionResponse>
{
    public string Cmd => "SetAdventureAction";
    public EAdventureActionType AdventureAction;
    public int GameArea;
}

public class SetAdventureActionResponse
{
    public SaveDataFormat SaveData;
    public ActionResult ActionResult;
    public FullAbilityBase FullAbility;
}
