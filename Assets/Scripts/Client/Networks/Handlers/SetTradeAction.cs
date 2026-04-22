using System.Collections.Generic;
using Newtonsoft.Json;

public class SetTradeAction : IApiHandler<SetTradeActionResponse>
{
    public SetTradeActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetTradeActionResponse>(response.ToString());
    }
}

public class SetTradeActionRequest : RequestBase<SetTradeActionResponse>
{
    public override string Cmd =>"SetTradeAction";
    public ETradeActionType TradeActionType;
    public int ItemID;
    public int TradeNum;
    public long SelledItemUID;
}

public class SetTradeActionResponse
{
    public int Gold;
    public int SelledItemSurplus;
}
