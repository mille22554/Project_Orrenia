// using Newtonsoft.Json;

// public class GetSaveData_old : IApiHandler<GetSaveDataResponse_old>
// {
//     public GetSaveDataResponse_old Get(object response)
//     {
//         return JsonConvert.DeserializeObject<GetSaveDataResponse_old>(response.ToString());
//     }
// }

// public class GetSaveDataRequest_old : RequestBase<GetSaveDataResponse_old>
// {
//     public override string Cmd =>"GetSaveData";
// }

// public class GetSaveDataResponse_old
// {
//     public Datas SaveData;
//     public PartyData PartyData;
//     public FullAbilityBase FullAbility;
//     public int AbilityPoint;
//     public int Exp;
// }
