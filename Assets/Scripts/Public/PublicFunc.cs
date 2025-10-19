using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

public class PublicFunc
{
    public static void SaveData()
    {
        var path = Path.Combine(Application.persistentDataPath, "savedata.json");
        // Debug.Log($"儲存遊戲資料到 {path}");
        File.WriteAllText(path, JsonConvert.SerializeObject(GameData.gameData));
    }

    public static GameSaveData UpdateSaveData(GameSaveData oldData)
    {
        // Debug.Log("更新存檔資料結構");
        var newData = new GameSaveData().Create();
        newData.datas.playerData.name = oldData.datas.playerData.name;

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
            GameData.NowPlayerData.ability,
            GameData.NowPlayerData.equips,
            GameData.NowPlayerData.effects,
            GameData.NowPlayerData.effectActions
        );
        if (GameData.NowPlayerData.CurrentHp > GameData.NowPlayerData.ability?.HP)
            GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
        if (GameData.NowPlayerData.CurrentMp > GameData.NowPlayerData.ability?.MP)
            GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;
        if (GameData.NowPlayerData.currentSTA > GameData.NowPlayerData.ability?.STA)
            GameData.NowPlayerData.SetCurrentSTA(GameData.NowPlayerData.ability.STA);
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

        EventMng.EmitEvent(EventName.RefreshPlayerInfo);
    }

    public static ItemData GetItem(ItemData source)
    {
        var target = CopyFields(source);
        target.uid = Math.Abs(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0));
        return target;
    }

    private static T CopyFields<T>(T source)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source));
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
                SetEquipAbility(item.ability);
            }
        }
    }

    public static void SetEquipAbility(AbilityBase ability) => SetEquipAbility(ability, false);
    public static void SetEquipAbility(AbilityBase ability, bool isUnload)
    {
        var fields = typeof(AbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            int valueB = (int)field.GetValue(ability);
            if (valueB != 0)
            {
                int valueA = (int)field.GetValue(GameData.NowPlayerData.ability);
                field.SetValue(GameData.NowPlayerData.ability, valueA + (isUnload ? valueB * (-1) : valueB));
            }
        }
    }

    public static void UnloadEquip(long uid)
    {
        SwitchEquipSlot(uid);
        var item = GameData.NowBagData.items.Find(x => x.uid == uid && ItemTypeCheck.IsEquipType(x.type));
        if (item != null)
            SetEquipAbility(item.ability, true);
    }

    public static void SwitchEquipSlot(long uid)
    {
        // 取得所有公開實例欄位
        var fields = typeof(EquipBase).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            long value = (long)field.GetValue(GameData.NowPlayerData.equips);

            if (value == uid && uid != 0)
            {
                field.SetValue(GameData.NowPlayerData.equips, 0L);
                Debug.Log($"已清空欄位：{field.Name}");
            }
        }
    }

    public static bool CheckIsPlayerEquip(ItemData item)
    {
        return item.type switch
        {
            EquipType.One_Hand_Weapon.Sword or
            EquipType.One_Hand_Weapon.Dagger => GameData.NowPlayerData.equips.Right_Hand == item.uid,
            // EquipType.Two_Hand_Weapon => GameData.NowPlayerData.equips.Right_Hand == item.uid && GameData.NowPlayerData.equips.Left_Hand == -1,
            EquipType.Shield => GameData.NowPlayerData.equips.Left_Hand == item.uid,
            EquipType.Helmet => GameData.NowPlayerData.equips.Helmet == item.uid,
            EquipType.Armor => GameData.NowPlayerData.equips.Armor == item.uid,
            EquipType.Greaves => GameData.NowPlayerData.equips.Greaves == item.uid,
            EquipType.Shoes => GameData.NowPlayerData.equips.Shoes == item.uid,
            EquipType.Gloves => GameData.NowPlayerData.equips.Gloves == item.uid,
            EquipType.Cape => GameData.NowPlayerData.equips.Cape == item.uid,
            EquipType.Ring => GameData.NowPlayerData.equips.Ring == item.uid,
            EquipType.Pendant => GameData.NowPlayerData.equips.Pendant == item.uid,
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
                    GameData.NowPlayerData.ability.HP *= effect.value;
                    EventMng.EmitEvent(EventName.RefreshPlayerInfo);

                    effectAction = isTimePass => ActionCounter(isTimePass, ref effectAction);
                    break;
                case EffectType.Buff.HP_Regen:
                    effectAction = isTimePass =>
                    {
                        if (isTimePass) GameData.NowPlayerData.CurrentHp += effect.value;
                        ActionCounter(isTimePass, ref effectAction);
                    };
                    break;
                case EffectType.Buff.Berserk:
                    GameData.NowPlayerData.ability.HP *= effect.value;
                    GameData.NowPlayerData.ability.MP *= effect.value;
                    GameData.NowPlayerData.ability.ATK *= effect.value;
                    GameData.NowPlayerData.ability.MATK *= effect.value;
                    GameData.NowPlayerData.ability.DEF *= effect.value;
                    GameData.NowPlayerData.ability.MDEF *= effect.value;
                    GameData.NowPlayerData.ability.ACC *= effect.value;
                    GameData.NowPlayerData.ability.EVA *= effect.value;
                    GameData.NowPlayerData.ability.CRIT *= effect.value;
                    GameData.NowPlayerData.ability.SPD *= effect.value;
                    effectAction = isTimePass => ActionCounter(isTimePass, ref effectAction);
                    break;

                case EffectType.Debuff.Exhausted:
                    GameData.NowPlayerData.ability.HP /= effect.value;
                    GameData.NowPlayerData.ability.MP /= effect.value;
                    GameData.NowPlayerData.ability.ATK /= effect.value;
                    GameData.NowPlayerData.ability.MATK /= effect.value;
                    GameData.NowPlayerData.ability.DEF /= effect.value;
                    GameData.NowPlayerData.ability.MDEF /= effect.value;
                    GameData.NowPlayerData.ability.ACC /= effect.value;
                    GameData.NowPlayerData.ability.EVA /= effect.value;
                    GameData.NowPlayerData.ability.CRIT /= effect.value;
                    GameData.NowPlayerData.ability.SPD /= effect.value;

                    Action<bool> temp = isTimePass =>
                    {
                        if (GameData.NowPlayerData.currentSTA > 0)
                        {
                            // Debug.Log("應該要清掉了");
                            RemovePlayerEffect(effect, ref effectAction);
                        }
                    };
                    effectAction = temp;

                    break;
            }
            if (effectAction != null)
                GameData.NowPlayerData.effectActions.Add(effectAction);

            void ActionCounter(bool isTimePass, ref Action<bool> action)
            {
                if (isTimePass) effect.times--;
                if (effect.times <= 0) RemovePlayerEffect(effect, ref action);
            }
        }
    }

    public static void AddPlayerEffect(string effectType, int effectValue, int effectTimes)
    {
        GameData.NowPlayerData.effects.Add(new()
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
        GameData.NowPlayerData.effects.Remove(effect);
        // var actionRemoved =
        GameData.NowPlayerData.effectActions.Remove(action);
        // Debug.Log($"Effect is {effect.type}. Effect removed: {effectRemoved}, Action removed: {actionRemoved}");
        SetPlayerAbility();
    }
}