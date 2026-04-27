// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Newtonsoft.Json;
// using UnityEngine;

// public class SetBattleAction_Server : IApiHandler_Server
// {
//     public string Cmd => "SetBattleAction";

//     string _account;
//     PlayerContextData PlayerData => GameData_Server.GetPlayerData(_account);
//     CharacterData CharacterData => GameData_Server.GetCharacterData(_account);
//     PartyData PartyData => GameData_Server.GetPartyData(PlayerData.NowPartyLeader);

//     public string Get(object request)
//     {
//         try
//         {
//             var requestData = JsonConvert.DeserializeObject<SetBattleAction_ServerRequest>(request.ToString());
//             _account = requestData.Account;

//             var actionResult = DoAction(requestData);

//             SaveDataCenter.SaveData(requestData.Account);

//             var responseData = new SetBattleAction_ServerResponse
//             {
//                 SaveData = GameData_Server.NowPlayers[requestData.Account],
//                 ActionResult = actionResult,
//                 FullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData)
//             };
//             var response = new ResponseData_Server
//             {
//                 Code = 0,
//                 Data = responseData
//             };
//             return JsonConvert.SerializeObject(response);
//         }
//         catch (Exception ex)
//         {
//             var errorMessage = $"設定戰鬥行動時發生錯誤: {ex.Message}, {ex.StackTrace}";
//             Debug.LogError(errorMessage);
//             var responseData = new ResponseData_Server
//             {
//                 Code = 2,
//                 Data = errorMessage
//             };
//             return JsonConvert.SerializeObject(responseData);
//         }
//     }

//     ActionResult DoAction(SetBattleAction_ServerRequest data)
//     {
//         var actionResult = new ActionResult();

//         if (CharacterData.CurrentTP >= GameData_Server.tpCost)
//         {
//             switch (data.BattleAction)
//             {
//                 case EBattleActionType.Attack:
//                     (actionResult.BattleResult, actionResult.EffectResult) = OnAttack(data.ActionTarget);
//                     break;
//                 case EBattleActionType.Skill:
//                     (actionResult.BattleResult, actionResult.EffectResult) = OnSkill(data.ActionTarget, data.SkillID);
//                     break;
//                 case EBattleActionType.Leave:
//                     OnLeave(true);
//                     break;
//             }
//         }
//         return actionResult;
//     }

//     (BattleResult battleResult, EffectResult effectResult) OnAttack(List<CharacterData> characterData)
//     {
//         var targets = PartyData.Enemies.Where(x => characterData.Any(y => x.CharacterData.Name == y.Name)).ToList();
//         if (targets != null)
//         {
//             var battleResult = BattleSystem.RunBattle(CharacterData, targets.Select(x => x.CharacterData).ToList());

//             foreach (var result in battleResult.Results)
//             {
//                 if (result.IsDefenderDead)
//                     BattleSystem.EnemyDeadProcess(targets.Find(x => x.CharacterData.Name == result.Defenderer), result, PartyData, battleResult.DropItems);
//             }

//             if (battleResult.IsAttackerDead)
//                 OnLeave();

//             var effectResult = new EffectResult();
//             var playerEffectResult = CharacterDataCenter.ActionEndProcess(CharacterData);

//             effectResult.Results.Add(playerEffectResult);

//             if (playerEffectResult.IsDead)
//             {
//                 CharacterData.Effects.Clear();
//                 OnLeave();
//             }

//             return (battleResult, effectResult);
//         }
//         else
//         {
//             Debug.LogError("戰鬥對象不存在!");
//             return (null, null);
//         }
//     }

//     (BattleResult battleResult, EffectResult effectResult) OnSkill(List<CharacterData> skillTargets, ESkillID skillID)
//     {
//         var skillData = SkillDataCenter.GetSkillData(skillID);
//         if (CharacterData.CurrentMP < skillData.Cost)
//         {
//             Debug.Log("MP不足!");
//             return (null, null);
//         }

//         CharacterData.CurrentMP -= skillData.Cost;

//         if (skillTargets.All(x => x.Name == CharacterData.Name))
//         {
//             CharacterDataCenter.ActionEndProcess(CharacterData);

//             return (null, null);
//         }
//         else
//         {
//             var targets = PartyData.Enemies.Where(x => skillTargets.Any(y => x.CharacterData.Name == y.Name)).ToList();
//             if (targets != null)
//             {
//                 var battleResult = BattleSystem.RunBattle(CharacterData, targets.Select(x => x.CharacterData).ToList(), skillData);

//                 foreach (var result in battleResult.Results)
//                 {
//                     if (result.IsDefenderDead)
//                         BattleSystem.EnemyDeadProcess(targets.Find(x => x.CharacterData.Name == result.Defenderer), result, PartyData, battleResult.DropItems);
//                 }

//                 if (battleResult.IsAttackerDead)
//                     OnLeave();

//                 var effectResult = new EffectResult();
//                 var playerEffectResult = CharacterDataCenter.ActionEndProcess(CharacterData);
//                 effectResult.Results.Add(playerEffectResult);
//                 if (playerEffectResult.IsDead)
//                 {
//                     CharacterData.Effects.Clear();
//                     OnLeave();
//                 }

//                 return (battleResult, effectResult);
//             }
//             else
//             {
//                 Debug.LogError("施法對象不存在!");
//                 return (null, null);
//             }
//         }
//     }

//     void OnLeave() => OnLeave(false);
//     void OnLeave(bool isNotDead)
//     {
//         PartyData.Area = 1;
//         PartyData.Deep = 0;

//         PartyData.Enemies.Clear();
//         if (!isNotDead)
//             CharacterData.Effects.Clear();

//         CharacterDataCenter.InitCurrentData(CharacterData);
//     }
// }

// public class SetBattleAction_ServerRequest : ServerRequestBase
// {
//     public EBattleActionType BattleAction;
//     public List<CharacterData> ActionTarget;
//     public ESkillID SkillID;
// }

// public class SetBattleAction_ServerResponse
// {
//     public PlayerSaveDataFormat SaveData;
//     public ActionResult ActionResult;
//     public FullAbilityBase FullAbility;
// }