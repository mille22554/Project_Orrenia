using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GetBattleStatus_Server : IApiHandler_Server
{
    public string Cmd => "GetBattleStatus";

    string _account;
    CharacterData CharacterData => GameData_Server.GetCharacterData(_account);
    PlayerContextData PlayerData => GameData_Server.GetPlayerData(_account);
    PartyData PartyData => GameData_Server.GetPartyData(PlayerData.NowPartyLeader);

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<GetBattleStatus_ServerRequest>(request.ToString());
            _account = requestData.Account;

            BattleResult battleResult = null;
            var effectResult = new EffectResult();
            var enemies = PartyData.Enemies;
            if (enemies.Count > 0)
            {
                EffectResult.Result nowActorEffectResult;
                (battleResult, nowActorEffectResult) = BattleSystem.CheckNowActor(PartyData);
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
                            var target = enemies.Find(x => x.CharacterData.Name == deadMob);
                            BattleSystem.EnemyDeadProcess(target, result, PartyData, battleResult.DropItems);
                        }
                    }
                }

                SaveDataCenter.SaveData(requestData.Account);
            }

            var responseData = new GetBattleStatus_ServerResponse
            {
                SaveData = GameData_Server.NowPlayers[requestData.Account],
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
        PartyData.Area = 1;
        PartyData.Deep = 0;

        PartyData.Enemies.Clear();
        CharacterData.Effects.Clear();

        CharacterDataCenter.InitCurrentData(CharacterData);
    }
}

public class GetBattleStatus_ServerRequest : ServerRequestBase { }

public class GetBattleStatus_ServerResponse
{
    public PlayerSaveDataFormat SaveData;
    public BattleResult BattleResult;
    public EffectResult EffectResult;
}
