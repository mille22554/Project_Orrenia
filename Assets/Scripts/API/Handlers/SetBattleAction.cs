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

    public void Send(SetBattleActionRequest requestData) => Send(requestData, null);
    public void Send(SetBattleActionRequest requestData, Action<SetBattleActionResponse> callback)
    {
        _all_OnceListeners[typeof(SetBattleActionResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetBattleActionResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetBattleActionResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetBattleActionResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetBattleActionResponse>)_all_OnceListeners[typeof(SetBattleActionResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetBattleActionResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetBattleActionRequest requestData, RpcParams rpcParams = default)
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

    SetBattleActionResponse Main(SetBattleActionRequest requestData)
    {
        try
        {
            var account = requestData.Account.ToString();
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = new SetBattleActionResponse
            {
                Code = EErrorCode.None,
                SaveData = GameData_Server.NowPlayers[account].Datas,
                FullAbility = CharacterDataCenter.GetCharacterAbility(characterData)
            };

            DoAction(requestData, characterData, partyData, responseData.ActionResult);

            SaveDataCenter.SaveData(account);

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定戰鬥行動時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetBattleActionResponse
            {
                Code = EErrorCode.SetBattleAction,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }

    void DoAction(SetBattleActionRequest data, CharacterData characterData, PartyData partyData, ActionResult actionResult)
    {
        if (characterData.CurrentTP >= GameData_Server.tpCost)
        {
            switch (data.BattleAction)
            {
                case EBattleActionType.Attack:
                    OnAttack(data.ActionTarget, characterData, partyData, actionResult);
                    break;
                case EBattleActionType.Skill:
                    OnSkill(data.ActionTarget, data.SkillID, characterData, partyData, actionResult);
                    break;
                case EBattleActionType.Leave:
                    OnGoHome(partyData);
                    break;
            }
        }
    }

    void OnAttack(List<CharacterData> actionTarget, CharacterData characterData, PartyData partyData, ActionResult actionResult)
    {
        var targets = partyData.Enemies.Where(x => actionTarget.Any(y => x.CharacterData.Name == y.Name)).ToList();
        if (targets != null)
        {
            var battleResult = actionResult.BattleResult = BattleSystem.RunBattle(characterData, targets.Select(x => x.CharacterData).ToList());

            foreach (var result in battleResult.Results)
            {
                if (result.IsDefenderDead)
                    BattleSystem.EnemyDeadProcess(targets.Find(x => x.CharacterData.Name == result.Defenderer), result, partyData, battleResult.DropItems);
            }

            actionResult.EffectResult.Results.Add(CharacterDataCenter.ActionEndProcess(characterData));

            if (!partyData.Members.Any(x => GameData_Server.GetCharacterData(x).CurrentHP > 0))
                OnGoHome(partyData);
        }
        else
        {
            Debug.LogError("戰鬥對象不存在!");
        }
    }

    void OnSkill(List<CharacterData> skillTargets, ESkillID skillID, CharacterData characterData, PartyData partyData, ActionResult actionResult)
    {
        var skillData = SkillDataCenter.GetSkillData(skillID);
        if (characterData.CurrentMP < skillData.Cost)
        {
            Debug.Log("MP不足!");
        }

        characterData.CurrentMP -= skillData.Cost;

        if (skillTargets.All(x => x.Name == characterData.Name))
        {
            CharacterDataCenter.ActionEndProcess(characterData);
        }
        else
        {
            var targets = partyData.Enemies.Where(x => skillTargets.Any(y => x.CharacterData.Name == y.Name)).ToList();
            if (targets != null)
            {
                var battleResult = BattleSystem.RunBattle(characterData, targets.Select(x => x.CharacterData).ToList(), skillData);

                foreach (var result in battleResult.Results)
                {
                    if (result.IsDefenderDead)
                        BattleSystem.EnemyDeadProcess(targets.Find(x => x.CharacterData.Name == result.Defenderer), result, partyData, battleResult.DropItems);
                }

                actionResult.EffectResult.Results.Add(CharacterDataCenter.ActionEndProcess(characterData));

                if (!partyData.Members.Any(x => GameData_Server.GetCharacterData(x).CurrentHP > 0))
                    OnGoHome(partyData);
            }
            else
            {
                Debug.LogError("施法對象不存在!");
            }
        }
    }
    #endregion
}

public class SetBattleActionRequest : INetworkSerializable
{
    public string Account = "";
    public EBattleActionType BattleAction;
    public List<CharacterData> ActionTarget = new();
    public ESkillID SkillID;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        var actionTarget = ActionTarget.ToArray();

        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref BattleAction);
        serializer.SerializeValue(ref actionTarget);
        serializer.SerializeValue(ref SkillID);
    }
}

public class SetBattleActionResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public Datas SaveData = new();
    public ActionResult ActionResult = new();
    public FullAbilityBase FullAbility = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref ActionResult);
        serializer.SerializeValue(ref FullAbility);
    }
}