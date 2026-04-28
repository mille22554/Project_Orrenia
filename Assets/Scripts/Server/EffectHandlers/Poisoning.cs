public class Poisoning : IEffectHandler
{
    public EEffectID ID => EEffectID.Poisoning;

    public void Passive(FullAbilityBase baseAbility, EffectData effectData, FullAbilityBase afterAbility)
    {

    }

    public void Proc(CharacterData characterData, EffectData effectData, EffectResult.Result result)
    {
        var info = new EffectResult.Result.Info()
        {
            EffectName = effectData.Name,
            MofityAbility = new()
            {
                HP = -effectData.Times
            }
        };
        result.Infos.Add(info);

        CharacterData.ChangeHP(characterData, info.MofityAbility.HP);

        effectData.Times--;

        if (effectData.Times == 0)
        {
            characterData.Effects.Remove(effectData);
            info.IsTimeUp = true;
        }
    }
}