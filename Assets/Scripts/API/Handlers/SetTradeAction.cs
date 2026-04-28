using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public partial class APIController
{
    #region Client

    public void Send(SetTradeActionRequest requestData) => Send(requestData, null);
    public void Send(SetTradeActionRequest requestData, Action<SetTradeActionResponse> callback)
    {
        _all_OnceListeners[typeof(SetTradeActionResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetTradeActionResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetTradeActionResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetTradeActionResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetTradeActionResponse>)_all_OnceListeners[typeof(SetTradeActionResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetTradeActionResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetTradeActionRequest requestData, RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        // Debug.Log(clientId);

        var returnParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                // 注意！這裡的欄位名稱是 Target，而不是 TargetClientIds
                Target = RpcTarget.Single(clientId, RpcTargetUse.Temp)
            }
        };

        ReturnResponseClientRpc(Main(requestData), returnParams);
    }

    SetTradeActionResponse Main(SetTradeActionRequest requestData)
    {
        try
        {
            var account = requestData.Account;
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = new SetTradeActionResponse
            {
                Code = EErrorCode.None,
                Gold = playerData.Gold,
                SelledItemSurplus = -1
            };
            var itemData = ItemDataCenter_Server.GetItemData(requestData.ItemID);

            switch (requestData.TradeActionType)
            {
                case ETradeActionType.Buy:
                    OnBuy(itemData, characterData, playerData, requestData.TradeNum);
                    break;
                case ETradeActionType.Sell:
                    OnSell(itemData, requestData.TradeNum, requestData.SelledItemUID, characterData, playerData, responseData);
                    break;
            }

            SaveDataCenter.SaveData(account);

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"交易道具時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetTradeActionResponse
            {
                Code = EErrorCode.SetTradeAction,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }

    public void OnBuy(ItemData itemData, CharacterData characterData, PlayerContextData playerData, int tradeNum)
    {
        if (playerData.Gold >= itemData.Price * tradeNum)
        {
            playerData.Gold -= itemData.Price * tradeNum;

            var existing = characterData.BagItems.Find(item => item.ItemID == itemData.ID);

            ItemDataCenter_Server.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

            void EquipCallBack()
            {
                for (int i = 0; i < tradeNum; i++)
                    characterData.BagItems.Add(ItemDataCenter_Server.GetNewItem(itemData));
            }

            void OtherCallBack()
            {
                if (existing == null)
                {
                    var buyItem = ItemDataCenter_Server.GetNewItem(itemData);
                    buyItem.Count = tradeNum;
                    characterData.BagItems.Add(buyItem);
                }
                else
                {
                    existing.Count += tradeNum;
                }
            }
        }
    }

    public void OnSell(ItemData itemData, int tradeNum, long sellItemUID, CharacterData characterData, PlayerContextData playerData, SetTradeActionResponse response)
    {
        var existing = characterData.BagItems.Find(item => item.UID == sellItemUID);
        response.SelledItemSurplus = existing.Count;

        if (existing != null && existing.Count >= tradeNum)
        {
            existing.Count -= tradeNum;
            playerData.Gold += itemData.Price / 2 * tradeNum;

            ItemDataCenter_Server.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

            void EquipCallBack()
            {
                characterData.BagItems.Remove(existing);
                response.SelledItemSurplus = 0;
            }

            void OtherCallBack()
            {
                response.SelledItemSurplus = existing.Count;

                if (response.SelledItemSurplus == 0)
                    characterData.BagItems.Remove(existing);
            }
        }
    }
    #endregion
}

public class SetTradeActionRequest : INetworkSerializable
{
    public string Account = "";
    public ETradeActionType TradeActionType;
    public int ItemID;
    public int TradeNum;
    public long SelledItemUID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref TradeActionType);
        serializer.SerializeValue(ref ItemID);
        serializer.SerializeValue(ref TradeNum);
        serializer.SerializeValue(ref SelledItemUID);
    }
}

public class SetTradeActionResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public int Gold;
    public int SelledItemSurplus;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref Gold);
        serializer.SerializeValue(ref SelledItemSurplus);
    }
}