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

    public void Send(GetAdventureInfoRequest requestData) => Send(requestData, null);
    public void Send(GetAdventureInfoRequest requestData, Action<GetAdventureInfoResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(GetAdventureInfoResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetAdventureInfoResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetAdventureInfoResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetAdventureInfoResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetAdventureInfoResponse>)_all_OnceListeners[typeof(GetAdventureInfoResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetAdventureInfoResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetAdventureInfoRequest requestData, RpcParams rpcParams = default)
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

    GetAdventureInfoResponse Main(GetAdventureInfoRequest requestData)
    {
        try
        {
            var uid = requestData.UID;
            var playerData = SaveDataCenter.GetPlayerData(uid);

            var responseData = new GetAdventureInfoResponse
            {
                Code = EErrorCode.None,
                PartyData = SaveDataCenter.GetPartyData(playerData.PartyUID),
                Enemies = SaveDataCenter.GetPartyEnemies(playerData.PartyUID)
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetAdventureInfoResponse
            {
                Code = EErrorCode.GetAdventureInfo,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class GetAdventureInfoRequest : INetworkSerializable
{
    public long UID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
    }
}

public class GetAdventureInfoResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public PartyData PartyData = new();
    public List<CharacterData> Enemies = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref PartyData);

        PublicFunc.SerializeClassList(serializer, ref Enemies);
    }
}