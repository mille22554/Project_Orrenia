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

    SetAdventureActionResponse Main(SetAdventureActionRequest requestData)
    {
        try
        {
            var uid = requestData.UID;
            var playerData = SaveDataCenter.GetPlayerData(uid);
            var partyData = SaveDataCenter.GetPartyData(playerData.PartyUID);
            var characterData = SaveDataCenter.GetCharacterData(uid);

            var responseData = new SetAdventureActionResponse
            {
                Code = EErrorCode.None,
            };

            if (playerData.UID == playerData.PartyUID)
            {
                responseData.IsLeader = true;
                responseData.PartyData = partyData;
                responseData.PlayerData = playerData;
                responseData.CharacterData = characterData;
                responseData.FullAbility = CharacterDataCenter.GetCharacterAbility(characterData);

                DoAction(requestData, responseData.ActionResult, partyData);

                // SaveDataCenter.SaveData(account);
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

    void DoAction(SetAdventureActionRequest data, ActionResult actionResult, PartyData partyData)
    {
        switch (data.AdventureAction)
        {
            case EAdventureActionType.IntoArea:
                OnIntoArea(data, partyData);
                break;
            case EAdventureActionType.GoAhead:
                OnGoAhead(partyData, actionResult);
                break;
            case EAdventureActionType.Rest:
                OnRest(partyData, actionResult);
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

    void OnGoAhead(PartyData partyData, ActionResult actionResult)
    {
        foreach (var member in SaveDataCenter.GetPartyMembers(partyData.UID))
            ActionEndProcess(member, false, actionResult.EffectResult.Results);

        if (!SaveDataCenter.GetPartyMembers(partyData.UID).Any(x => x.CurrentHP > 0))
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

    void OnRest(PartyData partyData, ActionResult actionResult)
    {
        var members = SaveDataCenter.GetPartyMembers(partyData.UID);
        var restResult = actionResult.RestResult;

        while (true)
        {
            // 1. 找出第一個需要治療的成員，同時取得他的能力值
            var target = members
                .Select(m => new { Data = m, Max = CharacterDataCenter.GetCharacterAbility(m) })
                .FirstOrDefault(x => x.Data.CurrentHP < x.Max.HP || x.Data.CurrentMP < x.Max.MP || x.Data.CurrentSTA < x.Max.STA);

            // 如果沒人需要治療，跳出迴圈
            if (target == null)
                break;

            var m = target.Data;
            var max = target.Max;

            if (m.CurrentHP < max.HP)
                m.CurrentHP++; restResult.RecoverHP++;

            if (m.CurrentMP < max.MP)
                m.CurrentMP++; restResult.RecoverMP++;

            if (m.CurrentSTA < max.STA)
                m.CurrentSTA++; restResult.RecoverSTA++;

            ActionEndProcess(m, true, actionResult.EffectResult.Results);

            // 2. 機率性遇敵
            if (PublicFunc.Dice(1, 100) <= 3)
            {
                OnEnemyAppear(partyData);
                break;
            }
        }

        foreach (var member in members)
            SaveDataCenter.SaveDataToDB(CharacterSave.Create(member));
    }

    void OnEnemyAppear(PartyData partyData)
    {
        BattleSystem.InitNewBattle(partyData);
    }

    void ActionEndProcess(CharacterData characterData, bool isRest, List<EffectResult.Result> results)
    {
        var playerEffectResult = CharacterDataCenter.ActionEndProcess(characterData, isRest);
        if (playerEffectResult.Infos.Count > 0)
            results.Add(playerEffectResult);
    }
    #endregion
}

public class SetAdventureActionRequest : INetworkSerializable
{
    public long UID;
    public EAdventureActionType AdventureAction;
    public int GameArea;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref UID);
        serializer.SerializeValue(ref AdventureAction);
        serializer.SerializeValue(ref GameArea);
    }
}

public class SetAdventureActionResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public bool IsLeader;
    public PlayerData PlayerData = new();
    public CharacterData CharacterData = new();
    public PartyData PartyData = new();
    public List<CharacterData> Enemies = new();
    public ActionResult ActionResult = new();
    public FullAbilityBase FullAbility = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref IsLeader);
        serializer.SerializeValue(ref PlayerData);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref PartyData);
        serializer.SerializeValue(ref ActionResult);
        serializer.SerializeValue(ref FullAbility);

        PublicFunc.SerializeClassList(serializer, ref Enemies);
    }
}