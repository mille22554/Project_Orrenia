using System.Collections.Generic;
using Newtonsoft.Json;

public class SetItemAction : IApiHandler<SetItemActionResponse>
{
    public SetItemActionResponse Get(object response)
    {
        return JsonConvert.DeserializeObject<SetItemActionResponse>(response.ToString());
    }
}

public class SetItemActionRequest : RequestBase<SetItemActionResponse>
{
    public override string Cmd =>"SetItemAction";
    public BagItemData BagItemData;
}

public class SetItemActionResponse
{
    public EItemCategory ItemCategory;
    public BagItemData BagItemData;
    public List<BagItemData> UnEquiped;
    public bool IsEquipped;
    public List<MobData> Enemies;
    public CharacterData CharacterData;
    public FullAbilityBase FullAbility;
}
