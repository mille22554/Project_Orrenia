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

            SaveDataCenter.SaveData();

            var responseData = new SetBattleAction_ServerResponse
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
            var errorMessage = $"設定戰鬥行動時發生錯誤: {ex.Message}, {ex.StackTrace}";
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
            case EBattleActionType.Attack:
                actionResult.BattleResult = OnAttack(data.ActionTarget);
                break;
            case EBattleActionType.Skill:
                OnSkill(data.ActionTarget, data.SkillID);
                break;
            case EBattleActionType.Leave:
                OnLeave();
                break;
        }
        return actionResult;
    }

    BattleResult OnAttack(CharacterData characterData)
    {
        var target = EnemyData.Enemies.Find(x => x.CharacterData.Name == characterData.Name);
        if (target != null)
        {
            var result = BattleSystem.RunBattle(CharacterData, target.CharacterData);

            if (result.IsDefenderDead)
            {
                BattleSystem.EnemyDeadProcess(target, result);
            }
            else if (result.IsAttackerDead)
            {
                OnLeave();
            }

            CharacterDataCenter.STAProcess(CharacterData, -1);
            return result;
        }
        else
        {
            Debug.LogError("戰鬥對象不存在!");
            return null;
        }
    }

    BattleResult OnSkill(CharacterData characterData, ESkillID skillID)
    {
        var skillData = SkillDataCenter.GetSkillData(skillID);
        if (CharacterData.CurrentMP < skillData.Cost)
        {
            Debug.Log("MP不足!");
            return null;
        }

        CharacterData.CurrentMP -= skillData.Cost;

        if (characterData.Name == CharacterData.Name)
        {

            CharacterDataCenter.STAProcess(CharacterData, -1);
            return null;
        }
        else
        {
            var target = EnemyData.Enemies.Find(x => x.CharacterData.Name == characterData.Name);
            if (target != null)
            {
                var result = BattleSystem.RunBattle(CharacterData, target.CharacterData, skillData);

                if (result.IsDefenderDead)
                {
                    BattleSystem.EnemyDeadProcess(target, result);
                }
                else if (result.IsAttackerDead)
                {
                    OnLeave();
                }

                CharacterDataCenter.STAProcess(CharacterData, -1);
                return result;
            }
            else
            {
                Debug.LogError("施法對象不存在!");
                return null;
            }
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

public class SetBattleAction_ServerRequest
{
    public EBattleActionType BattleAction;
    public CharacterData ActionTarget;
    public ESkillID SkillID;
}

public class SetBattleAction_ServerResponse
{
    public SaveDataFormat SaveData;
    public ActionResult ActionResult;
    public FullAbilityBase FullAbility;
}