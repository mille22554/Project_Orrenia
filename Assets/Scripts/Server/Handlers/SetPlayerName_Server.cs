using System;
using Newtonsoft.Json;
using UnityEngine;

public class SetPlayerName_Server : IApiHandler_Server
{
    public string Cmd => "SetPlayerName";

    string _account;
    CharacterData CharacterData => GameData_Server.GetCharacterData(_account);
    PartyData PartyData => GameData_Server.GetPartyData(_account);

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetPlayerName_ServerRequest>(request.ToString());
            _account = requestData.Account;

            CharacterData.Name = requestData.PlayerName;

            SaveDataCenter.SaveData(requestData.Account);

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

public class SetPlayerName_ServerRequest : ServerRequestBase
{
    public string PlayerName;
}
