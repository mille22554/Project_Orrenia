public class HP_Regen : IEffectHandler
{
    public int Type => 12;

    public void Passive(FullAbilityBase fullAbility, EffectData effectData)
    {

    }

    public void Proc(CharacterData characterData, EffectData effectData)
    {
        characterData.CurrentHP += effectData.Value;

        effectData.Times--;

        if (effectData.Times == 0)
            characterData.Effects.Remove(effectData);
    }
}