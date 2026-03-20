using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GetAreaData_Server : IApiHandler_Server
{
    public string Cmd => "GetAreaData";

    public string Get(object request)
    {
        try
        {
            var responseData = new GetAreaData_ServerResponse
            {
                AreaData = AreaDataCenter.GetAllAreaData()
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
            var errorMessage = $"獲取區域資料時發生錯誤: {ex.Message}";
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

public class GetAreaData_ServerResponse
{
    public Dictionary<int, AreaData> AreaData;
}
