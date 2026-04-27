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

    public void Send(SetPlayerAbilityRequest requestData) => Send(requestData, null);
    public void Send(SetPlayerAbilityRequest requestData, Action<SetPlayerAbilityResponse> callback)
    {
        _all_OnceListeners[typeof(SetPlayerAbilityResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetPlayerAbilityResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetPlayerAbilityResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetPlayerAbilityResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetPlayerAbilityResponse>)_all_OnceListeners[typeof(SetPlayerAbilityResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetPlayerAbilityResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetPlayerAbilityRequest requestData, RpcParams rpcParams = default)
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

    SetPlayerAbilityResponse Main(SetPlayerAbilityRequest requestData)
    {
        try
        {
            var account = requestData.Account;
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = new SetPlayerAbilityResponse
            {
                Code = EErrorCode.None,
                CharacterData = characterData,
                AbilityPoint = PublicFunc.GetAbilityPoint(characterData)
            };

            if (responseData.AbilityPoint >= 0)
            {
                characterData.Ability = requestData.Ability;
                responseData.AbilityPoint = PublicFunc.GetAbilityPoint(characterData);
                SaveDataCenter.SaveData(account);
            }

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家能力值時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetPlayerAbilityResponse
            {
                Code = EErrorCode.SetPlayerAbility,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class SetPlayerAbilityRequest : INetworkSerializable
{
    public string Account = "";
    public AbilityBase Ability = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref Ability);
    }
}

public class SetPlayerAbilityResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public CharacterData CharacterData = new();
    public int AbilityPoint;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref AbilityPoint);
    }
}