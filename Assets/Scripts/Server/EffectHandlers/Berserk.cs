public class Berserk : IEffectHandler
{
    public EEffectID ID => EEffectID.Berserk;

    public void Passive(FullAbilityBase baseAbility, EffectData effectData, FullAbilityBase afterAbility)
    {
        afterAbility.HP += baseAbility.HP * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.MP += baseAbility.MP * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.ATK += baseAbility.ATK * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.MATK += baseAbility.MATK * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.DEF += baseAbility.DEF * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.MDEF += baseAbility.MDEF * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.ACC += baseAbility.ACC * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.EVA += baseAbility.EVA * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.CRIT += baseAbility.CRIT * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
        afterAbility.SPD += baseAbility.SPD * CharacterDataCenter.ParamCalculate(baseAbility, effectData.Value);
    }

    public void Proc(CharacterData characterData, EffectData effectData, EffectResult.Result result)
    {
        effectData.Times--;

        if (effectData.Times == 0)
        {
            characterData.Effects.Remove(effectData);
            result.Infos.Add(new()
            {
                EffectName = effectData.Name,
                IsTimeUp = true,
            });
        }
    }
}