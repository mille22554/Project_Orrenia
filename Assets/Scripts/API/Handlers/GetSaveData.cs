using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public partial class APIController
{
    #region Client

    public void Send(GetSaveDataRequest requestData) => Send(requestData, null);
    public void Send(GetSaveDataRequest requestData, Action<GetSaveDataResponse> callback)
    {
        _all_OnceListeners[typeof(GetSaveDataResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetSaveDataResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetSaveDataResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetSaveDataResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetSaveDataResponse>)_all_OnceListeners[typeof(GetSaveDataResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetSaveDataResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetSaveDataRequest requestData, RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        // Debug.Log(clientId);

        var returnParams = new RpcParams
        {
            Send = new()
            {
                // 注意！這裡的欄位名稱是 Target，而不是 TargetClientIds
                Target = RpcTarget.Single(clientId, RpcTargetUse.Temp)
            }
        };

        ReturnResponseClientRpc(Main(requestData), returnParams);
    }

    GetSaveDataResponse Main(GetSaveDataRequest requestData)
    {
        try
        {
            var account = requestData.Account.ToString();

            string path = GameData_Server.PlayerSaveDataPath(account);
            Debug.Log($"從 {path} 讀取遊戲資料");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<PlayerSaveDataFormat>(json);

                if (data.version != GameData_Server.version)
                {
                    GameData_Server.NowPlayers[account] = UpdateSaveData(data);
                    SaveDataCenter.SaveData(account);
                }
                else
                {
                    GameData_Server.NowPlayers[account] = data;
                }
            }
            else
            {
                GameData_Server.NowPlayers[account] = SaveDataCenter.CreateSaveData();
                CharacterDataCenter.InitCurrentData(GameData_Server.GetCharacterData(account));

                var playerData = GameData_Server.GetPlayerData(account);

                playerData.NowPartyLeader = account;
                var partyData = GameData_Server.GetPartyData(playerData.NowPartyLeader);

                partyData.Leader = account;
                partyData.Members.Add(account);
            }

            CheckFlags(account);

            var responseData = new GetSaveDataResponse
            {
                Code = EErrorCode.None,
                SaveData = GameData_Server.NowPlayers[account].Datas,
                PartyData = GameData_Server.GetPartyData(GameData_Server.GetPlayerData(account).NowPartyLeader),
                FullAbility = CharacterDataCenter.GetCharacterAbility(GameData_Server.GetCharacterData(account)),
                AbilityPoint = PublicFunc.GetAbilityPoint(GameData_Server.GetCharacterData(account)),
                Exp = PublicFunc.GetExp(GameData_Server.GetCharacterData(account).Level)
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"讀取遊戲資料時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetSaveDataResponse
            {
                Code = EErrorCode.GetSaveData,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }

    PlayerSaveDataFormat UpdateSaveData(PlayerSaveDataFormat oldData)
    {
        // Debug.Log("更新存檔資料結構");
        var newData = SaveDataCenter.CreateSaveData();
        newData.Datas.CharacterData.Name = oldData.Datas.CharacterData.Name;

        CopyNonDefaultValues(oldData.Datas, newData.Datas);
        return newData;
    }

    T CopyNonDefaultValues<T>(T oldData, T newData)
    {
        if (oldData == null || newData == null)
            return newData;

        foreach (var field in oldData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object oldValue = field.GetValue(oldData);
            if (oldValue == null)
                continue;

            FieldInfo newField = newData.GetType().GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
            // 新物件沒有這個欄位就跳過
            if (newField == null)
                continue;

            Type fieldType = field.FieldType;

            // 判斷是否為自訂 class
            if (!fieldType.IsPrimitive && fieldType != typeof(string))
            {
                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    var oldList = oldValue as IList;

                    if (newField.GetValue(newData) is not IList newList)
                    {
                        newList = Activator.CreateInstance(fieldType) as IList;
                        newField.SetValue(newData, newList);
                    }

                    newList.Clear();
                    foreach (var item in oldList)
                    {
                        newList.Add(item);
                    }
                }
                else
                {
                    object newChild = newField.GetValue(newData);
                    if (newChild == null)
                    {
                        newChild = Activator.CreateInstance(fieldType);
                        newField.SetValue(newData, newChild);
                    }
                    CopyNonDefaultValues(oldValue, newChild);
                }
            }
            else
            {
                // 基本型別直接賦值
                newField.SetValue(newData, oldValue);
            }
        }

        // ====== 處理屬性 ======
        foreach (var prop in oldData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite)
                continue; // 跳過唯讀屬性

            object oldValue = prop.GetValue(oldData);
            if (oldValue == null)
                continue;

            Type propType = prop.PropertyType;

            if (!propType.IsPrimitive && propType != typeof(string))
            {
                if (typeof(IList).IsAssignableFrom(propType))
                {
                    if (oldValue is not IList oldList)
                        continue;

                    if (prop.GetValue(newData) is not IList newList)
                    {
                        newList = Activator.CreateInstance(propType) as IList;
                        prop.SetValue(newData, newList);
                    }

                    newList.Clear();
                    foreach (var item in oldList)
                        newList.Add(item);
                }
                else
                {
                    object newChild = prop.GetValue(newData);
                    if (newChild == null)
                    {
                        newChild = Activator.CreateInstance(propType);
                        prop.SetValue(newData, newChild);
                    }
                    CopyNonDefaultValues(oldValue, newChild);
                }
            }
            else
            {
                prop.SetValue(newData, oldValue);
            }
        }

        return newData;
    }

    void CheckFlags(string account)
    {
        SaveDataCenter.SaveData(account);
    }
    #endregion
}

public class GetSaveDataRequest : INetworkSerializable
{
    public string Account = "";

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Account);
    }
}

public class GetSaveDataResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public Datas SaveData = new();
    public PartyData PartyData = new();
    public FullAbilityBase FullAbility = new();
    public int AbilityPoint;
    public int Exp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);
        serializer.SerializeValue(ref SaveData);
        serializer.SerializeValue(ref PartyData);
        serializer.SerializeValue(ref FullAbility);
        serializer.SerializeValue(ref AbilityPoint);
        serializer.SerializeValue(ref Exp);
    }
}