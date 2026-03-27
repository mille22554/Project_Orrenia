using System.Collections.Generic;
using Newtonsoft.Json;

public class GetAreaData : IApiHandler<GetAreaDataResponse>
{
    public GetAreaDataResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetAreaDataResponse>(response.ToString());
    }
}

public class GetAreaDataRequest : IRequestBase<GetAreaDataResponse>
{
    public string Cmd => "GetAreaData";
}

public class GetAreaDataResponse
{
    public Dictionary<int, AreaData> AreaData;
}
