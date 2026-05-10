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

    public void Send(GetCharacterInfoRequest requestData) => Send(requestData, null);
    public void Send(GetCharacterInfoRequest requestData, Action<GetCharacterInfoResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(GetCharacterInfoResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetCharacterInfoResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetCharacterInfoResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetCharacterInfoResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetCharacterInfoResponse>)_all_OnceListeners[typeof(GetCharacterInfoResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetCharacterInfoResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetCharacterInfoRequest requestData, RpcParams rpcParams = default)
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

    GetCharacterInfoResponse Main(GetCharacterInfoRequest requestData)
    {
        try
        {
            var uid = requestData.UID;
            var characterData = SaveDataCenter.GetCharacterData(uid);

            var responseData = new GetCharacterInfoResponse
            {
                Code = EErrorCode.None,
                CharacterData = characterData,
                Ability = SaveDataCenter.GetCharacterAbilityData(uid),
                AbilityPoint = CharacterDataCenter.GetAbilityPoint(characterData)
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetCharacterInfoResponse
            {
                Code = EErrorCode.GetCharacterInfo,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class GetCharacterInfoRequest : INetworkSerializable
{
    public long UID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
    }
}

public class GetCharacterInfoResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public CharacterData CharacterData = new();
    public AbilityBase Ability = new();
    public int AbilityPoint;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref Ability);
        serializer.SerializeValue(ref AbilityPoint);
    }
}