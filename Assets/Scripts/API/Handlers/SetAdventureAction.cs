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
    const int enemyProp = 100;
    #region Client

    public void Send(SetAdventureActionRequest requestData) => Send(requestData, null);
    public void Send(SetAdventureActionRequest requestData, Action<SetAdventureActionResponse> callback)
    {
        _all_OnceListeners[typeof(SetAdventureActionResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetAdventureActionResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetAdventureActionResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetAdventureActionResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetAdventureActionResponse>)_all_OnceListeners[typeof(SetAdventureActionResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetAdventureActionResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetAdventureActionRequest requestData, RpcParams rpcParams = default)
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

    SetAdventureActionResponse Main(SetAdventureActionRequest requestData)
    {
        try
        {
            var account = requestData.Account.ToString();
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = new SetAdventureActionResponse
            {
                Code = EErrorCode.None,
            };

            if (account == playerData.NowPartyLeader)
            {
                responseData.IsLeader = true;
                responseData.Datas = GameData_Server.NowPlayers[account].Datas;
                responseData.PartyData = partyData;
                responseData.FullAbility = CharacterDataCenter.GetCharacterAbility(characterData);

                DoAction(requestData, responseData.ActionResult, partyData, characterData);

                SaveDataCenter.SaveData(account);
            }

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"讀取遊戲資料時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetAdventureActionResponse
            {
                Code = EErrorCode.SetAdventureAction,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }

    void DoAction(SetAdventureActionRequest data, ActionResult actionResult, PartyData partyData, CharacterData characterData)
    {
        switch (data.AdventureAction)
        {
            case EAdventureActionType.IntoArea:
                OnIntoArea(data, partyData);
                break;
            case EAdventureActionType.GoAhead:
                OnGoAhead(characterData, partyData, actionResult);
                break;
            case EAdventureActionType.Rest:
                OnRest(characterData, partyData, actionResult);
                break;
            case EAdventureActionType.Leave:
                OnGoHome(partyData);
                break;
        }
    }

    void OnIntoArea(SetAdventureActionRequest data, PartyData partyData)
    {
        partyData.Area = data.GameArea;
        partyData.Deep = 1;
    }

    void OnGoAhead(CharacterData characterData, PartyData partyData, ActionResult actionResult)
    {
        ActionEndProcess(partyData, false, actionResult.EffectResult.Results);

        if (!partyData.Members.Any(x => GameData_Server.GetCharacterData(x).CurrentHP > 0))
        {
            OnGoHome(partyData);
            return;
        }

        partyData.Deep++;

        var prop = PublicFunc.Dice(1, 100);
        if (prop <= enemyProp)
        {
            OnEnemyAppear(partyData);
        }
    }

    void OnRest(CharacterData characterData, PartyData partyData, ActionResult actionResult)
    {
        var fullAbility = CharacterDataCenter.GetCharacterAbility(characterData);

        int prop;
        while (characterData.CurrentHP < fullAbility.HP || characterData.CurrentMP < fullAbility.MP || characterData.CurrentSTA < fullAbility.STA)
        {
            if (characterData.CurrentHP < fullAbility.HP)
            {
                actionResult.RestResult.RecoverHP++;
                characterData.CurrentHP++;
            }

            if (characterData.CurrentMP < fullAbility.MP)
            {
                actionResult.RestResult.RecoverMP++;
                characterData.CurrentMP++;
            }

            if (characterData.CurrentSTA < fullAbility.STA)
            {
                actionResult.RestResult.RecoverSTA++;
                characterData.CurrentSTA++;
            }

            ActionEndProcess(partyData, true, actionResult.EffectResult.Results);

            prop = PublicFunc.Dice(1, 100);
            if (prop <= 3)
            {
                OnEnemyAppear(partyData);
                break;
            }
        }
    }

    void OnEnemyAppear(PartyData partyData)
    {
        BattleSystem.InitNewBattle(partyData);
    }

    void ActionEndProcess(PartyData partyData, bool isRest, List<EffectResult.Result> results)
    {
        foreach (var member in partyData.Members)
        {
            var characterData = GameData_Server.GetCharacterData(member);
            var playerEffectResult = CharacterDataCenter.ActionEndProcess(characterData, isRest);
            if (playerEffectResult.Infos.Count > 0)
                results.Add(playerEffectResult);
        }
    }
    #endregion
}

public class SetAdventureActionRequest : INetworkSerializable
{
    public string Account = "";
    public EAdventureActionType AdventureAction;
    public int GameArea;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref AdventureAction);
        serializer.SerializeValue(ref GameArea);
    }
}

public class SetAdventureActionResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public bool IsLeader;
    public Datas Datas = new();
    public PartyData PartyData = new();
    public ActionResult ActionResult = new();
    public FullAbilityBase FullAbility = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref IsLeader);
        serializer.SerializeValue(ref Datas);
        serializer.SerializeValue(ref PartyData);
        serializer.SerializeValue(ref ActionResult);
        serializer.SerializeValue(ref FullAbility);
    }
}