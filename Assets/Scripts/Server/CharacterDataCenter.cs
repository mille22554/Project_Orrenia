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
    readonly static Dictionary<EEffectID, IEffectHandler> _effectHandlers = new();

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

            Debug.Log($"Register effect handler: {instance.ID} -> {type.Name}");

            _effectHandlers[instance.ID] = instance;
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

    public static EffectData GetEffectData(EEffectID effectID)
    {
        _effectDatas.TryGetValue(effectID, out var data);
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

        CalculateEquipAbility(ability, data);
        CalculateEffectAbility(ability, data.Effects, out var afterAbility);

        if (data.Role == ECharacterRole.Mob)
            afterAbility.HP /= 10;

        FixAbility(data, afterAbility);

        return afterAbility;
    }

    static void CalculateEquipAbility(FullAbilityBase data, CharacterData characterData)
    {
        foreach (var equipUID in characterData.Equips)
        {
            var item = characterData.BagItems.Find(x => x.UID == equipUID);
            var ability = ItemDataCenter_Server.FinalAbilityProcess(item);
            var fields = typeof(FullAbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var valueB = (decimal)field.GetValue(ability);
                if (valueB != 0)
                {
                    var valueA = (decimal)field.GetValue(data);
                    field.SetValue(data, valueA + valueB);
                }
            }
        }
    }

    static void CalculateEffectAbility(FullAbilityBase baseAbility, List<EffectData> effects, out FullAbilityBase afterAbility)
    {
        afterAbility = new FullAbilityBase
        {
            STR = baseAbility.STR,
            DEX = baseAbility.DEX,
            INT = baseAbility.INT,
            VIT = baseAbility.VIT,
            AGI = baseAbility.AGI,
            LUK = baseAbility.LUK,
            HP = baseAbility.HP,
            MP = baseAbility.MP,
            STA = baseAbility.STA,
            ATK = baseAbility.ATK,
            MATK = baseAbility.MATK,
            DEF = baseAbility.DEF,
            MDEF = baseAbility.MDEF,
            ACC = baseAbility.ACC,
            EVA = baseAbility.EVA,
            CRIT = baseAbility.CRIT,
            SPD = baseAbility.SPD,
        };

        foreach (var effect in effects)
        {
            if (_effectHandlers.TryGetValue(effect.ID, out var effectHandler))
            {
                effectHandler.Passive(baseAbility, effect, afterAbility);
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

    static EffectResult.Result EffectProcess(CharacterData characterData)
    {
        var result = new EffectResult.Result()
        {
            CharacterName = characterData.Name
        };

        foreach (var effect in characterData.Effects.ToList())
        {
            if (_effectHandlers.TryGetValue(effect.ID, out var effectHandler))
            {
                effectHandler.Proc(characterData, effect, result);
            }
        }
        return result;
    }

    public static void STAProcess(CharacterData characterData, decimal value)
    {
        characterData.CurrentSTA += value;

        if (characterData.CurrentSTA <= 0)
        {
            characterData.CurrentSTA = 0;
            var effectValue = new List<ParamFormat> { new() { Constant = -0.9m } };
            AddCharacterEffect(characterData, EEffectID.Exhausted, effectValue, 1);
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
    }

    static void SkillCDProcess(CharacterData characterData)
    {
        foreach (var skill in characterData.Skills.Values)
        {
            if (skill.CurrentCD > 0)
                skill.CurrentCD--;
        }
    }

    public static EffectResult.Result ActionEndProcess(CharacterData characterData) => ActionEndProcess(characterData, false);
    public static EffectResult.Result ActionEndProcess(CharacterData characterData, bool isRest)
    {
        if (!isRest)
            STAProcess(characterData, -1);

        var result = EffectProcess(characterData);

        if (characterData.CurrentHP <= 0)
        {
            result.IsDead = true;
            characterData.Effects.Clear();
        }

        SkillCDProcess(characterData);

        return result;
    }

    public static void AddCharacterEffect(CharacterData characterData, EffectData effectData)
        => AddCharacterEffect(characterData, effectData.ID, effectData.Value, effectData.Times);
    public static void AddCharacterEffect(CharacterData characterData, EEffectID effectID, List<ParamFormat> effectValue, int effectTimes)
    {
        if (_effectDatas.TryGetValue(effectID, out var effect))
        {
            var exist = characterData.Effects.Find(x => x.ID == effect.ID);

            if (exist != null)
            {
                if (exist.Value.Count == effectValue.Count &&
                    exist.Value
                        .GroupBy(x => x)
                        .ToDictionary(g => g.Key, g => g.Count())
                        .SequenceEqual(
                            effectValue
                                .GroupBy(x => x)
                                .ToDictionary(g => g.Key, g => g.Count())
                        )
                    )
                    exist.Times += effectTimes;
            }
            else
            {
                characterData.Effects.Add(new()
                {
                    Name = effect.Name,
                    ID = effect.ID,
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

    public static int ParamCalculate(FullAbilityBase ability, List<ParamFormat> paramFormats)
    {
        var damage = 0m;
        foreach (var format in paramFormats)
        {
            if (format.Ability != null)
            {
                damage += format.Constant *
                (
                    format.Ability.STR * ability.STR +
                    format.Ability.DEX * ability.DEX +
                    format.Ability.INT * ability.INT +
                    format.Ability.VIT * ability.VIT +
                    format.Ability.AGI * ability.AGI +
                    format.Ability.LUK * ability.LUK +
                    format.Ability.HP * ability.HP +
                    format.Ability.MP * ability.MP +
                    format.Ability.STA * ability.STA +
                    format.Ability.ATK * ability.ATK +
                    format.Ability.MATK * ability.MATK +
                    format.Ability.DEF * ability.DEF +
                    format.Ability.MDEF * ability.MDEF +
                    format.Ability.ACC * ability.ACC +
                    format.Ability.EVA * ability.EVA +
                    format.Ability.CRIT * ability.CRIT +
                    format.Ability.SPD * ability.SPD
                );
            }
            else
            {
                damage += format.Constant;
            }
        }

        return (int)damage;
    }
}

public interface IEffectHandler
{
    EEffectID ID { get; }
    void Passive(FullAbilityBase baseAbility, EffectData effectData, FullAbilityBase afterAbility);
    void Proc(CharacterData characterData, EffectData effectData, EffectResult.Result result);
}