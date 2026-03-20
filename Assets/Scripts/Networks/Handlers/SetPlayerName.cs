using Newtonsoft.Json;

public class SetPlayerName : IApiHandler<SetPlayerNameResponse>
{
    public SetPlayerNameResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetPlayerNameResponse>(response.ToString());
    }
}

public class SetPlayerNameRequest : IRequestBase<SetPlayerNameResponse>
{
    public string Cmd => "SetPlayerName";
    public string PlayerName;
}

public class SetPlayerNameResponse { }
