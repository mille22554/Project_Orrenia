using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Unity.Netcode;
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

    public static int GetAbilityPoint(CharacterData data) => GetAbilityPoint(data, null);
    public static int GetAbilityPoint(CharacterData data, AbilityBase ability)
    {
        ability ??= data.Ability;
        var totalUsedPoint = ability.STR_Point + ability.AGI_Point + ability.DEX_Point + ability.INT_Point + ability.LUK_Point + ability.VIT_Point;

        return (data.Level + 1) * 6 - totalUsedPoint;
    }

    public static int GetExp(int level)
    {
        return (1 << (level - 1)) * 100;
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
            case EItemCategory.Shield:
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

    public static Color SetColorFromHex(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color c))
            return c;
        else
            return Color.white;
    }

    public static void SerializeList<TSerialized, TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref List<TSerialized> list)
    where TReaderWriter : IReaderWriter
    where TSerialized : unmanaged, IComparable, IConvertible, IComparable<TSerialized>, IEquatable<TSerialized>
    {
        var count = list != null ? list.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
            list = new(new TSerialized[count]);

        for (var i = 0; i < count; i++)
        {
            var val = list[i];
            serializer.SerializeValue(ref val);
            if (serializer.IsReader)
                list[i] = val;
        }
    }

    public static void SerializeStringList<TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref List<string> list)
    where TReaderWriter : IReaderWriter
    {
        var count = list != null ? list.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
            list = new(count);

        for (int i = 0; i < count; i++)
        {
            var val = serializer.IsReader ? "" : list[i];
            serializer.SerializeValue(ref val);
            if (serializer.IsReader)
                list.Add(val);
        }
    }

    public static void SerializeEnumList<TKey, TReaderWriter>(
    BufferSerializer<TReaderWriter> serializer,
    ref List<TKey> list)
    where TReaderWriter : IReaderWriter
    where TKey : unmanaged, Enum
    {
        var count = list != null ? list.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            list = new(count);
        }

        for (var i = 0; i < count; i++)
        {
            TKey val = serializer.IsReader ? default : list[i];

            // 注意：這裡如果編譯器抱怨 TKey 約束，
            // 可以視情況像之前處理 Dict 一樣轉成 int，
            // 但通常在 NGO 最新版中，有 unmanaged, Enum 約束是可以直接過件的。
            serializer.SerializeValue(ref val);

            if (serializer.IsReader)
            {
                list.Add(val);
            }
        }
    }

    public static void SerializeClassList<TSerialized, TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref List<TSerialized> list)
    where TReaderWriter : IReaderWriter
    where TSerialized : class, INetworkSerializable, new()
    {
        var count = list != null ? list.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
            list = new(count);

        for (var i = 0; i < count; i++)
        {
            var item = serializer.IsReader ? new() : list[i];

            // 關鍵：對於 NetworkSerializable 物件，有時候需要顯式指定序列化方式
            serializer.SerializeValue(ref item);

            if (serializer.IsReader)
                list.Add(item);
        }
    }

    public static void SerializeClassDictionary<TKey, TValue, TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref Dictionary<TKey, TValue> dict)
    where TReaderWriter : IReaderWriter
    // 補齊所有 NGO 需要的介面約束
    where TKey : unmanaged, IComparable, IConvertible, IComparable<TKey>, IEquatable<TKey>
    where TValue : class, INetworkSerializable, new()
    {
        var count = dict != null ? dict.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            dict = new();
            for (var i = 0; i < count; i++)
            {
                TKey key = default;
                var val = new TValue();
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
                dict.Add(key, val);
            }
        }
        else
        {
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var val = kvp.Value;
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
            }
        }
    }


    public static void SerializeEnum_ClassDictionary<TKey, TValue, TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref Dictionary<TKey, TValue> dict)
    where TReaderWriter : IReaderWriter where TKey : unmanaged, Enum where TValue : class, INetworkSerializable, new()
    {
        var count = dict != null ? dict.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            dict = new();
            for (var i = 0; i < count; i++)
            {
                TKey key = default;
                var val = new TValue();
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
                dict.Add(key, val);
            }
        }
        else
        {
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var val = kvp.Value;
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
            }
        }
    }

    public static void SerializeEnum_StringDict<TKey, TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref Dictionary<TKey, string> dict)
    where TReaderWriter : IReaderWriter
    where TKey : unmanaged, Enum
    {
        var count = dict != null ? dict.Count : 0;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            dict = new();
            for (var i = 0; i < count; i++)
            {
                TKey key = default;
                var val = "";
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
                dict.Add(key, val);
            }
        }
        else if (dict != null)
        {
            foreach (var kvp in dict)
            {
                TKey key = kvp.Key;
                var val = kvp.Value ?? "";
                serializer.SerializeValue(ref key);
                serializer.SerializeValue(ref val);
            }
        }
    }

    public static void SerializeString_StringListDict<TReaderWriter>(BufferSerializer<TReaderWriter> serializer, ref Dictionary<string, List<string>> dict)
    where TReaderWriter : IReaderWriter
    {
        // 1. 處理字典數量
        var dictCount = dict != null ? dict.Count : 0;
        serializer.SerializeValue(ref dictCount);

        if (serializer.IsReader)
        {
            dict = new();
            for (var i = 0; i < dictCount; i++)
            {
                // 2. 讀取 Key (string)
                var key = "";
                serializer.SerializeValue(ref key);

                // 3. 讀取 Value (List<string>) 的長度
                var listCount = 0;
                serializer.SerializeValue(ref listCount);

                var list = new List<string>(listCount);
                for (var j = 0; j < listCount; j++)
                {
                    var item = "";
                    serializer.SerializeValue(ref item);
                    list.Add(item);
                }

                dict.Add(key, list);
            }
        }
        else if (dict != null)
        {
            // 4. 寫入模式
            foreach (var kvp in dict)
            {
                // 寫入 Key
                var key = kvp.Key;
                serializer.SerializeValue(ref key);

                // 寫入 List 內容
                var list = kvp.Value;
                var listCount = list != null ? list.Count : 0;
                serializer.SerializeValue(ref listCount);

                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var s = item ?? "";
                        serializer.SerializeValue(ref s);
                    }
                }
            }
        }
    }
}
