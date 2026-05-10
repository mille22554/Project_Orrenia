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

    public void Send(CheckIsNewAccountRequest requestData) => Send(requestData, null);
    public void Send(CheckIsNewAccountRequest requestData, Action<CheckIsNewAccountResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(CheckIsNewAccountResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(CheckIsNewAccountResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(CheckIsNewAccountResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<CheckIsNewAccountResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<CheckIsNewAccountResponse>)_all_OnceListeners[typeof(CheckIsNewAccountResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(CheckIsNewAccountResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(CheckIsNewAccountRequest requestData, RpcParams rpcParams = default)
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

    CheckIsNewAccountResponse Main(CheckIsNewAccountRequest requestData)
    {
        try
        {
            var account = requestData.Account;
            var path = GameData_Server.SaveDataBasePath();
            Debug.Log($"從 {path} 讀取遊戲資料");

            var playerData = SaveDataCenter.GetPlayerData(account);

            var responseData = new CheckIsNewAccountResponse
            {
                Code = EErrorCode.None,
                IsNewAccount = playerData == null
            };

            if (responseData.IsNewAccount)
            {
                SaveDataCenter.CreateSaveData_New(account);

                var characterData = SaveDataCenter.GetCharacterData(account);
                CharacterDataCenter.InitCurrentData(characterData);
                SaveDataCenter.SaveDataToDB(CharacterSave.Create(characterData));

                responseData.UID = characterData.UID;
            }
            else
            {
                responseData.UID = SaveDataCenter.GetPlayerData(account).UID;
            }

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new CheckIsNewAccountResponse
            {
                Code = EErrorCode.CheckIsNewAccount,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class CheckIsNewAccountRequest : INetworkSerializable
{
    public string Account = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
    }
}

public class CheckIsNewAccountResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public bool IsNewAccount;
    public long UID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref IsNewAccount);
        serializer.SerializeValue(ref UID);
    }
}