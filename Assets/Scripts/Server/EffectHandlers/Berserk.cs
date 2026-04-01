public class Berserk : IEffectHandler
{
    public int Type => 22;

    public void Passive(FullAbilityBase fullAbility, EffectData effectData)
    {
        fullAbility.HP *= effectData.Value;
        fullAbility.MP *= effectData.Value;
        fullAbility.ATK *= effectData.Value;
        fullAbility.MATK *= effectData.Value;
        fullAbility.DEF *= effectData.Value;
        fullAbility.MDEF *= effectData.Value;
        fullAbility.ACC *= effectData.Value;
        fullAbility.EVA *= effectData.Value;
        fullAbility.CRIT *= effectData.Value;
        fullAbility.SPD *= effectData.Value;
    }

    public void Proc(CharacterData characterData, EffectData effectData)
    {
        effectData.Times--;

        if (effectData.Times == 0)
            characterData.Effects.Remove(effectData);
    }
}