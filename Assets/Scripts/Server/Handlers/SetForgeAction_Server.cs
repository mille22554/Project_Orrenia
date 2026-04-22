using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class SetForgeAction_Server : IApiHandler_Server
{
    public string Cmd => "SetForgeAction";

    string _account;
    PlayerContextData PlayerData => GameData_Server.GetPlayerData(_account);
    CharacterData CharacterData => GameData_Server.GetCharacterData(_account);

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetForgeAction_ServerRequest>(request.ToString());
            _account = requestData.Account;

            DoAction(requestData);

            SaveDataCenter.SaveData(requestData.Account);

            var responseData = new SetForgeAction_ServerResponse
            {
                BagItemDatas = CharacterData.BagItems
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
        newItem.Description = $"Create by {CharacterData.Name} at {DateTime.Now:yyyy/MM/dd HH:mm}";
        newItem.Durability = baseParam * 20;
        newItem.Count = 1;

        var newBagItem = ItemDataCenter_Server.GetNewItem(newItem);
        newBagItem.Seed = GetStableHashCode($"{DateTime.Now:yyyyMMdd}_{newItem.Name}");

        foreach (var material in request.Materials)
        {
            var bagItem = CharacterData.BagItems.Find(x => x.UID == material);

            newBagItem.Materials.Add(bagItem.ItemID);

            if (newBagItem.Trait != null && bagItem.Trait != null)
            {
                newBagItem.Trait.Poisoning += bagItem.Trait.Poisoning;
            }

            bagItem.Count--;
            if (bagItem.Count == 0)
                CharacterData.BagItems.Remove(bagItem);
        }

        SetQuality(newBagItem);

        CharacterData.BagItems.Add(newBagItem);

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

        var forgeParam = PlayerData.ForgeLevel - 1;
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
            case EItemKind.Book:
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
            case EItemKind.Staff:
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

    int GetStableHashCode(string str)
    {
        unchecked
        {
            int hash = 5381;
            foreach (char c in str)
            {
                hash = (hash * 33) ^ c;
            }
            return hash;
        }
    }

    void SetQuality(BagItemData item)
    {
        var prop = PublicFunc.Dice(1, 100);

        if (prop == 1)//1,3,6,9,13,18
        {
            item.Quality = EQuality.Legendary;
        }
        else if (prop < 5)
        {
            item.Quality = EQuality.Epic;
        }
        else if (prop <= 10)
        {
            item.Quality = EQuality.Rare;
        }
        else if (prop < 20)
        {
            item.Quality = EQuality.Uncommon;
        }
        else if (prop <= 32)
        {
            item.Quality = EQuality.Special;
        }
        else if (prop <= 68)
        {
            item.Quality = EQuality.Common;
        }
        else if (prop <= 81)
        {
            item.Quality = EQuality.Worn;
        }
        else if (prop <= 90)
        {
            item.Quality = EQuality.Old;
        }
        else if (prop <= 96)
        {
            item.Quality = EQuality.Decay;
        }
        else if (prop <= 99)
        {
            item.Quality = EQuality.Broken;
        }
        else if (prop == 100)
        {
            item.Quality = EQuality.Junk;
        }
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

public class SetForgeAction_ServerRequest : ServerRequestBase
{
    public string ItemName;
    public EItemKind ItemKind;
    public List<long> Materials;
}

public class SetForgeAction_ServerResponse
{
    public List<BagItemData> BagItemDatas;
}
