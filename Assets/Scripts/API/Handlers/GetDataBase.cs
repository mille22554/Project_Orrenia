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

    public void Send(GetDataBaseRequest requestData) => Send(requestData, null);
    public void Send(GetDataBaseRequest requestData, Action<GetDataBaseResponse> callback)
    {
        _all_OnceListeners[typeof(GetDataBaseResponse)] = callback;
        ExecuteCommandServerRpc(requestData);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ReturnResponseClientRpc(GetDataBaseResponse responseData, RpcParams rpcParams = default)
    {
        // 這段就會回到 Client 端執行了
        Debug.Log($"收: {JsonConvert.SerializeObject(responseData)}");

        if (_allListeners.TryGetValue(typeof(GetDataBaseResponse), out var callbacks))
        {
            // 從後往前跑，方便在迴圈中直接刪除已失效的物件
            for (int i = callbacks.Count - 1; i >= 0; i--)
            {
                if (callbacks[i].IsValid)
                    ((Action<GetDataBaseResponse>)callbacks[i].Callback).Invoke(responseData);
                else
                    callbacks.RemoveAt(i); // 自動清理已銷毀的物件
            }
        }

        ((Action<GetDataBaseResponse>)_all_OnceListeners[typeof(GetDataBaseResponse)])?.Invoke(responseData);
        _all_OnceListeners[typeof(GetDataBaseResponse)] = null;
    }
    #endregion

    #region Server

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void ExecuteCommandServerRpc(GetDataBaseRequest requestData, RpcParams rpcParams = default)
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

    GetDataBaseResponse Main(GetDataBaseRequest requestData)
    {
        try
        {
            var responseData = new GetDataBaseResponse
            {
                Code = EErrorCode.None,
                AreaData = AreaDataCenter.GetAllAreaData(),
                ItemData = ItemDataCenter_Server.ItemData,
                ItemKind = ItemDataCenter_Server.ItemKind,
                GameShopItem = ItemDataCenter_Server.GameShopItem,
                QualityData = ItemDataCenter_Server.QualityData,
                DamageTypes = SkillDataCenter.SkillTypes,
            };
            return responseData;
        }
        catch (Exception ex)
        {
            var errorMessage = $"獲取資料庫時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new GetDataBaseResponse
            {
                Code = EErrorCode.GetDataBase,
                ErrorMessage = errorMessage
            };
            return responseData;
        }
    }
    #endregion
}

public class GetDataBaseRequest : INetworkSerializable
{
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
    }
}

public class GetDataBaseResponse : INetworkSerializable
{
    public EErrorCode Code;
    public string ErrorMessage = "";
    public Dictionary<int, AreaData> AreaData = new();
    public Dictionary<int, ItemData> ItemData = new();
    public Dictionary<EItemKind, ItemKind> ItemKind = new();
    public List<int> GameShopItem = new();
    public List<QualityData> QualityData = new();
    public Dictionary<ESkillType, string> DamageTypes = new();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Code);
        serializer.SerializeValue(ref ErrorMessage);

        PublicFunc.SerializeList(serializer, ref GameShopItem);
        PublicFunc.SerializeClassList(serializer, ref QualityData);
        PublicFunc.SerializeClassDictionary(serializer, ref AreaData);
        PublicFunc.SerializeClassDictionary(serializer, ref ItemData);
        PublicFunc.SerializeEnum_ClassDictionary(serializer, ref ItemKind);
        PublicFunc.SerializeEnum_StringDict(serializer, ref DamageTypes);
    }
}