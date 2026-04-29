using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class APIController : NetworkBehaviour
{
    public static APIController Ins { get; private set; }

    readonly Dictionary<Type, List<CallbackWrapper>> _allListeners = new();
    readonly Dictionary<Type, object> _all_OnceListeners = new();

    void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddListener<T>(MonoBehaviour owner, Action<T> callback)
    {
        var type = typeof(T);
        if (!_allListeners.ContainsKey(type)) _allListeners[type] = new List<CallbackWrapper>();

        _allListeners[type].Add(new CallbackWrapper { Owner = owner, Callback = callback });
    }

    void OnGoHome(PartyData partyData)
    {
        partyData.Area = 1;
        partyData.Deep = 0;

        partyData.Enemies.Clear();

        foreach (var member in partyData.Members)
        {
            var characterData = GameData_Server.GetCharacterData(member);
            CharacterDataCenter.InitCurrentData(characterData);
        }
    }
}

class CallbackWrapper
{
    public MonoBehaviour Owner;
    public object Callback; // 這裡存 Action<T>

    public bool IsValid => Owner != null && Owner.gameObject != null;
}

public enum EErrorCode
{
    None,
    GetSaveData,
    GetBattleStatus,
    SetItemAction,
    SetPlayerName,
    SetAdventureAction,
    SetBattleAction,
    GetDataBase,
    SetTradeAction,
    SetPlayerAbility,
    SetForgeAction,
    InitDataBase,
}