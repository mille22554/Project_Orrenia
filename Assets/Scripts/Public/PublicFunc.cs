using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class PublicFunc
{

    public static void SetEquipAbility(AbilityBase ability, AbilityBase baseAbility)
    {
        var fields = typeof(AbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            int valueB = (int)field.GetValue(ability);
            if (valueB != 0)
            {
                int valueA = (int)field.GetValue(baseAbility);
                field.SetValue(baseAbility, valueA + valueB);
            }
        }
    }

    public static void AddCharacterEffect(CharacterData characterData, string effectType, int effectValue, int effectTimes)
    {
        characterData.Effects.Add(new()
        {
            type = effectType,
            value = effectValue,
            times = effectTimes
        });
    }

    public static int GetAbilityPoint(CharacterData data) => GetAbilityPoint(data, null);
    public static int GetAbilityPoint(CharacterData data, AbilityBase ability)
    {
        ability ??= data.Ability;
        var totalUsedPoint = ability.STR_Point + ability.AGI_Point + ability.DEX_Point + ability.INT_Point + ability.LUK_Point + ability.VIT_Point;

        return (data.Level + 1) * 6 - totalUsedPoint;
    }

    public static int GetExp(CharacterData data)
    {
        return (1 << (data.Level - 1)) * 100;
    }

    public static int Dice(int num, int face)
    {
        Dice(num, face, 1, out _, out var sum);
        return sum;
    }
    public static void Dice(int num, int face, int times, out List<int> results, out int sum)
    {
        results = new();
        sum = 0;

        for (int i = 0; i < times; i++)
        {
            for (int j = 0; j < num; j++)
            {
                var result = Random.Range(1, face + 1);
                results?.Add(result);
                sum += result;
            }
        }
    }

    public static void DoActionAccordingToCategory(EItemCategory category, Action equipCallBack, Action useCallBack, Action materialCallBack)
    {
        switch (category)
        {
            case EItemCategory.One_Hand:
            case EItemCategory.Two_Hand:
            case EItemCategory.Helmet:
            case EItemCategory.Armor:
            case EItemCategory.Greaves:
            case EItemCategory.Shoes:
            case EItemCategory.Gloves:
            case EItemCategory.Cape:
            case EItemCategory.Ring:
            case EItemCategory.Pendant:
                equipCallBack?.Invoke();
                break;
            case EItemCategory.Use:
                useCallBack?.Invoke();
                break;
            case EItemCategory.Material:
                materialCallBack?.Invoke();
                break;
        }
    }

    public static bool IsEquipCategory(EItemCategory category)
    {
        return category != EItemCategory.Use && category != EItemCategory.Material;
    }

    public static bool IsUseCategory(EItemCategory category)
    {
        return category == EItemCategory.Use;
    }

    public static bool IsMaterialCategory(EItemCategory category)
    {
        return category == EItemCategory.Material;
    }
}
