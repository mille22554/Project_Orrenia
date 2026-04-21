using System;
using System.Collections.Generic;
using System.Linq;
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
                ActionResult = actionResult,
                FullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData)
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
            var errorMessage = $"設定一般行動時發生錯誤: {ex.Message}, {ex.StackTrace}";
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
        var playerEffectResult = ActionEndProcess(CharacterData, false, actionResult.EffectResult.Results);

        if (playerEffectResult.IsDead)
        {
            CharacterData.Effects.Clear();
            OnLeave();
            return actionResult;
        }

        PlayerData.Deep++;

        var prop = PublicFunc.Dice(1, 100);
        if (prop <= enemyProp)
        {
            OnEnemyAppear(actionResult.EffectResult);
            // actionResult.BattleResult = OnEnemyAppear(actionResult.EffectResult);
        }

        return actionResult;
    }

    ActionResult OnRest()
    {
        var actionResult = new ActionResult();

        var fullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData);

        int prop;
        while (CharacterData.CurrentHP < fullAbility.HP || CharacterData.CurrentMP < fullAbility.MP || CharacterData.CurrentSTA < fullAbility.STA)
        {
            if (CharacterData.CurrentHP < fullAbility.HP)
            {
                actionResult.RestResult.RecoverHP++;
                CharacterData.CurrentHP++;
            }

            if (CharacterData.CurrentMP < fullAbility.MP)
            {
                actionResult.RestResult.RecoverMP++;
                CharacterData.CurrentMP++;
            }

            if (CharacterData.CurrentSTA < fullAbility.STA)
            {
                actionResult.RestResult.RecoverSTA++;
                CharacterData.CurrentSTA++;
            }

            var playerEffectResult = ActionEndProcess(CharacterData, true, actionResult.EffectResult.Results);
            foreach (var info in playerEffectResult.Infos)
            {
                if (info.MofityAbility.HP < 0)
                    break;
            }

            prop = PublicFunc.Dice(1, 100);
            if (prop <= 3)
            {
                OnEnemyAppear(actionResult.EffectResult);
                // actionResult.BattleResult = OnEnemyAppear(actionResult.EffectResult);
                break;
            }
        }

        return actionResult;
    }

    void OnLeave()
    {
        PlayerData.Area = 1;
        PlayerData.Deep = 0;

        EnemyData.Enemies.Clear();

        CharacterDataCenter.InitCurrentData(CharacterData);
    }

    void OnEnemyAppear(EffectResult effectResult)
    {
        EnemyData.Enemies = EnemySetting.SetEnemy(
            PlayerData.Area,
            PlayerData.Deep
        );

        BattleSystem.InitNewBattle(new() { CharacterData });
    }

    EffectResult.Result ActionEndProcess(CharacterData characterData, bool isRest, List<EffectResult.Result> results)
    {
        var playerEffectResult = CharacterDataCenter.ActionEndProcess(characterData, isRest);
        if (playerEffectResult.Infos.Count > 0)
            results.Add(playerEffectResult);

        return playerEffectResult;
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
    public FullAbilityBase FullAbility;
}
