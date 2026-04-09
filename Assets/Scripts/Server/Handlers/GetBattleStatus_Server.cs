using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GetBattleStatus_Server : IApiHandler_Server
{
    public string Cmd => "GetBattleStatus";

    CharacterData CharacterData => GameData_Server.NowCharacterData;
    PlayerContextData PlayerData => GameData_Server.NowPlayerData;
    EnemyData EnemyData => GameData_Server.NowEnemyData;

    public string Get(object request)
    {
        try
        {
            BattleResult battleResult = null;
            if (EnemyData.Enemies.Count > 0)
            {
                battleResult = BattleSystem.CheckNowActor();

                if (battleResult != null && (battleResult.IsAttackerDead || battleResult.IsDefenderDead))
                {
                    if (battleResult.IsAttackerDead && CharacterData.Name == battleResult.Attacker ||
                        battleResult.IsDefenderDead && CharacterData.Name == battleResult.Defenderer)
                    {
                        OnLeave();
                    }
                    else
                    {
                        var deadMob = battleResult.IsAttackerDead ? battleResult.Attacker : battleResult.Defenderer;
                        var target = EnemyData.Enemies.Find(x => x.CharacterData.Name == deadMob);
                        BattleSystem.EnemyDeadProcess(target, battleResult);
                    }
                }

                SaveDataCenter.SaveData();
            }

            var responseData = new GetBattleStatus_ServerResponse
            {
                SaveData = GameData_Server.SaveData,
                BattleResult = battleResult
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
            var errorMessage = $"讀取戰鬥狀態時發生錯誤: {ex.Message}, {ex.StackTrace}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 4,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    void OnLeave()
    {
        PlayerData.Area = 1;
        PlayerData.Deep = 0;

        EnemyData.Enemies.Clear();

        CharacterDataCenter.InitCurrentData(CharacterData);
    }
}

public class GetBattleStatus_ServerResponse
{
    public SaveDataFormat SaveData;
    public BattleResult BattleResult;
}
