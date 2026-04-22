using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GetDataBase_Server : IApiHandler_Server
{
    public string Cmd => "GetDataBase";

    public string Get(object request)
    {
        try
        {
            var responseData = new GetDataBase_ServerResponse
            {
                AreaData = AreaDataCenter.GetAllAreaData(),
                ItemData = ItemDataCenter_Server.ItemData,
                ItemKind = ItemDataCenter_Server.ItemKind,
                GameShopItem = ItemDataCenter_Server.GameShopItem,
                QualityData = ItemDataCenter_Server.QualityData,
                DamageTypes = SkillDataCenter.SkillTypes,
            };

            var response = new ResponseData_Server
            {
                Code = 0,
                Data = responseData
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"獲取資料庫時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 1,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }
}

public class GetDataBase_ServerResponse
{
    public Dictionary<int, AreaData> AreaData;
    public Dictionary<int, ItemData> ItemData;
    public Dictionary<EItemKind, ItemKind> ItemKind;
    public List<int> GameShopItem;
    public List<QualityData> QualityData;
    public Dictionary<ESkillType, string> DamageTypes;
}
