// using Newtonsoft.Json;

// public class GetBattleStatus : IApiHandler<GetBattleStatusResponse>
// {
//     public GetBattleStatusResponse Get(object response)
//     {
//         return JsonConvert.DeserializeObject<GetBattleStatusResponse>(response.ToString());
//     }
// }

// public class GetBattleStatusRequest : RequestBase<GetBattleStatusResponse>
// {
//     public override string Cmd => "GetBattleStatus";
// }

// public class GetBattleStatusResponse
// {
//     public PlayerSaveDataFormat SaveData;
//     public BattleResult BattleResult;
//     public EffectResult EffectResult;
// }
