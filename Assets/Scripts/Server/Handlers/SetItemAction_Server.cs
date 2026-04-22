using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class SetItemAction_Server : IApiHandler_Server
{
    public string Cmd => "SetItemAction";

    string _account;
    CharacterData CharacterData => GameData_Server.GetCharacterData(_account);

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetItemAction_ServerRequest>(request.ToString());
            _account = requestData.Account;

            var responseData = Do(requestData.BagItemData);

            SaveDataCenter.SaveData(requestData.Account);

            var response = new ResponseData_Server
            {
                Code = 0,
                Data = responseData
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"使用道具時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    SetItemAction_ServerResponse Do(BagItemData bagItemData)
    {
        var response = new SetItemAction_ServerResponse();

        var itemKind = ItemDataCenter_Server.GetItemKind(bagItemData.Kind);

        ItemDataCenter_Server.DoActionAccordingToCategory(bagItemData.Kind, EquipCallBack, UseCallBack, null);

        if (CharacterData.CurrentTP >= GameData_Server.tpCost)
            CharacterData.CurrentTP -= GameData_Server.tpCost;

        response.ItemCategory = itemKind.Category;
        response.BagItemData = bagItemData;
        response.Enemies = GameData_Server.GetPartyData(GameData_Server.GetPlayerData(_account).NowPartyLeader).Enemies;
        response.CharacterData = CharacterData;
        response.FullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData);

        return response;

        void EquipCallBack()
        {
            var equips = CharacterData.Equips;

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
                    var equip = CharacterData.BagItems.Find(x => x.UID == equipUID);
                    var equipCategory = ItemDataCenter_Server.GetItemKind(equip.Kind).Category;

                    if (equipCategory == itemKind.Category || ItemDataCenter_Server.IsWeapon(equipCategory) || equip.Kind == EItemKind.Shield)
                    {
                        if (equip.Kind == EItemKind.Ring && ringCounter < 9)
                        {
                            ringCounter++;
                            continue;
                        }
                        else if (CharacterData.Skills.ContainsKey(ESkillID.雙持) && itemKind.Category == EItemCategory.One_Hand && dualWieldCounter < 1)
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
            bagItemData = CharacterData.BagItems.Find(x => x.UID == bagItemData.UID);

            CharacterDataCenter.MotifyCurrentAbility(CharacterData, bagItemData.Ability);

            if (bagItemData.Effects != null)
            {
                foreach (var effect in bagItemData.Effects)
                {
                    Debug.Log($"ID: {effect.ID}, Value: {effect.Value}, Times: {effect.Times}");
                    CharacterDataCenter.AddCharacterEffect(CharacterData, effect);
                }
            }

            if (bagItemData.Skill != ESkillID.None)
            {
                if (CharacterData.Skills.ContainsKey(bagItemData.Skill))
                    return;

                CharacterData.Skills.Add(bagItemData.Skill, SkillDataCenter.GetSkillData(bagItemData.Skill));
            }

            bagItemData.Count--;

            if (bagItemData.Count == 0)
                CharacterData.BagItems.Remove(bagItemData);
        }
    }
}

public class SetItemAction_ServerRequest : ServerRequestBase
{
    public BagItemData BagItemData;
}

public class SetItemAction_ServerResponse
{
    public EItemCategory ItemCategory;
    public BagItemData BagItemData;
    public List<BagItemData> UnEquiped = new();
    public bool IsEquipped;
    public List<MobData> Enemies;
    public CharacterData CharacterData;
    public FullAbilityBase FullAbility;
}
