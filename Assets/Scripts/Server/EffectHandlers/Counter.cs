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
            characterData.Effects.Remove(effectData);
            result.Infos.Add(new()
            {
                EffectName = effectData.Name,
                IsTimeUp = true,
            });
        }
    }
}