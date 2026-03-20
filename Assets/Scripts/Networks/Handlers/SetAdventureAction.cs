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
    public AdventureActionType AdventureAction;
    public int GameArea;
}

public class SetAdventureActionResponse
{
    public GameSaveData SaveData;
}
