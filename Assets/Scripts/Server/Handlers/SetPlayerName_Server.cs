using System;
using Newtonsoft.Json;
using UnityEngine;

public class SetPlayerName_Server : IApiHandler_Server
{
    public string Cmd => "SetPlayerName";

    CharacterData CharacterData => GameData_Server.NowCharacterData;

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetPlayerName_ServerRequest>(request.ToString());

            CharacterData.Name = requestData.PlayerName;
            SaveDataCenter.SaveData();

            var response = new ResponseData_Server
            {
                Code = 0,
            };
            return JsonConvert.SerializeObject(response);
        }
        catch (Exception ex)
        {
            var errorMessage = $"設定玩家名稱時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }
}

public class SetPlayerName_ServerRequest
{
    public string PlayerName;
}
