using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public partial class APIController
{
    #region Client

    public void Send(GetBattleStatusRequest requestData) => Send(requestData, null);
    public void Send(GetBattleStatusRequest requestData, Action<GetBattleStatusResponse> callback)
    {
        _all_OnceListeners[typeof(GetBattleStatusResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetBattleStatusResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetBattleStatusResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetBattleStatusResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetBattleStatusResponse>)_all_OnceListeners[typeof(GetBattleStatusResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetBattleStatusResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetBattleStatusRequest requestData, RpcParams rpcParams = default)
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

    GetBattleStatusResponse Main(GetBattleStatusRequest requestData)
    {
        try
        {
            var account = requestData.Account.ToString();
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = new GetBattleStatusResponse
            {
                Code = EErrorCode.None,
            };
            var enemies = partyData.Enemies;

            if (enemies.Count > 0)
            {
                var (battleResult, nowActorEffectResult) = BattleSystem.CheckNowActor(partyData);
                responseData.EffectResult.Results.Add(nowActorEffectResult);

                if (battleResult != null && (battleResult.IsAttackerDead || battleResult.Results.Any(x => x.IsDefenderDead)))
                {
                    var playerDeadAtDefence = battleResult.Results.Any(x => x.IsDefenderDead && partyData.Members.Any(y => y == x.Defenderer));

                    if (battleResult.IsAttackerDead && characterData.Name == battleResult.Attacker || playerDeadAtDefence)
                    {
                        if (!partyData.Members.Any(x => GameData_Server.GetCharacterData(x).CurrentHP > 0))
                            OnGoHome(partyData);
                    }
                    else
                    {
                        foreach (var result in battleResult.Results)
                        {
                            var deadMob = battleResult.IsAttackerDead ? battleResult.Attacker : result.Defenderer;
                            var target = enemies.Find(x => x.CharacterData.Name == deadMob);
                            BattleSystem.EnemyDeadProcess(target, result, partyData, battleResult.DropItems);
                        }
                    }
                }

                SaveDataCenter.SaveData(requestData.Account);
            }

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"讀取戰鬥狀態時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetBattleStatusResponse
            {
                Code = EErrorCode.GetBattleStatus,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class GetBattleStatusRequest : INetworkSerializable
{
    public string Account = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
    }
}

public class GetBattleStatusResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public Datas SaveData = new();
    public BattleResult BattleResult = new();
    public EffectResult EffectResult = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref BattleResult);
        serializer.SerializeValue(ref EffectResult);
    }
}