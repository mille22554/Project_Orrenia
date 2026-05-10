using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public partial class APIController : NetworkBehaviour
{
    public static APIController Ins { get; private set; }

    readonly static Dictionary<Type, List<CallbackWrapper>> _allListeners = new();
    readonly static Dictionary<Type, object> _all_OnceListeners = new();

    void Awake()
    {
        Ins = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void AddListener<T>(MonoBehaviour owner, Action<T> callback)
    {
        var type = typeof(T);
        if (!_allListeners.ContainsKey(type)) _allListeners[type] = new List<CallbackWrapper>();

        _allListeners[type].Add(new CallbackWrapper { Owner = owner, Callback = callback });
    }

    void OnGoHome(PartyData partyData)
    {
        partyData.Area = 1;
        partyData.Deep = 0;

        foreach (var mob in SaveDataCenter.GetMobs(partyData.UID))
            SaveDataCenter.RemoveMob(mob.UID);

        foreach (var member in SaveDataCenter.GetPartyMembers(partyData.UID))
        {
            CharacterDataCenter.InitCurrentData(member);
            SaveDataCenter.SaveDataToDB(CharacterSave.Create(member));
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
    CheckIsNewAccount,
    GetPlayerInfo,
    GetAdventureInfo,
    GetCharacterInfo,
    GetBagInfo,
    GetPlayerSkill,
    GetPartyEffects,
}