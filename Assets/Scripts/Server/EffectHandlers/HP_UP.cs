public class HP_UP : IEffectHandler
{
    public int Type => 1;

    public void Passive(FullAbilityBase fullAbility, EffectData effectData)
    {
        fullAbility.HP *= effectData.Value;
    }

    public void Proc(CharacterData characterData, EffectData effectData)
    {
        effectData.Times--;

        if (effectData.Times == 0)
            characterData.Effects.Remove(effectData);
    }
}