using System.Collections.Generic;
using System.Reflection;

public static class CharacterDataCenter
{
    public static CharacterData InitCurrentData(CharacterData data)
    {
        var fullAbility = GetCharacterAbility(data);
        data.CurrentHP = fullAbility.HP;
        data.CurrentMP = fullAbility.MP;
        data.CurrentSTA = fullAbility.STA;

        return data;
    }

    public static FullAbilityBase GetCharacterAbility(CharacterData data)
    {
        var ability = new FullAbilityBase
        {
            STR = data.Ability.STR_Point,
            VIT = data.Ability.VIT_Point,
            DEX = data.Ability.DEX_Point,
            INT = data.Ability.INT_Point,
            AGI = data.Ability.AGI_Point,
            LUK = data.Ability.LUK_Point
        };

        ability.HP = ability.VIT * 10 + ability.STR * 5 + 85;
        ability.MP = ability.INT * 10 + ability.VIT * 5 + 35;
        ability.STA = ability.VIT * 5 + 95;
        ability.ATK = ability.STR * 2 + ability.VIT;
        ability.MATK = ability.INT * 2 + ability.VIT;
        ability.DEF = ability.VIT * 2 + ability.STR;
        ability.MDEF = ability.VIT * 2 + ability.INT;
        ability.ACC = ability.AGI * 3 + ability.DEX * 2 + ability.LUK;
        ability.EVA = ability.DEX * 3 + ability.AGI * 2 + ability.LUK;
        ability.CRIT = ability.AGI * 2 + ability.LUK;
        ability.SPD = ability.DEX;

        if (data.Role == ECharacterRole.Mob)
            ability.HP /= 10;

        ability = CalculateEquipAbility(ability, data.Equips);
        ability = CalculateEffectAbility(ability, data.Effects);

        return ability;
    }

    public static FullAbilityBase CalculateEquipAbility(FullAbilityBase data, List<long> equips)
    {
        foreach (var equipUID in equips)
        {
            var item = GameData_Server.NowBagData.Items.Find(x => x.UID == equipUID);
            var ability = ItemDataCenter_Server.GetItemData(item.ItemID).Ability;
            var fields = typeof(FullAbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                int valueB = (int)field.GetValue(ability);
                if (valueB != 0)
                {
                    int valueA = (int)field.GetValue(data);
                    field.SetValue(data, valueA + valueB);
                }
            }
        }

        return data;
    }

    public static FullAbilityBase CalculateEffectAbility(FullAbilityBase data, List<EffectData> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.type)
            {
                case EffectType.Buff.HP_UP:
                    data.HP *= effect.value;
                    break;

                case EffectType.Buff.Berserk:
                    data.HP *= effect.value;
                    data.MP *= effect.value;
                    data.ATK *= effect.value;
                    data.MATK *= effect.value;
                    data.DEF *= effect.value;
                    data.MDEF *= effect.value;
                    data.ACC *= effect.value;
                    data.EVA *= effect.value;
                    data.CRIT *= effect.value;
                    data.SPD *= effect.value;
                    break;

                case EffectType.Debuff.Exhausted:
                    data.HP /= effect.value;
                    data.MP /= effect.value;
                    data.ATK /= effect.value;
                    data.MATK /= effect.value;
                    data.DEF /= effect.value;
                    data.MDEF /= effect.value;
                    data.ACC /= effect.value;
                    data.EVA /= effect.value;
                    data.CRIT /= effect.value;
                    data.SPD /= effect.value;
                    break;
            }
        }
        return data;
    }

    public static void EffectProcess(CharacterData characterData)
    {
        foreach (var effect in characterData.Effects)
        {
            switch (effect.type)
            {
                case EffectType.Buff.HP_Regen:
                    characterData.CurrentHP += effect.value;
                    break;
            }

            effect.times--;
            if (effect.times <= 0)
                characterData.Effects.Remove(effect);
        }
    }
}