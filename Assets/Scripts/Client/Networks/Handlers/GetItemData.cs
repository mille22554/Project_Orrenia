using System.Collections.Generic;
using Newtonsoft.Json;

public class GetItemData : IApiHandler<GetItemDataResponse>
{
    public GetItemDataResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<GetItemDataResponse>(response.ToString());
    }
}

public class GetItemDataRequest : IRequestBase<GetItemDataResponse>
{
    public string Cmd => "GetItemData";
}

public class GetItemDataResponse
{
    public Dictionary<int, ItemData> ItemData;
    public Dictionary<EItemKind, ItemKind> ItemKind;
    public List<int> GameShopItem;
}
