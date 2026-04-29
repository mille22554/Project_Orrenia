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

    public void Send(InitDataBaseRequest requestData) => Send(requestData, null);
    public void Send(InitDataBaseRequest requestData, Action<InitDataBaseResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(InitDataBaseResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(InitDataBaseResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(InitDataBaseResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<InitDataBaseResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<InitDataBaseResponse>)_all_OnceListeners[typeof(InitDataBaseResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(InitDataBaseResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(InitDataBaseRequest requestData, RpcParams rpcParams = default)
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

    InitDataBaseResponse Main(InitDataBaseRequest requestData)
    {
        try
        {
            SaveDataCenter.Init();

            var responseData = new InitDataBaseResponse
            {
                Code = EErrorCode.None,
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"初始化存檔資料庫時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new InitDataBaseResponse
            {
                Code = EErrorCode.InitDataBase,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class InitDataBaseRequest : INetworkSerializable
{

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
    }
}

public class InitDataBaseResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
    }
}