// using System;
// using System.Collections;
// using System.IO;
// using System.Reflection;
// using Newtonsoft.Json;
// using UnityEngine;

// public class GetSaveData_Server : IApiHandler_Server
// {
//     public string Cmd => "GetSaveData";

//     string _account;
//     CharacterData CharacterData => GameData_Server.GetCharacterData(_account);
//     PlayerContextData PlayerData => GameData_Server.GetPlayerData(_account);
//     PartyData PartyData => GameData_Server.GetPartyData(_account);

//     public string Get(object request)
//     {
//         try
//         {
//             var requestData = JsonConvert.DeserializeObject<GetSaveData_ServerRequest>(request.ToString());
//             _account = requestData.Account;

//             string path = GameData_Server.PlayerSaveDataPath(_account);
//             Debug.Log($"從 {path} 讀取遊戲資料");

//             if (File.Exists(path))
//             {
//                 string json = File.ReadAllText(path);
//                 var data = JsonConvert.DeserializeObject<PlayerSaveDataFormat>(json);

//                 if (data.version != GameData_Server.version)
//                 {
//                     GameData_Server.NowPlayers[_account] = UpdateSaveData(data);
//                     SaveDataCenter.SaveData(_account);
//                 }
//                 else
//                 {
//                     GameData_Server.NowPlayers[_account] = data;
//                 }
//             }
//             else
//             {
//                 GameData_Server.NowPlayers[_account] = SaveDataCenter.CreateSaveData();
//                 CharacterDataCenter.InitCurrentData(CharacterData);
//                 PlayerData.NowPartyLeader = _account;
//                 PartyData.Leader = _account;
//                 PartyData.Members.Add(_account);
//             }

//             CheckFlags(_account);

//             var responseData = new GetSaveData_ServerResponse
//             {
//                 SaveData = GameData_Server.NowPlayers[_account].Datas,
//                 PartyData = GameData_Server.GetPartyData(PlayerData.NowPartyLeader),
//                 FullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData),
//                 AbilityPoint = PublicFunc.GetAbilityPoint(CharacterData),
//                 Exp = PublicFunc.GetExp(CharacterData.Level)
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
//             var errorMessage = $"讀取遊戲資料時發生錯誤: {ex.Message}, {ex.StackTrace}";
//             Debug.LogError(errorMessage);
//             var responseData = new ResponseData_Server
//             {
//                 Code = 1,
//                 Data = errorMessage
//             };
//             return JsonConvert.SerializeObject(responseData);
//         }
//     }

//     PlayerSaveDataFormat UpdateSaveData(PlayerSaveDataFormat oldData)
//     {
//         // Debug.Log("更新存檔資料結構");
//         var newData = SaveDataCenter.CreateSaveData();
//         newData.Datas.CharacterData.Name = oldData.Datas.CharacterData.Name;

//         CopyNonDefaultValues(oldData.Datas, newData.Datas);
//         return newData;
//     }

//     T CopyNonDefaultValues<T>(T oldData, T newData)
//     {
//         if (oldData == null || newData == null)
//             return newData;

//         foreach (var field in oldData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
//         {
//             object oldValue = field.GetValue(oldData);
//             if (oldValue == null)
//                 continue;

//             FieldInfo newField = newData.GetType().GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
//             // 新物件沒有這個欄位就跳過
//             if (newField == null)
//                 continue;

//             Type fieldType = field.FieldType;

//             // 判斷是否為自訂 class
//             if (!fieldType.IsPrimitive && fieldType != typeof(string))
//             {
//                 if (typeof(IList).IsAssignableFrom(fieldType))
//                 {
//                     var oldList = oldValue as IList;

//                     if (newField.GetValue(newData) is not IList newList)
//                     {
//                         newList = Activator.CreateInstance(fieldType) as IList;
//                         newField.SetValue(newData, newList);
//                     }

//                     newList.Clear();
//                     foreach (var item in oldList)
//                     {
//                         newList.Add(item);
//                     }
//                 }
//                 else
//                 {
//                     object newChild = newField.GetValue(newData);
//                     if (newChild == null)
//                     {
//                         newChild = Activator.CreateInstance(fieldType);
//                         newField.SetValue(newData, newChild);
//                     }
//                     CopyNonDefaultValues(oldValue, newChild);
//                 }
//             }
//             else
//             {
//                 // 基本型別直接賦值
//                 newField.SetValue(newData, oldValue);
//             }
//         }

//         // ====== 處理屬性 ======
//         foreach (var prop in oldData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
//         {
//             if (!prop.CanRead || !prop.CanWrite)
//                 continue; // 跳過唯讀屬性

//             object oldValue = prop.GetValue(oldData);
//             if (oldValue == null)
//                 continue;

//             Type propType = prop.PropertyType;

//             if (!propType.IsPrimitive && propType != typeof(string))
//             {
//                 if (typeof(IList).IsAssignableFrom(propType))
//                 {
//                     if (oldValue is not IList oldList)
//                         continue;

//                     if (prop.GetValue(newData) is not IList newList)
//                     {
//                         newList = Activator.CreateInstance(propType) as IList;
//                         prop.SetValue(newData, newList);
//                     }

//                     newList.Clear();
//                     foreach (var item in oldList)
//                         newList.Add(item);
//                 }
//                 else
//                 {
//                     object newChild = prop.GetValue(newData);
//                     if (newChild == null)
//                     {
//                         newChild = Activator.CreateInstance(propType);
//                         prop.SetValue(newData, newChild);
//                     }
//                     CopyNonDefaultValues(oldValue, newChild);
//                 }
//             }
//             else
//             {
//                 prop.SetValue(newData, oldValue);
//             }
//         }

//         return newData;
//     }

//     void CheckFlags(string account)
//     {
//         SaveDataCenter.SaveData(account);
//     }
// }

// public class GetSaveData_ServerRequest : ServerRequestBase { }

// public class GetSaveData_ServerResponse
// {
//     public Datas SaveData;
//     public PartyData PartyData;
//     public FullAbilityBase FullAbility;
//     public int AbilityPoint;
//     public int Exp;
// }
