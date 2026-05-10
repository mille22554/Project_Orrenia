public class HP_UP : IEffectHandler
{
    public EEffectID ID => EEffectID.HP_UP;

    public void Passive(FullAbilityBase baseAbility, EffectData effectData, FullAbilityBase afterAbility)
    {
        baseAbility.HP += baseAbility.HP * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
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