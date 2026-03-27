using Newtonsoft.Json;

public class SetPlayerAbility : IApiHandler<SetPlayerAbilityResponse>
{
    public SetPlayerAbilityResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetPlayerAbilityResponse>(response.ToString());
    }
}

public class SetPlayerAbilityRequest : IRequestBase<SetPlayerAbilityResponse>
{
    public string Cmd => "SetPlayerAbility";
    public AbilityBase Ability;
}

public class SetPlayerAbilityResponse { }
