using System;
using Newtonsoft.Json;
using UnityEngine;

public class SetBattleAction_Server : IApiHandler_Server
{
    public string Cmd => "SetBattleAction";

    PlayerContextData PlayerData => GameData_Server.NowPlayerData;
    CharacterData CharacterData => GameData_Server.NowCharacterData;
    EnemyData EnemyData => GameData_Server.NowEnemyData;

    public string Get(object request)
    {
        try
        {
            var requestData = JsonConvert.DeserializeObject<SetBattleAction_ServerRequest>(request.ToString());
            var actionResult = DoAction(requestData);

            PublicFunc.SaveData();

            var responseData = new SetBattleAction_ServerResponse
            {
                SaveData = GameData_Server.SaveData,
                ActionResult = actionResult
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
            var errorMessage = $"設定戰鬥行動時發生錯誤: {ex.Message}";
            Debug.LogError(errorMessage);
            var responseData = new ResponseData_Server
            {
                Code = 2,
                Data = errorMessage
            };
            return JsonConvert.SerializeObject(responseData);
        }
    }

    ActionResult DoAction(SetBattleAction_ServerRequest data)
    {
        var actionResult = new ActionResult();
        switch (data.BattleAction)
        {
            case BattleActionType.Attack:
                actionResult.BattleResult = OnAttack(data.AttackTarget);
                break;
            case BattleActionType.Leave:
                OnLeave();
                break;
        }
        return actionResult;
    }

    BattleResult OnAttack(MobData mob)
    {
        CharacterData.CurrentSTA--;
        PublicFunc.EffectProcess();

        var target = EnemyData.enemies.Find(x => x.CharacterData.Name == mob.CharacterData.Name);
        if (target != null)
        {
            return BattleSystem.RunBattle(CharacterData, target.CharacterData);
        }
        else
        {
            Debug.LogError("戰鬥對象不存在!");
            return null;
        }
    }

    void OnLeave()
    {
        PlayerData.Area = 1;
        PlayerData.Deep = 0;

        EnemyData.enemies.Clear();

        PublicFunc.InitCurrentData(CharacterData);
    }
}

public class SetBattleAction_ServerRequest
{
    public BattleActionType BattleAction;
    public MobData AttackTarget;
}

public class SetBattleAction_ServerResponse
{
    public GameSaveData SaveData;
    public ActionResult ActionResult;
}