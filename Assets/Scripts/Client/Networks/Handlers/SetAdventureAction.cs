using System.Collections.Generic;
using Newtonsoft.Json;

public class SetAdventureAction : IApiHandler<SetAdventureActionResponse>
{
    public SetAdventureActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetAdventureActionResponse>(response.ToString());
    }
}

public class SetAdventureActionRequest : RequestBase<SetAdventureActionResponse>
{
    public override string Cmd =>"SetAdventureAction";
    public EAdventureActionType AdventureAction;
    public int GameArea;
}

public class SetAdventureActionResponse
{
    public bool IsLeader;
    public Datas Datas;
    public PartyData PartyData;
    public ActionResult ActionResult;
    public FullAbilityBase FullAbility;
}
