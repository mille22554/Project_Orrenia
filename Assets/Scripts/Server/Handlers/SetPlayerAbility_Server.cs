// using System;
// using System.Collections.Generic;
// using Newtonsoft.Json;
// using UnityEngine;

// public class SetPlayerAbility_Server : IApiHandler_Server
// {
//     public string Cmd => "SetPlayerAbility";

//     string _account;
//     CharacterData CharacterData => GameData_Server.GetCharacterData(_account);

//     public string Get(object request)
//     {
//         try
//         {
//             var requestData = JsonConvert.DeserializeObject<SetPlayerAbility_ServerRequest>(request.ToString());
//             _account = requestData.Account;

//             var ability = requestData.Ability;
//             var abilityPoint = PublicFunc.GetAbilityPoint(CharacterData);

//             if (abilityPoint >= 0)
//             {
//                 CharacterData.Ability = ability;
//                 SaveDataCenter.SaveData(requestData.Account);
//             }

//             var response = new ResponseData_Server
//             {
//                 Code = 0,
//             };
//             return JsonConvert.SerializeObject(response);
//         }
//         catch (Exception ex)
//         {
//             var errorMessage = $"設定玩家能力值時發生錯誤: {ex.Message}, {ex.StackTrace}";
//             Debug.LogError(errorMessage);
//             var responseData = new ResponseData_Server
//             {
//                 Code = 3,
//                 Data = errorMessage
//             };
//             return JsonConvert.SerializeObject(responseData);
//         }
//     }
// }

// public class SetPlayerAbility_ServerRequest : ServerRequestBase
// {
//     public AbilityBase Ability;
// }
