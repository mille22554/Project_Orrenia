using System.Collections.Generic;
using Newtonsoft.Json;

public class SetForgeAction : IApiHandler<SetForgeActionResponse>
{
    public SetForgeActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetForgeActionResponse>(response.ToString());
    }
}

public class SetForgeActionRequest : RequestBase<SetForgeActionResponse>
{
    public override string Cmd =>"SetForgeAction";
    public string ItemName;
    public EItemKind ItemKind;
    public List<long> Materials;
}

public class SetForgeActionResponse
{
    public List<BagItemData> BagItemDatas;
}
