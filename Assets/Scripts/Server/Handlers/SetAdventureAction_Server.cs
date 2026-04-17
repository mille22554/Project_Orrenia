using System;
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
        var playerEffectResult = CharacterDataCenter.ActionEndProcess(CharacterData);
        actionResult.EffectResult.Results.Add(playerEffectResult);

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
            actionResult.BattleResult = OnEnemyAppear(actionResult.EffectResult);
        }

        return actionResult;
    }

    ActionResult OnRest()
    {
        var actionResult = new ActionResult
        {
            RestResult = new(),
            EffectResult = new()
        };

        var fullAbility = CharacterDataCenter.GetCharacterAbility(CharacterData);

        int prop;
        while (
            CharacterData.CurrentHP + actionResult.RestResult.RecoverHP < fullAbility.HP ||
            CharacterData.CurrentMP + actionResult.RestResult.RecoverMP < fullAbility.MP ||
            CharacterData.CurrentSTA + actionResult.RestResult.RecoverSTA < fullAbility.STA
        )
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

            var playerEffectResult = CharacterDataCenter.ActionEndProcess(CharacterData, true);
            actionResult.EffectResult.Results.Add(playerEffectResult);

            foreach (var info in playerEffectResult.Infos)
            {
                if (info.MofityAbility.HP < 0)
                    break;
            }

            prop = PublicFunc.Dice(1, 100);
            if (prop <= 3)
            {
                actionResult.BattleResult = OnEnemyAppear(actionResult.EffectResult);
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

    BattleResult OnEnemyAppear(EffectResult effectResult)
    {
        EnemyData.Enemies = EnemySetting.SetEnemy(
            PlayerData.Area,
            PlayerData.Deep
        );

        var (battleResult, nowActorEffectResult) = BattleSystem.CheckNowActor();
        if (battleResult != null && (battleResult.IsAttackerDead || battleResult.Results.Any(x => x.IsDefenderDead)))
        {
            if (battleResult.IsAttackerDead && CharacterData.Name == battleResult.Attacker ||
                battleResult.Results.Any(x => x.IsDefenderDead && CharacterData.Name == x.Defenderer))
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

        effectResult.Results.Add(nowActorEffectResult);
        return battleResult;
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
