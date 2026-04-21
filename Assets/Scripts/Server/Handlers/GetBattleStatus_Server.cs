using System;
using System.IO;
using System.Linq;
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
            var effectResult = new EffectResult();
            if (EnemyData.Enemies.Count > 0)
            {
                EffectResult.Result nowActorEffectResult;
                (battleResult, nowActorEffectResult) = BattleSystem.CheckNowActor();
                effectResult.Results.Add(nowActorEffectResult);

                if (battleResult != null && (battleResult.IsAttackerDead || battleResult.Results.Any(x => x.IsDefenderDead)))
                {
                    var playerDeadAtDefence = battleResult.Results.Any(x => x.IsDefenderDead && x.Defenderer == CharacterData.Name);

                    if (battleResult.IsAttackerDead && CharacterData.Name == battleResult.Attacker || playerDeadAtDefence)
                    {
                        OnLeave();
                    }
                    else
                    {
                        foreach (var result in battleResult.Results)
                        {
                            var deadMob = battleResult.IsAttackerDead ? battleResult.Attacker : result.Defenderer;
                            var target = EnemyData.Enemies.Find(x => x.CharacterData.Name == deadMob);
                            BattleSystem.EnemyDeadProcess(target, result, battleResult.DropItems);
                        }
                    }
                }

                SaveDataCenter.SaveData();
            }

            var responseData = new GetBattleStatus_ServerResponse
            {
                SaveData = GameData_Server.SaveData,
                BattleResult = battleResult,
                EffectResult = effectResult,
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
        CharacterData.Effects.Clear();

        CharacterDataCenter.InitCurrentData(CharacterData);
    }
}

public class GetBattleStatus_ServerResponse
{
    public SaveDataFormat SaveData;
    public BattleResult BattleResult;
    public EffectResult EffectResult;
}
