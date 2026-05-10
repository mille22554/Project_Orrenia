public class Counter : IEffectHandler
{
    public EEffectID ID => EEffectID.Counter;

    public void Passive(FullAbilityBase baseAbility, EffectData effectData, FullAbilityBase afterAbility)
    {

    }

    public void Proc(CharacterData characterData, EffectData effectData, EffectResult.Result result)
    {
        effectData.Times--;

        if (effectData.Times == 0)
        {
            SaveDataCenter.RemoveDataFromDB(EffectSave.Create(effectData));
            result.Infos.Add(new()
            {
                EffectName = effectData.Name,
                IsTimeUp = true,
            });
        }
        else
        {
            SaveDataCenter.SaveDataToDB(EffectSave.Create(effectData));
        }
    }
}