// using System.Collections.Generic;
// using Newtonsoft.Json;

// public class SetBattleAction : IApiHandler<SetBattleActionResponse>
// {
//     public SetBattleActionResponse Get(object response)
//     {
//         return JsonConvert.DeserializeObject<SetBattleActionResponse>(response.ToString());
//     }
// }

// public class SetBattleActionRequest : RequestBase<SetBattleActionResponse>
// {
//     public override string Cmd =>"SetBattleAction";
//     public EBattleActionType BattleAction;
//     public List<CharacterData> ActionTarget;
//     public ESkillID SkillID;
// }

// public class SetBattleActionResponse
// {
//     public PlayerSaveDataFormat SaveData;
//     public ActionResult ActionResult;
//     public FullAbilityBase FullAbility;
// }
