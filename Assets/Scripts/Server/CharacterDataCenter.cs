using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public static class CharacterDataCenter
{
    static Dictionary<EEffectID, EffectData> _effectDatas;
    readonly static Dictionary<int, IEffectHandler> _effectHandlers = new();

    static CharacterDataCenter()
    {
        LoadEffectData();
        RegisterHandlers();
    }

    static void LoadEffectData()
    {
        var path = GameData_Server.EffectDataPath;
        Debug.Log($"從 {path} 讀取效果類別資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            _effectDatas = JsonConvert.DeserializeObject<Dictionary<EEffectID, EffectData>>(json);
            foreach (var effectData in _effectDatas)
            {
                effectData.Value.ID = effectData.Key;
            }
        }
        else
        {
            Debug.LogError("EffectData檔案丟失!");
        }
    }

    static void RegisterHandlers()
    {
        var types = typeof(CharacterDataCenter).Assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IEffectHandler).IsAssignableFrom(type))
                continue;

            var instance = (IEffectHandler)Activator.CreateInstance(type);

            Debug.Log($"Register effect handler: {instance.Type} -> {type.Name}");

            _effectHandlers[instance.Type] = instance;
        }
    }

    public static CharacterData InitCurrentData(CharacterData data)
    {

        var fullAbility = GetCharacterAbility(data);
        data.CurrentHP = fullAbility.HP;
        data.CurrentMP = fullAbility.MP;
        STAProcess(data, fullAbility.STA);

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

        CalculateEquipAbility(ability, data.Equips);
        CalculateEffectAbility(ability, data.Effects);

        FixAbility(data, ability);

        return ability;
    }

    static void CalculateEquipAbility(FullAbilityBase data, List<long> equips)
    {
        foreach (var equipUID in equips)
        {
            var item = GameData_Server.NowBagData.Items.Find(x => x.UID == equipUID);
            var ability = ItemDataCenter_Server.FinalAbilityProcess(item);
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
    }

    static void CalculateEffectAbility(FullAbilityBase data, List<EffectData> effects)
    {
        foreach (var effect in effects)
        {
            if (_effectHandlers.TryGetValue(effect.Type, out var effectHandler))
            {
                effectHandler.Passive(data, effect);
            }
        }
    }

    static void FixAbility(CharacterData data, FullAbilityBase ability)
    {
        if (data.CurrentHP > ability.HP)
            data.CurrentHP = ability.HP;

        if (data.CurrentMP > ability.MP)
            data.CurrentMP = ability.MP;

        if (ability.LUK < 1)
            ability.LUK = 1;

        if (ability.ATK < 1)
            ability.ATK = 1;

        if (ability.MATK < 1)
            ability.MATK = 1;

        if (ability.DEF < 1)
            ability.DEF = 1;

        if (ability.MDEF < 1)
            ability.MDEF = 1;

        if (ability.ACC < 1)
            ability.ACC = 1;

        if (ability.EVA < 1)
            ability.EVA = 1;

        if (ability.CRIT < 1)
            ability.CRIT = 1;

        if (ability.SPD < 1)
            ability.SPD = 1;

    }

    public static void EffectProcess(CharacterData characterData)
    {
        foreach (var effect in characterData.Effects.ToList())
        {
            if (_effectHandlers.TryGetValue(effect.Type, out var effectHandler))
            {
                effectHandler.Proc(characterData, effect);
            }
        }
    }

    public static void STAProcess(CharacterData characterData, int value)
    {
        characterData.CurrentSTA += value;

        if (characterData.CurrentSTA <= 0)
        {
            characterData.CurrentSTA = 0;
            AddCharacterEffect(characterData, EEffectID.Exhausted, 10, 1);
        }
        else
        {
            var fullAbility = GetCharacterAbility(characterData);
            if (characterData.CurrentSTA > fullAbility.STA)
                characterData.CurrentSTA = fullAbility.STA;

            if (_effectDatas.TryGetValue(EEffectID.Exhausted, out var effect) && characterData.Effects.Contains(effect))
            {
                characterData.Effects.Remove(effect);
            }
        }

        EffectProcess(characterData);
    }

    public static void AddCharacterEffect(CharacterData characterData, EEffectID effectID, int effectValue, int effectTimes)
    {
        if (_effectDatas.TryGetValue(effectID, out var effect))
        {
            var exist = characterData.Effects.Find(x => x.ID == effect.ID);

            if (exist != null && exist.Value == effectValue)
            {
                exist.Times += effectTimes;
            }
            else
            {
                characterData.Effects.Add(new()
                {
                    Name = effect.Name,
                    ID = effect.ID,
                    Type = effect.Type,
                    Value = effectValue,
                    Times = effectTimes
                });
            }
        }
    }

    public static void MotifyCurrentAbility(CharacterData characterData, FullAbilityBase ability)
    {
        if (ability == null)
            return;

        if (ability.HP != 0)
            characterData.CurrentHP += ability.HP;

        if (ability.MP != 0)
            characterData.CurrentMP += ability.MP;

        if (ability.STA != 0)
            characterData.CurrentSTA += ability.STA;
    }
}

public interface IEffectHandler
{
    int Type { get; }
    void Passive(FullAbilityBase fullAbility, EffectData effectData);
    void Proc(CharacterData characterData, EffectData effectData);
}