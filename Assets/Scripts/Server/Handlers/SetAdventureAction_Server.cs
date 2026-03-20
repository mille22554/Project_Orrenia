using System;
using Newtonsoft.Json;
using UnityEngine;

public class SetAdventureAction_Server : IApiHandler_Server
{
    public string Cmd => "SetAdventureAction";

    PlayerContextData PlayerData => GameData_Server.NowPlayerData;
    CharacterData CharacterData => GameData_Server.NowCharacterData;
    EnemyData EnemyData => GameData_Server.NowEnemyData;

    const int enemyProp = 100;

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetAdventureAction_ServerRequest>(request.ToString());
            DoAction(requestData);

            PublicFunc.SaveData();

            var responseData = new SetAdventureAction_ServerResponse
            {
                SaveData = GameData_Server.SaveData
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
            var errorMessage = $"設定一般行動時發生錯誤: {ex.Message}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    void DoAction(SetAdventureAction_ServerRequest data)
    {
        switch (data.AdventureAction)
        {
            case AdventureActionType.IntoArea:
                OnIntoArea(data);
                break;
            case AdventureActionType.GoAhead:
                OnGoAhead();
                break;
            case AdventureActionType.Leave:
                OnLeave();
                break;
        }
    }

    void OnIntoArea(SetAdventureAction_ServerRequest data)
    {
        PlayerData.Area = data.GameArea;
        PlayerData.Deep = 1;
    }

    void OnGoAhead()
    {
        CharacterData.CurrentSTA--;
        PublicFunc.EffectProcess();

        PlayerData.Deep++;

        var prop = PublicFunc.Dice(1, 100);
        if (prop < enemyProp)
        {
            OnEnemyAppear();
        }
    }

    void OnLeave()
    {
        PlayerData.Area = 1;
        PlayerData.Deep = 0;

        EnemyData.enemies.Clear();

        PublicFunc.InitCurrentData(CharacterData);
    }

    void OnEnemyAppear()
    {
        EnemyData.enemies = EnemySetting.SetEnemy(
            PlayerData.Area,
            PlayerData.Deep
        );
    }
}

public class SetAdventureAction_ServerRequest
{
    public AdventureActionType AdventureAction;
    public int GameArea;
}

public class SetAdventureAction_ServerResponse
{
    public GameSaveData SaveData;
}
