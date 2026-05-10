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

    public void Send(GetPlayerInfoRequest requestData) => Send(requestData, null);
    public void Send(GetPlayerInfoRequest requestData, Action<GetPlayerInfoResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(GetPlayerInfoResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetPlayerInfoResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetPlayerInfoResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetPlayerInfoResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetPlayerInfoResponse>)_all_OnceListeners[typeof(GetPlayerInfoResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetPlayerInfoResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetPlayerInfoRequest requestData, RpcParams rpcParams = default)
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

    GetPlayerInfoResponse Main(GetPlayerInfoRequest requestData)
    {
        try
        {
            var uid = requestData.UID;
            var characterData = SaveDataCenter.GetCharacterData(uid);

            var responseData = new GetPlayerInfoResponse
            {
                Code = EErrorCode.None,
                CharacterData = characterData,
                FullAbility = CharacterDataCenter.GetCharacterAbility(characterData),
                Exp = PublicFunc.GetExp(characterData.Level),
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"獲取玩家資訊時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetPlayerInfoResponse
            {
                Code = EErrorCode.GetPlayerInfo,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class GetPlayerInfoRequest : INetworkSerializable
{
    public long UID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
    }
}

public class GetPlayerInfoResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public CharacterData CharacterData = new();
    public FullAbilityBase FullAbility = new();
    public int Exp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref FullAbility);
        serializer.SerializeValue(ref Exp);
    }
}