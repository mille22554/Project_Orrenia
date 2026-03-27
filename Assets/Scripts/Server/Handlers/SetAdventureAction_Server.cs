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
            var actionResult = DoAction(requestData);

            SaveDataCenter.SaveData();

            var responseData = new SetAdventureAction_ServerResponse
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

    ActionResult DoAction(SetAdventureAction_ServerRequest data)
    {
        var actionResult = new ActionResult();

        switch (data.AdventureAction)
        {
            case EAdventureActionType.IntoArea:
                OnIntoArea(data);
                break;
            case EAdventureActionType.GoAhead:
                actionResult = OnGoAhead();
                break;
            case EAdventureActionType.Rest:
                actionResult = OnRest();
                break;
            case EAdventureActionType.Leave:
                OnLeave();
                break;
        }
        return actionResult;
    }

    void OnIntoArea(SetAdventureAction_ServerRequest data)
    {
        PlayerData.Area = data.GameArea;
        PlayerData.Deep = 1;
    }

    ActionResult OnGoAhead()
    {
        var actionResult = new ActionResult();

        CharacterData.CurrentSTA--;
        CharacterDataCenter.EffectProcess(CharacterData);

        PlayerData.Deep++;

        var prop = PublicFunc.Dice(1, 100);
        if (prop <= enemyProp)
        {
            actionResult.BattleResult = OnEnemyAppear();
        }

        return actionResult;
    }

    ActionResult OnRest()
    {
        var actionResult = new ActionResult
        {
            RestResult = new RestResult()
        };

        var fullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData);

        int prop;
        while (
            CharacterData.CurrentHP + actionResult.RestResult.RecoverHP < fullAbility.HP ||
            CharacterData.CurrentMP + actionResult.RestResult.RecoverMP < fullAbility.MP ||
            CharacterData.CurrentSTA + actionResult.RestResult.RecoverSTA < fullAbility.STA
        )
        {
            if (CharacterData.CurrentHP + actionResult.RestResult.RecoverHP < fullAbility.HP)
                actionResult.RestResult.RecoverHP++;

            if (CharacterData.CurrentMP + actionResult.RestResult.RecoverMP < fullAbility.MP)
                actionResult.RestResult.RecoverMP++;

            if (CharacterData.CurrentSTA + actionResult.RestResult.RecoverSTA < fullAbility.STA)
                actionResult.RestResult.RecoverSTA++;

            prop = PublicFunc.Dice(1, 100);
            if (prop <= 3)
            {
                actionResult.BattleResult = OnEnemyAppear();
                break;
            }
        }

        CharacterData.CurrentHP += actionResult.RestResult.RecoverHP;
        CharacterData.CurrentMP += actionResult.RestResult.RecoverMP;
        CharacterData.CurrentSTA += actionResult.RestResult.RecoverSTA;

        return actionResult;
    }

    void OnLeave()
    {
        PlayerData.Area = 1;
        PlayerData.Deep = 0;

        EnemyData.Enemies.Clear();

        CharacterDataCenter.InitCurrentData(CharacterData);
    }

    BattleResult OnEnemyAppear()
    {
        EnemyData.Enemies = EnemySetting.SetEnemy(
            PlayerData.Area,
            PlayerData.Deep
        );

        var result = BattleSystem.CheckNowActor();
        if (result != null)
        {
            var target = EnemyData.Enemies.Find(x => x.CharacterData.Name == result.Attacker);

            if (result.IsDefenderDead)
            {
                OnLeave();
            }
            else if (result.IsAttackerDead)
            {
                BattleSystem.EnemyDeadProcess(target, result);
            }
        }

        return result;
    }
}

public class SetAdventureAction_ServerRequest
{
    public EAdventureActionType AdventureAction;
    public int GameArea;
}

public class SetAdventureAction_ServerResponse
{
    public SaveDataFormat SaveData;
    public ActionResult ActionResult;
}
