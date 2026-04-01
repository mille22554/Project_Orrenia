using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class GetItemData_Server : IApiHandler_Server
{
    public string Cmd => "GetItemData";

    public string Get(object request)
    {
        try
        {
            var responseData = new GetItemData_ServerResponse
            {
                ItemData = ItemDataCenter_Server.ItemData,
                ItemKind = ItemDataCenter_Server.ItemKind,
                GameShopItem = ItemDataCenter_Server.GameShopItem
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
            var errorMessage = $"獲取道具資料時發生錯誤: {ex.Message}, {ex.StackTrace}";
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

public class GetItemData_ServerResponse
{
    public Dictionary<int, ItemData> ItemData;
    public Dictionary<EItemKind, ItemKind> ItemKind;
    public List<int> GameShopItem;
}
