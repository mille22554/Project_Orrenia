using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public class PublicFunc
{
    static PlayerData _playerData => GameData.NowPlayerData;

    public static void SaveData()
    {
        var path = GameData.SaveDataPath;
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData.gameData));
    }

    public static GameSaveData UpdateSaveData(GameSaveData oldData)
    {
        // Debug.Log("更新存檔資料結構");
        var newData = GameSaveData.CreateDefault();
        newData.datas.playerData.PlayerName = oldData.datas.playerData.PlayerName;

        CopyNonDefaultValues(oldData.datas, newData.datas);
        return newData;
    }

    public static T CopyNonDefaultValues<T>(T oldData, T newData)
    {
        if (oldData == null || newData == null) return newData;

        foreach (var field in oldData.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            object oldValue = field.GetValue(oldData);
            if (oldValue == null) continue;

            FieldInfo newField = newData.GetType().GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
            if (newField == null) continue; // 新物件沒有這個欄位就跳過

            Type fieldType = field.FieldType;

            // 判斷是否為自訂 class
            if (!fieldType.IsPrimitive && fieldType != typeof(string))
            {
                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    var oldList = oldValue as IList;

                    if (newField.GetValue(newData) is not IList newList)
                    {
                        newList = Activator.CreateInstance(fieldType) as IList;
                        newField.SetValue(newData, newList);
                    }

                    newList.Clear();
                    foreach (var item in oldList)
                    {
                        newList.Add(item);
                    }
                }
                else
                {
                    object newChild = newField.GetValue(newData);
                    if (newChild == null)
                    {
                        newChild = Activator.CreateInstance(fieldType);
                        newField.SetValue(newData, newChild);
                    }
                    CopyNonDefaultValues(oldValue, newChild);
                }
            }
            else
            {
                // 基本型別直接賦值
                newField.SetValue(newData, oldValue);
            }
        }

        // ====== 處理屬性 ======
        foreach (var prop in oldData.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue; // 跳過唯讀屬性

            object oldValue = prop.GetValue(oldData);
            if (oldValue == null) continue;

            Type propType = prop.PropertyType;

            if (!propType.IsPrimitive && propType != typeof(string))
            {
                if (typeof(IList).IsAssignableFrom(propType))
                {
                    if (oldValue is not IList oldList) continue;

                    if (prop.GetValue(newData) is not IList newList)
                    {
                        newList = Activator.CreateInstance(propType) as IList;
                        prop.SetValue(newData, newList);
                    }

                    newList.Clear();
                    foreach (var item in oldList)
                        newList.Add(item);
                }
                else
                {
                    object newChild = prop.GetValue(newData);
                    if (newChild == null)
                    {
                        newChild = Activator.CreateInstance(propType);
                        prop.SetValue(newData, newChild);
                    }
                    CopyNonDefaultValues(oldValue, newChild);
                }
            }
            else
            {
                prop.SetValue(newData, oldValue);
            }
        }

        return newData;
    }

    public static void SetPlayerAbility()
    {
        SetPlayerAbility(
            _playerData.ability,
            _playerData.equips,
            _playerData.effects,
            _playerData.effectActions
        );
        SetHP(_playerData.CurrentHp);
        SetMP(_playerData.CurrentMp);
        SetCurrentSTA(_playerData.CurrentSTA);
    }

    public static void SetPlayerAbility(AbilityBase data, EquipBase equips, List<EffectData> effects, List<Action<bool>> actions)
    {
        data.STR = data.STR_Point;
        data.VIT = data.VIT_Point;
        data.DEX = data.DEX_Point;
        data.INT = data.INT_Point;
        data.AGI = data.AGI_Point;
        data.LUK = data.LUK_Point;

        data.HP = data.VIT * 10 + data.STR * 5 + 85;
        data.MP = data.INT * 10 + data.VIT * 5 + 35;
        data.STA = data.VIT * 5 + 95;
        data.ATK = data.STR * 2 + data.VIT;
        data.MATK = data.INT * 2 + data.VIT;
        data.DEF = data.VIT * 2 + data.STR;
        data.MDEF = data.VIT * 2 + data.INT;
        data.ACC = data.AGI * 3 + data.DEX * 2 + data.LUK;
        data.EVA = data.DEX * 3 + data.AGI * 2 + data.LUK;
        data.CRIT = data.AGI * 2 + data.LUK;
        data.SPD = data.DEX;

        ReSetEquipAbility(equips);

        SetEffectAbility(effects, actions);
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

    public static void ReSetEquipAbility(EquipBase equips)
    {
        var fields = typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            long uid = (long)field.GetValue(equips);
            if (uid != 0)
            {
                var item = GameData.NowBagData.items.Find(x => x.uid == uid);
                SetEquipAbility(ItemBaseData.Get(item.itemID).ability);
            }
        }
    }

    public static void SetEquipAbility(AbilityBase ability) => SetEquipAbility(ability, false);
    public static void SetEquipAbility(AbilityBase ability, bool isUnload)
        => SetEquipAbility(ability, _playerData.ability, isUnload);
    public static void SetEquipAbility(AbilityBase ability, AbilityBase baseAbility) => SetEquipAbility(ability, baseAbility, false);
    public static void SetEquipAbility(AbilityBase ability, AbilityBase baseAbility, bool isUnload)
    {
        var fields = typeof(AbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            int valueB = (int)field.GetValue(ability);
            if (valueB != 0)
            {
                int valueA = (int)field.GetValue(baseAbility);
                field.SetValue(baseAbility, valueA + (isUnload ? valueB * (-1) : valueB));
            }
        }
    }

    public static void UnloadEquip(long uid)
    {
        SwitchEquipSlot(uid);
        var item = GameData.NowBagData.items.Find(x => x.uid == uid && ItemTypeCheck.IsEquipType(ItemBaseData.Get(x.itemID).type));
        if (item != null)
            SetEquipAbility(ItemBaseData.Get(item.itemID).ability, true);
    }

    public static void SwitchEquipSlot(long uid)
    {
        // 取得所有公開實例欄位
        var fields = typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            long value = (long)field.GetValue(_playerData.equips);

            if (value == uid && uid != 0)
            {
                field.SetValue(_playerData.equips, 0L);
                Debug.Log($"已清空欄位：{field.Name}");
            }
        }
    }

    public static bool CheckIsPlayerEquip(ItemData item)
    {
        return ItemBaseData.Get(item.itemID).type switch
        {
            EquipType.One_Hand_Weapon.Sword or
            EquipType.One_Hand_Weapon.Dagger => _playerData.equips.Right_Hand == item.uid,
            // EquipType.Two_Hand_Weapon => _playerData.equips.Right_Hand == item.uid && _playerData.equips.Left_Hand == -1,
            EquipType.Shield => _playerData.equips.Left_Hand == item.uid,
            EquipType.Helmet => _playerData.equips.Helmet == item.uid,
            EquipType.Armor => _playerData.equips.Armor == item.uid,
            EquipType.Greaves => _playerData.equips.Greaves == item.uid,
            EquipType.Shoes => _playerData.equips.Shoes == item.uid,
            EquipType.Gloves => _playerData.equips.Gloves == item.uid,
            EquipType.Cape => _playerData.equips.Cape == item.uid,
            EquipType.Ring => _playerData.equips.Ring == item.uid,
            EquipType.Pendant => _playerData.equips.Pendant == item.uid,
            _ => false,
        };
    }

    public static void SetEffectAbility(List<EffectData> effects, List<Action<bool>> actions)
    {
        actions.Clear();

        foreach (var effect in effects)
        {
            Action<bool> effectAction = null;
            switch (effect.type)
            {
                case EffectType.Buff.HP_UP:
                    _playerData.ability.HP *= effect.value;
                    PanelInfo.Instance.RefreshInfo();

                    effectAction = isTimePass => ActionCounter(isTimePass, ref effectAction);
                    break;
                case EffectType.Buff.HP_Regen:
                    effectAction = isTimePass =>
                    {
                        if (isTimePass)
                            SetHP(_playerData.CurrentHp + effect.value);

                        ActionCounter(isTimePass, ref effectAction);
                    };
                    break;
                case EffectType.Buff.Berserk:
                    _playerData.ability.HP *= effect.value;
                    _playerData.ability.MP *= effect.value;
                    _playerData.ability.ATK *= effect.value;
                    _playerData.ability.MATK *= effect.value;
                    _playerData.ability.DEF *= effect.value;
                    _playerData.ability.MDEF *= effect.value;
                    _playerData.ability.ACC *= effect.value;
                    _playerData.ability.EVA *= effect.value;
                    _playerData.ability.CRIT *= effect.value;
                    _playerData.ability.SPD *= effect.value;
                    effectAction = isTimePass => ActionCounter(isTimePass, ref effectAction);
                    break;

                case EffectType.Debuff.Exhausted:
                    _playerData.ability.HP /= effect.value;
                    _playerData.ability.MP /= effect.value;
                    _playerData.ability.ATK /= effect.value;
                    _playerData.ability.MATK /= effect.value;
                    _playerData.ability.DEF /= effect.value;
                    _playerData.ability.MDEF /= effect.value;
                    _playerData.ability.ACC /= effect.value;
                    _playerData.ability.EVA /= effect.value;
                    _playerData.ability.CRIT /= effect.value;
                    _playerData.ability.SPD /= effect.value;

                    Action<bool> temp = isTimePass =>
                    {
                        if (_playerData.CurrentSTA > 0)
                        {
                            // Debug.Log("應該要清掉了");
                            RemovePlayerEffect(effect, ref effectAction);
                        }
                    };
                    effectAction = temp;

                    break;
            }
            if (effectAction != null)
                _playerData.effectActions.Add(effectAction);

            void ActionCounter(bool isTimePass, ref Action<bool> action)
            {
                if (isTimePass) effect.times--;
                if (effect.times <= 0) RemovePlayerEffect(effect, ref action);
            }
        }
    }

    public static void AddPlayerEffect(string effectType, int effectValue, int effectTimes)
    {
        _playerData.effects.Add(new()
        {
            type = effectType,
            value = effectValue,
            times = effectTimes
        });
        SetPlayerAbility();
    }

    public static void RemovePlayerEffect(EffectData effect, ref Action<bool> action)
    {
        // var effectRemoved =
        _playerData.effects.Remove(effect);
        // var actionRemoved =
        _playerData.effectActions.Remove(action);
        // Debug.Log($"Effect is {effect.type}. Effect removed: {effectRemoved}, Action removed: {actionRemoved}");
        SetPlayerAbility();
    }

    public static void CheckFlags()
    {
        var playerData = _playerData;
        if (!playerData.isGetBasicDagger)
        {
            GameData.NowBagData.items.Add(GetItem(GameItem.Equip.BasicDagger));
            playerData.isGetBasicDagger = true;
        }
        SaveData();
    }

    public static void SetEXP(int value) => _playerData.CurrentExp = value;

    public static void SetHP(int value)
    {
        _playerData.CurrentHp = value;
        if (_playerData.CurrentHp > _playerData.ability?.HP)
            _playerData.CurrentHp = _playerData.ability.HP;
    }

    public static void SetMP(int value)
    {
        _playerData.CurrentMp = value;
        if (_playerData.CurrentMp > _playerData.ability?.MP)
            _playerData.CurrentMp = _playerData.ability.MP;
    }

    public static void SetCurrentSTA(int value)
    {
        _playerData.CurrentSTA = value;
        if (_playerData.CurrentSTA > _playerData.ability?.STA)
            _playerData.CurrentSTA = _playerData.ability.STA;
        else if (_playerData.CurrentSTA < 0)
            _playerData.CurrentSTA = 0;
        else if (_playerData.CurrentSTA == 0)
            AddPlayerEffect(EffectType.Debuff.Exhausted, 10, 1);
    }

    public static void SetAbilityPoint(int value) => _playerData.AbilityPoint = value;
}
