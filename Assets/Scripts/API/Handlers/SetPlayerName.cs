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

    public void Send(SetPlayerNameRequest requestData) => Send(requestData, null);
    public void Send(SetPlayerNameRequest requestData, Action<SetPlayerNameResponse> callback)
    {
        _all_OnceListeners[typeof(SetPlayerNameResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetPlayerNameResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetPlayerNameResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetPlayerNameResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetPlayerNameResponse>)_all_OnceListeners[typeof(SetPlayerNameResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetPlayerNameResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetPlayerNameRequest requestData, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log(clientId);

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

    SetPlayerNameResponse Main(SetPlayerNameRequest requestData)
    {
        try
        {
            var account = requestData.Account;
            var characterData = GameData_Server.GetCharacterData(account);

            characterData.Name = requestData.PlayerName;

            SaveDataCenter.SaveData(account);

            var responseData = new SetPlayerNameResponse
            {
                Code = EErrorCode.None,
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetPlayerNameResponse
            {
                Code = EErrorCode.SetPlayerName,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class SetPlayerNameRequest : INetworkSerializable
{
    public string Account = "";
    public string PlayerName = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref PlayerName);
    }
}

public class SetPlayerNameResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
    }
}