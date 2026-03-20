using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GetBattleStatus_Server : IApiHandler_Server
{
    public string Cmd => "GetBattleStatus";

    public string Get(object request)
    {
        try
        {


            var response = new ResponseData_Server
            {
                Code = 0,
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"讀取戰鬥狀態時發生錯誤: {ex.Message}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 4,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }
}

public class GetBattleStatus_ServerResponse
{
    
}
