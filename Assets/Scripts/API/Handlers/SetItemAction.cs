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

    public void Send(SetItemActionRequest requestData) => Send(requestData, null);
    public void Send(SetItemActionRequest requestData, Action<SetItemActionResponse> callback)
    {
        Debug.Log($"送: {JsonConvert.SerializeObject(requestData)}");
        _all_OnceListeners[typeof(SetItemActionResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(SetItemActionResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(SetItemActionResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<SetItemActionResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<SetItemActionResponse>)_all_OnceListeners[typeof(SetItemActionResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(SetItemActionResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(SetItemActionRequest requestData, RpcParams rpcParams = default)
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

    SetItemActionResponse Main(SetItemActionRequest requestData)
    {
        try
        {
            var account = requestData.Account.ToString();
            var characterData = GameData_Server.GetCharacterData(account);
            var playerData = GameData_Server.GetPlayerData(account);
            var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

            var responseData = Do(requestData.BagItemData, characterData, partyData);

            SaveDataCenter.SaveData(account);

            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"使用道具時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new SetItemActionResponse
            {
                Code = EErrorCode.SetItemAction,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }

    SetItemActionResponse Do(BagItemData bagItemData, CharacterData characterData, PartyData partyData)
    {
        var response = new SetItemActionResponse
        {
            Code = EErrorCode.None,
        };

        var itemKind = ItemDataCenter_Server.GetItemKind(bagItemData.Kind);

        ItemDataCenter_Server.DoActionAccordingToCategory(bagItemData.Kind, EquipCallBack, UseCallBack, null);

        if (characterData.CurrentTP >= GameData_Server.tpCost)
            characterData.CurrentTP -= GameData_Server.tpCost;

        response.ItemCategory = itemKind.Category;
        response.BagItemData = bagItemData;
        response.Enemies = partyData.Enemies;
        response.CharacterData = characterData;
        response.FullAbility = CharacterDataCenter.GetCharacterAbility(characterData);

        return response;

        void EquipCallBack()
        {
            var equips = characterData.Equips;

            if (equips.Contains(bagItemData.UID))
            {
                equips.Remove(bagItemData.UID);
                response.IsEquipped = true;
            }
            else
            {
                var ringCounter = 0;
                var dualWieldCounter = 0;
                var twoHandCounter = 0;

                foreach (var equipUID in equips.ToList())
                {
                    var equip = characterData.BagItems.Find(x => x.UID == equipUID);
                    var equipCategory = ItemDataCenter_Server.GetItemKind(equip.Kind).Category;

                    if (equipCategory == itemKind.Category || ItemDataCenter_Server.IsWeapon(equipCategory) || equip.Kind == EItemKind.Shield)
                    {
                        if (equip.Kind == EItemKind.Ring && ringCounter < 9)
                        {
                            ringCounter++;
                            continue;
                        }
                        else if (characterData.Skills.ContainsKey(ESkillID.雙持) && itemKind.Category == EItemCategory.One_Hand && dualWieldCounter < 1)
                        {
                            if (equipCategory != EItemCategory.Two_Hand)
                            {
                                dualWieldCounter++;
                                continue;
                            }
                        }

                        equips.Remove(equip.UID);
                        response.UnEquiped.Add(equip);

                        if (itemKind.Category == EItemCategory.Two_Hand && twoHandCounter < 1)
                        {
                            twoHandCounter++;
                            continue;
                        }

                        break;
                    }
                }

                equips.Add(bagItemData.UID);
                response.IsEquipped = false;
            }
        }

        void UseCallBack()
        {
            bagItemData = characterData.BagItems.Find(x => x.UID == bagItemData.UID);

            CharacterDataCenter.MotifyCurrentAbility(characterData, bagItemData.Ability);

            if (bagItemData.Effects != null)
            {
                foreach (var effect in bagItemData.Effects)
                {
                    Debug.Log($"ID: {effect.ID}, Value: {effect.Value}, Times: {effect.Times}");
                    CharacterDataCenter.AddCharacterEffect(characterData, effect);
                }
            }

            if (bagItemData.Skill != ESkillID.None)
            {
                if (characterData.Skills.ContainsKey(bagItemData.Skill))
                    return;

                characterData.Skills.Add(bagItemData.Skill, SkillDataCenter.GetSkillData(bagItemData.Skill));
            }

            bagItemData.Count--;

            if (bagItemData.Count == 0)
                characterData.BagItems.Remove(bagItemData);
        }
    }

    #endregion
}

public class SetItemActionRequest : INetworkSerializable
{
    public string Account = "";
    public BagItemData BagItemData = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
        serializer.SerializeValue(ref BagItemData);
    }
}

public class SetItemActionResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public EItemCategory ItemCategory = new();
    public BagItemData BagItemData = new();
    public List<BagItemData> UnEquiped = new();
    public bool IsEquipped;
    public List<MobData> Enemies = new();
    public CharacterData CharacterData = new();
    public FullAbilityBase FullAbility = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref ItemCategory);
        serializer.SerializeValue(ref BagItemData);
        serializer.SerializeValue(ref IsEquipped);
        serializer.SerializeValue(ref CharacterData);
        serializer.SerializeValue(ref FullAbility);

        PublicFunc.SerializeClassList(serializer, ref UnEquiped);
        PublicFunc.SerializeClassList(serializer, ref Enemies);
    }
}