using System.Collections.Generic;
using Newtonsoft.Json;

public class GetDataBase : IApiHandler<GetDataBaseResponse>
{
    public GetDataBaseResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetDataBaseResponse>(response.ToString());
    }
}

public class GetDataBaseRequest : IRequestBase<GetDataBaseResponse>
{
    public string Cmd => "GetDataBase";
}

public class GetDataBaseResponse
{
    public Dictionary<int, ItemData> ItemData;
    public Dictionary<EItemKind, ItemKind> ItemKind;
    public List<int> GameShopItem;
    public List<QualityData> QualityData;
    public Dictionary<ESkillType, string> DamageTypes;
}
