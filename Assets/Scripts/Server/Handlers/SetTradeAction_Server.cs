using System;
using Newtonsoft.Json;
using UnityEngine;

public class SetTradeAction_Server : IApiHandler_Server
{
    public string Cmd => "SetTradeAction";

    string _account;
    CharacterData CharacterData => GameData_Server.GetCharacterData(_account);
    PlayerContextData PlayerData => GameData_Server.GetPlayerData(_account);

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetTradeAction_ServerRequest>(request.ToString());
            _account = requestData.Account;

            var itemData = ItemDataCenter_Server.GetItemData(requestData.ItemID);
            var sellItemSurplus = -1;

            switch (requestData.TradeActionType)
            {
                case ETradeActionType.Buy:
                    OnBuy(itemData, requestData.TradeNum);
                    break;
                case ETradeActionType.Sell:
                    sellItemSurplus = OnSell(itemData, requestData.TradeNum, requestData.SelledItemUID);
                    break;
            }

            SaveDataCenter.SaveData(requestData.Account);

            var responseData = new SetTradeAction_ServerResponse
            {
                Gold = PlayerData.Gold,
                SelledItemSurplus = sellItemSurplus
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
            var errorMessage = $"交易道具時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    public void OnBuy(ItemData itemData, int tradeNum)
    {
        if (PlayerData.Gold >= itemData.Price * tradeNum)
        {
            PlayerData.Gold -= itemData.Price * tradeNum;

            var existing = CharacterData.BagItems.Find(item => item.ItemID == itemData.ID);

            ItemDataCenter_Server.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

            void EquipCallBack()
            {
                for (int i = 0; i < tradeNum; i++)
                    CharacterData.BagItems.Add(ItemDataCenter_Server.GetNewItem(itemData));
            }

            void OtherCallBack()
            {
                if (existing == null)
                {
                    var buyItem = ItemDataCenter_Server.GetNewItem(itemData);
                    buyItem.Count = tradeNum;
                    CharacterData.BagItems.Add(buyItem);
                }
                else
                {
                    existing.Count += tradeNum;
                }
            }
        }
    }

    public int OnSell(ItemData itemData, int tradeNum, long sellItemUID)
    {
        var existing = CharacterData.BagItems.Find(item => item.UID == sellItemUID);
        var sellItemSurplus = existing.Count;

        if (existing != null && existing.Count >= tradeNum)
        {
            existing.Count -= tradeNum;
            PlayerData.Gold += itemData.Price / 2 * tradeNum;

            ItemDataCenter_Server.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

            void EquipCallBack()
            {
                CharacterData.BagItems.Remove(existing);
                sellItemSurplus = 0;
            }

            void OtherCallBack()
            {
                sellItemSurplus = existing.Count;

                if (sellItemSurplus == 0)
                    CharacterData.BagItems.Remove(existing);
            }
        }

        return sellItemSurplus;
    }
}

public class SetTradeAction_ServerRequest : ServerRequestBase
{
    public ETradeActionType TradeActionType;
    public int ItemID;
    public int TradeNum;
    public long SelledItemUID;
}

public class SetTradeAction_ServerResponse
{
    public int Gold;
    public int SelledItemSurplus;
}
