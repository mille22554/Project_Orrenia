using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static GameItemData;

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

            var responseData = Do(requestData.ItemData);

            PublicFunc.SaveData();

            var response = new ResponseData_Server
            {
                Code = 0,
                Data = responseData
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    SetItemAction_ServerResponse Do(ItemData itemData)
    {
        var response = new SetItemAction_ServerResponse();

        var itemBaseData = ItemBaseData.Get(itemData.ItemID);

        if (ItemTypeCheck.IsEquipType(itemBaseData.Type))
        {
            response.IsEquipped = PublicFunc.CheckIsPlayerEquip(itemData, CharacterData.Equips);
            RefreshEquipState(itemBaseData.Type, itemData.UID, response.IsEquipped);

            response.ItemType = 0;
        }
        else if (ItemTypeCheck.IsUseType(itemBaseData.Type))
        {
            var item = BagData.Items.Find(x => x.UID == itemData.UID);
            item.Count--;

            if (item.Count == 0)
                BagData.Items.Remove(item);

            if (itemBaseData.Ability.HP != 0)
                CharacterData.CurrentHP += itemBaseData.Ability.HP;

            if (itemBaseData.Ability.MP != 0)
                CharacterData.CurrentMP += itemBaseData.Ability.MP;

            if (itemBaseData.Ability.STA != 0)
                CharacterData.CurrentSTA += itemBaseData.Ability.STA;

            UseItemSpecial(itemBaseData.Name);
            PublicFunc.EffectProcess();

            response.ItemType = 1;
        }
        response.ItemData = itemData;
        response.Enemies=GameData_Server.NowEnemyData.Enemies;

        if (CharacterData.CurrentTP >= GameData_Server.tpCost)
        {
            CharacterData.CurrentTP -= GameData_Server.tpCost;
        }

        return response;
    }

    void RefreshEquipState(string type, long uid, bool isEquipped)
    {
        string fieldName = type switch
        {
            EquipType.One_Hand_Weapon.Sword or EquipType.One_Hand_Weapon.Dagger => "Right_Hand",
            EquipType.Shield => "Left_Hand",
            EquipType.Helmet => "Helmet",
            EquipType.Armor => "Armor",
            EquipType.Greaves => "Greaves",
            EquipType.Shoes => "Shoes",
            EquipType.Gloves => "Gloves",
            EquipType.Cape => "Cape",
            EquipType.Ring => "Ring",
            EquipType.Pendant => "Pendant",
            _ => null
        };

        if (fieldName == null)
            return;

        var equips = CharacterData.Equips;
        var field = typeof(EquipBase).GetField(fieldName);
        var currentUid = (long)field.GetValue(equips);

        // 解除當前裝備
        PublicFunc.UnloadEquip(currentUid);

        if (isEquipped)
        {
            field.SetValue(equips, 0L);
        }
        else
        {
            field.SetValue(equips, uid);
        }
    }

    void UseItemSpecial(string itemName)
    {
        if (itemName == Use.BerserkPotion.Name)
        {
            PublicFunc.AddCharacterEffect(CharacterData, EffectType.Buff.Berserk, 2, 100);
        }
    }

}

public class SetItemAction_ServerRequest
{

    public ItemData ItemData;
}

public class SetItemAction_ServerResponse
{
    public int ItemType;
    public ItemData ItemData;
    public bool IsEquipped;
    public List<MobData> Enemies;
}
