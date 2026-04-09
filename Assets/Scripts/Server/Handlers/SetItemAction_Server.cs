using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SetItemAction_Server : IApiHandler_Server
{
    public string Cmd => "SetItemAction";

    CharacterData CharacterData => GameData_Server.NowCharacterData;
    BagData BagData => GameData_Server.NowBagData;

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetItemAction_ServerRequest>(request.ToString());

            var responseData = Do(requestData.BagItemData);

            SaveDataCenter.SaveData();

            var response = new ResponseData_Server
            {
                Code = 0,
                Data = responseData
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"使用道具時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    SetItemAction_ServerResponse Do(BagItemData bagItemData)
    {
        var response = new SetItemAction_ServerResponse();

        var itemKind = ItemDataCenter_Server.GetItemKind(bagItemData.Kind);

        ItemDataCenter_Server.DoActionAccordingToCategory(bagItemData.Kind, EquipCallBack, UseCallBack, null);

        if (CharacterData.CurrentTP >= GameData_Server.tpCost)
            CharacterData.CurrentTP -= GameData_Server.tpCost;

        response.ItemCategory = itemKind.Category;
        response.BagItemData = bagItemData;
        response.Enemies = GameData_Server.NowEnemyData.Enemies;
        response.CharacterData = CharacterData;
        response.FullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData);

        return response;

        void EquipCallBack() => RefreshEquipState(bagItemData.Kind, bagItemData.UID);

        void UseCallBack()
        {
            bagItemData = BagData.Items.Find(x => x.UID == bagItemData.UID);
            bagItemData.Count--;

            if (bagItemData.Count == 0)
                BagData.Items.Remove(bagItemData);

            CharacterDataCenter.MotifyCurrentAbility(CharacterData, bagItemData.Ability);

            if (bagItemData.Effects != null)
            {
                foreach (var effect in bagItemData.Effects)
                {
                    Debug.Log($"ID: {effect.ID}, Value: {effect.Value}, Times: {effect.Times}");
                    CharacterDataCenter.AddCharacterEffect(CharacterData, effect.ID, effect.Value, effect.Times);
                }
            }

            // CharacterDataCenter.EffectProcess(CharacterData);
        }
    }

    void RefreshEquipState(EItemKind kind, long uid)
    {
        var equips = CharacterData.Equips;

        if (equips.Contains(uid))
        {
            equips.Remove(uid);
        }
        else
        {
            foreach (var equipUID in equips)
            {
                var equip = BagData.Items.Find(x => x.UID == equipUID);

                if (equip.Kind == kind)
                {
                    equips.Remove(equip.UID);
                    break;
                }
            }

            equips.Add(uid);
        }
    }
}

public class SetItemAction_ServerRequest
{
    public BagItemData BagItemData;
}

public class SetItemAction_ServerResponse
{
    public EItemCategory ItemCategory;
    public BagItemData BagItemData;
    public bool IsEquipped;
    public List<MobData> Enemies;
    public CharacterData CharacterData;
    public FullAbilityBase FullAbility;
}
