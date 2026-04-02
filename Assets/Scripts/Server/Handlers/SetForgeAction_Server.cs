using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SetForgeAction_Server : IApiHandler_Server
{
    public string Cmd => "SetForgeAction";

    PlayerContextData PlayerData => GameData_Server.NowPlayerData;
    BagData BagData => GameData_Server.NowBagData;

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetForgeAction_ServerRequest>(request.ToString());
            DoAction(requestData);

            SaveDataCenter.SaveData();

            var responseData = new SetForgeAction_ServerResponse
            {
                BagItemDatas = BagData.Items
            };
            var response = new ResponseData_Server
            {
                Code = 0,
                Data = responseData
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"鍛造裝備時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 4,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    void DoAction(SetForgeAction_ServerRequest request)
    {
        var baseParam = CheckMaterial(request.ItemKind, request.Materials.Count);
        if (baseParam == -1)
            return;

        var newItem = CreateItem(request.ItemKind, baseParam);
        newItem.Name = request.ItemName;
        newItem.Description = $"Create by {GameData_Server.NowCharacterData.Name} at {DateTime.Now:yyyy/MM/dd HH:mm}";
        newItem.Durability = baseParam * 100;
        newItem.Count = 1;

        foreach (var material in request.Materials)
        {
            var bagItem = BagData.Items.Find(x => x.UID == material);

            newItem.Ability.STR += bagItem.Ability.STR;
            newItem.Ability.DEX += bagItem.Ability.DEX;
            newItem.Ability.INT += bagItem.Ability.INT;
            newItem.Ability.VIT += bagItem.Ability.VIT;
            newItem.Ability.AGI += bagItem.Ability.AGI;
            newItem.Ability.LUK += bagItem.Ability.LUK;
            newItem.Ability.HP += bagItem.Ability.HP;
            newItem.Ability.MP += bagItem.Ability.MP;
            newItem.Ability.STA += bagItem.Ability.STA;
            newItem.Ability.ATK += bagItem.Ability.ATK;
            newItem.Ability.MATK += bagItem.Ability.MATK;
            newItem.Ability.DEF += bagItem.Ability.DEF;
            newItem.Ability.MDEF += bagItem.Ability.MDEF;
            newItem.Ability.ACC += bagItem.Ability.ACC;
            newItem.Ability.EVA += bagItem.Ability.EVA;
            newItem.Ability.CRIT += bagItem.Ability.CRIT;
            newItem.Ability.SPD += bagItem.Ability.SPD;

            bagItem.Count--;
            if (bagItem.Count == 0)
                BagData.Items.Remove(bagItem);
        }

        BagData.Items.Add(ItemDataCenter_Server.GetNewItem(newItem));

        ForgeExpProcess();
    }

    int CheckMaterial(EItemKind kind, int inputNum)
    {
        int needNum = 0;
        switch (ItemDataCenter_Server.GetItemKind(kind).Category)
        {
            case EItemCategory.One_Hand:
                needNum = 6;
                break;
            case EItemCategory.Two_Hand:
                needNum = 10;
                break;
            case EItemCategory.Shield:
            case EItemCategory.Helmet:
            case EItemCategory.Armor:
            case EItemCategory.Greaves:
            case EItemCategory.Shoes:
            case EItemCategory.Gloves:
            case EItemCategory.Cape:
            case EItemCategory.Ring:
            case EItemCategory.Pendant:
                needNum = 8;
                break;
        }

        if (needNum == inputNum)
            return needNum;
        else
            return -1;
    }

    ItemData CreateItem(EItemKind kind, int baseParam)
    {
        var item = new ItemData
        {
            Kind = kind
        };

        var forgeParam = GameData_Server.NowPlayerData.ForgeLevel - 1;
        switch (kind)
        {
            case EItemKind.Sword:
                item.Ability = new()
                {
                    ATK = 5 + forgeParam,
                    STR = 5 + forgeParam
                };
                break;
            case EItemKind.Hammer:
                item.Ability = new()
                {
                    ATK = 3 + forgeParam,
                    DEF = 1 + forgeParam,
                    MDEF = 1 + forgeParam,
                    VIT = 5 + forgeParam
                };
                break;
            case EItemKind.Spear:
                item.Ability = new()
                {
                    ATK = 4 + forgeParam,
                    SPD = 1 + forgeParam,
                    DEX = 5 + forgeParam
                };
                break;
            case EItemKind.Staff:
                item.Ability = new()
                {
                    MATK = 5 + forgeParam,
                    INT = 5 + forgeParam
                };
                break;
            case EItemKind.Rapier:
                item.Ability = new()
                {
                    ATK = 3 + forgeParam,
                    CRIT = 2 + forgeParam,
                    AGI = 5 + forgeParam
                };
                break;
            case EItemKind.Dagger:
                item.Ability = new()
                {
                    ATK = 2 + forgeParam,
                    LUK = 8 + forgeParam
                };
                break;
            case EItemKind.Axe:
                item.Ability = new()
                {
                    ATK = 8 + forgeParam,
                    STR = 8 + forgeParam
                };
                break;
            case EItemKind.Aegis:
                item.Ability = new()
                {
                    ATK = 4 + forgeParam,
                    DEF = 2 + forgeParam,
                    MDEF = 2 + forgeParam,
                    VIT = 8 + forgeParam
                };
                break;
            case EItemKind.Bow:
                item.Ability = new()
                {
                    ATK = 6 + forgeParam,
                    SPD = 1 + forgeParam,
                    EVA = 1 + forgeParam,
                    DEX = 8 + forgeParam
                };
                break;
            case EItemKind.Book:
                item.Ability = new()
                {
                    MATK = 8 + forgeParam,
                    INT = 8 + forgeParam
                };
                break;
            case EItemKind.Katana:
                item.Ability = new()
                {
                    ATK = 6 + forgeParam,
                    CRIT = 2 + forgeParam,
                    AGI = 8 + forgeParam
                };
                break;
            case EItemKind.Tarot:
                item.Ability = new()
                {
                    ATK = 4 + forgeParam,
                    LUK = 12 + forgeParam
                };
                break;
            case EItemKind.Shield:
                item.Ability = new()
                {
                    ATK = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Helmet:
                item.Ability = new()
                {
                    ACC = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Armor:
                item.Ability = new()
                {
                    DEF = 5 + forgeParam,
                    MDEF = 5 + forgeParam
                };
                break;
            case EItemKind.Greaves:
                item.Ability = new()
                {
                    STA = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Shoes:
                item.Ability = new()
                {
                    SPD = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Gloves:
                item.Ability = new()
                {
                    CRIT = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Cape:
                item.Ability = new()
                {
                    EVA = 2 + forgeParam,
                    DEF = 4 + forgeParam,
                    MDEF = 4 + forgeParam
                };
                break;
            case EItemKind.Ring:
                item.Ability = new()
                {
                    STR = 1 + forgeParam,
                    VIT = 1 + forgeParam,
                    DEX = 1 + forgeParam,
                    INT = 1 + forgeParam,
                    AGI = 1 + forgeParam,
                    LUK = 1 + forgeParam,
                    DEF = 2 + forgeParam,
                    MDEF = 2 + forgeParam
                };
                break;
            case EItemKind.Pendant:
                item.Ability = new()
                {
                    HP = 1 + forgeParam,
                    MP = 1 + forgeParam,
                    STA = 1 + forgeParam,
                    ACC = 1 + forgeParam,
                    EVA = 1 + forgeParam,
                    CRIT = 1 + forgeParam,
                    DEF = 2 + forgeParam,
                    MDEF = 2 + forgeParam
                };
                break;
        }

        item.Price = (int)(baseParam * 100 * ((float)forgeParam / 10 + 1));

        return item;
    }

    void ForgeExpProcess()
    {
        PlayerData.CurrentForgeExp++;

        var maxExp = PublicFunc.GetExp(PlayerData.ForgeLevel);
        if (PlayerData.CurrentForgeExp >= maxExp)
        {
            PlayerData.ForgeLevel++;
            PlayerData.CurrentForgeExp -= maxExp;
        }
    }
}

public class SetForgeAction_ServerRequest
{
    public string ItemName;
    public EItemKind ItemKind;
    public List<long> Materials;
}

public class SetForgeAction_ServerResponse
{
    public List<BagItemData> BagItemDatas;
}
