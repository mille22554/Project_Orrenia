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
    static CharacterData _characterData => GameData_Server.NowCharacterData;

    public static void SaveData()
    {
        var path = GameData_Server.SaveDataPath;
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData_Server.SaveData));
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

        if (data.Role == CharacterRole.Mob)
            ability.HP /= 10;

        ability = CalculateEquipAbility(ability, data.Equips);
        ability = CalculateEffectAbility(ability, data.Effects);

        return ability;
    }

    public static CharacterData InitCurrentData(CharacterData data)
    {
        var fullAbility = GetCharacterAbility(data);
        data.CurrentHP = fullAbility.HP;
        data.CurrentMP = fullAbility.MP;
        data.CurrentSTA = fullAbility.STA;

        return data;
    }

    public static FullAbilityBase CalculateEquipAbility(FullAbilityBase data, EquipBase equips)
    {
        var equipFields = typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var equipField in equipFields)
        {
            long uid = (long)equipField.GetValue(equips);
            if (uid != 0)
            {
                var item = GameData_Server.NowBagData.items.Find(x => x.uid == uid);
                var ability = ItemBaseData.Get(item.itemID).ability;
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

    public static void EffectProcess()
    {
        foreach (var effect in _characterData.Effects)
        {
            switch (effect.type)
            {
                case EffectType.Buff.HP_Regen:
                    _characterData.CurrentHP += effect.value;
                    break;
            }

            effect.times--;
            if (effect.times <= 0)
                _characterData.Effects.Remove(effect);
        }
    }

    public static ItemData GetItem(ItemBaseData source)
    {
        var target = new ItemData
        {
            itemID = source.id,
            durability = source.durability,
            uid = Math.Abs(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0)),
            count = source.count
        };
        return target;
    }

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

    public static void UnloadEquip(long uid)
    {
        SwitchEquipSlot(uid);
    }

    public static void SwitchEquipSlot(long uid)
    {
        // 取得所有公開實例欄位
        var fields = typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            long value = (long)field.GetValue(_characterData.Equips);

            if (value == uid && uid != 0)
            {
                field.SetValue(_characterData.Equips, 0L);
                Debug.Log($"已清空欄位：{field.Name}");
            }
        }
    }

    public static bool CheckIsPlayerEquip(ItemData item)
    {
        return ItemBaseData.Get(item.itemID).type switch
        {
            EquipType.One_Hand_Weapon.Sword or
            EquipType.One_Hand_Weapon.Dagger => _characterData.Equips.Right_Hand == item.uid,
            // EquipType.Two_Hand_Weapon => _playerData.equips.Right_Hand == item.uid && _playerData.equips.Left_Hand == -1,
            EquipType.Shield => _characterData.Equips.Left_Hand == item.uid,
            EquipType.Helmet => _characterData.Equips.Helmet == item.uid,
            EquipType.Armor => _characterData.Equips.Armor == item.uid,
            EquipType.Greaves => _characterData.Equips.Greaves == item.uid,
            EquipType.Shoes => _characterData.Equips.Shoes == item.uid,
            EquipType.Gloves => _characterData.Equips.Gloves == item.uid,
            EquipType.Cape => _characterData.Equips.Cape == item.uid,
            EquipType.Ring => _characterData.Equips.Ring == item.uid,
            EquipType.Pendant => _characterData.Equips.Pendant == item.uid,
            _ => false,
        };
    }

    public static void AddPlayerEffect(string effectType, int effectValue, int effectTimes)
    {
        _characterData.Effects.Add(new()
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
}
