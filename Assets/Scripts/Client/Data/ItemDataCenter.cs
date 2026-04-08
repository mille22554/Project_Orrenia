using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Color = UnityEngine.Color;

public static class ItemDataCenter
{
    static Dictionary<int, ItemData> _itemData;
    static Dictionary<EItemKind, ItemKind> _itemKind;
    static List<int> _gameShopItem;
    static List<QualityData> _qualityData;

    static void RefreshData(Action callback)
    {
        if (_itemData == null || _itemKind == null || _gameShopItem == null || _qualityData == null)
        {
            var requestData = new GetItemDataRequest();
            ApiBridge.Send(requestData, CallBack);

            void CallBack(GetItemDataResponse response)
            {
                _itemData = response.ItemData;
                _itemKind = response.ItemKind;
                _gameShopItem = response.GameShopItem;
                _qualityData = response.QualityData;

                callback?.Invoke();
            }
        }
        else
        {
            callback?.Invoke();
        }
    }

    public static ItemData GetItemData(int id)
    {
        ItemData item = null;
        RefreshData(CallBack);

        return item;

        void CallBack() => _itemData.TryGetValue(id, out item);
    }

    public static ItemKind GetItemKind(EItemKind kind)
    {
        ItemKind itemKind = null;
        RefreshData(CallBack);

        return itemKind;

        void CallBack() => _itemKind.TryGetValue(kind, out itemKind);
    }

    public static void DoActionAccordingToCategory(EItemKind kind, Action equipCallBack, Action useCallBack, Action materialCallBack)
        => PublicFunc.DoActionAccordingToCategory(GetItemKind(kind).Category, equipCallBack, useCallBack, materialCallBack);

    public static List<int> GetShopList()
    {
        RefreshData(null);

        return _gameShopItem;
    }

    public static Dictionary<EItemKind, ItemKind> GetItemKindList()
    {
        RefreshData(null);

        return _itemKind;
    }

    public static QualityData GetQualityData(EQuality quality)
    {
        QualityData qualityData = null;
        RefreshData(CallBack);

        return qualityData;

        void CallBack() => qualityData = _qualityData.ElementAtOrDefault((int)quality);
    }


    public static string GetAbilityString(BagItemData itemData) => GetAbilityString(FinalAbilityProcess(itemData), itemData.Seed);
    public static string GetAbilityString(FullAbilityBase ability, int seed)
    {
        if (ability == null)
            return "";

        var parts = new List<string>();
        var nameRandom = seed == 0 ? null : new System.Random(seed);

        // 用反射抓 AbilityBase 的欄位
        foreach (var field in typeof(FullAbilityBase).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = (int)field.GetValue(ability);
            var color = ColorUtility.ToHtmlStringRGB(Color.white);

            if (nameRandom != null)
            {
                var randomValue = nameRandom.NextDouble();

                if (randomValue > 0.5)
                    color = ColorUtility.ToHtmlStringRGB(Color.yellow);
                else if (randomValue < 0.5)
                    color = ColorUtility.ToHtmlStringRGB(Color.gray);
            }

            if (value != 0)
            {
                var sign = value > 0 ? "+" : "";

                parts.Add($"{field.Name}{sign}<color=#{color}>{value}</color>");
            }
        }

        return string.Join(", ", parts);
    }

    public static FullAbilityBase FinalAbilityProcess(BagItemData itemData)
    {
        var final = new FullAbilityBase
        {
            STR = itemData.Ability.STR,
            DEX = itemData.Ability.DEX,
            INT = itemData.Ability.INT,
            VIT = itemData.Ability.VIT,
            AGI = itemData.Ability.AGI,
            LUK = itemData.Ability.LUK,
            HP = itemData.Ability.HP,
            MP = itemData.Ability.MP,
            STA = itemData.Ability.STA,
            ATK = itemData.Ability.ATK,
            MATK = itemData.Ability.MATK,
            DEF = itemData.Ability.DEF,
            MDEF = itemData.Ability.MDEF,
            ACC = itemData.Ability.ACC,
            EVA = itemData.Ability.EVA,
            CRIT = itemData.Ability.CRIT,
            SPD = itemData.Ability.SPD,
        };

        var nameRandom = new System.Random(itemData.Seed);

        var materialMulti = new FullAbilityBase
        {
            STR = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            DEX = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            INT = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            VIT = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            AGI = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            LUK = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            HP = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            MP = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            STA = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            ATK = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            MATK = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            DEF = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            MDEF = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            ACC = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            EVA = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            CRIT = (int)((nameRandom.NextDouble() + 0.5) * 1000),
            SPD = (int)((nameRandom.NextDouble() + 0.5) * 1000),
        };

        var tempSTR = 0m;
        var tempDEX = 0m;
        var tempINT = 0m;
        var tempVIT = 0m;
        var tempAGI = 0m;
        var tempLUK = 0m;
        var tempHP = 0m;
        var tempMP = 0m;
        var tempSTA = 0m;
        var tempATK = 0m;
        var tempMATK = 0m;
        var tempDEF = 0m;
        var tempMDEF = 0m;
        var tempACC = 0m;
        var tempEVA = 0m;
        var tempCRIT = 0m;
        var tempSPD = 0m;

        foreach (var material in itemData.Materials)
        {
            var materialData = GetItemData(material);

            tempSTR += materialData.Ability.STR * (materialMulti.STR / 1000m);
            tempDEX += materialData.Ability.DEX * (materialMulti.DEX / 1000m);
            tempINT += materialData.Ability.INT * (materialMulti.INT / 1000m);
            tempVIT += materialData.Ability.VIT * (materialMulti.VIT / 1000m);
            tempAGI += materialData.Ability.AGI * (materialMulti.AGI / 1000m);
            tempLUK += materialData.Ability.LUK * (materialMulti.LUK / 1000m);
            tempHP += materialData.Ability.HP * (materialMulti.HP / 1000m);
            tempMP += materialData.Ability.MP * (materialMulti.MP / 1000m);
            tempSTA += materialData.Ability.STA * (materialMulti.STA / 1000m);
            tempATK += materialData.Ability.ATK * (materialMulti.ATK / 1000m);
            tempMATK += materialData.Ability.MATK * (materialMulti.MATK / 1000m);
            tempDEF += materialData.Ability.DEF * (materialMulti.DEF / 1000m);
            tempMDEF += materialData.Ability.MDEF * (materialMulti.MDEF / 1000m);
            tempACC += materialData.Ability.ACC * (materialMulti.ACC / 1000m);
            tempEVA += materialData.Ability.EVA * (materialMulti.EVA / 1000m);
            tempCRIT += materialData.Ability.CRIT * (materialMulti.CRIT / 1000m);
            tempSPD += materialData.Ability.SPD * (materialMulti.SPD / 1000m);
        }

        final.STR += (int)tempSTR;
        final.DEX += (int)tempDEX;
        final.INT += (int)tempINT;
        final.VIT += (int)tempVIT;
        final.AGI += (int)tempAGI;
        final.LUK += (int)tempLUK;
        final.HP += (int)tempHP;
        final.MP += (int)tempMP;
        final.STA += (int)tempSTA;
        final.ATK += (int)tempATK;
        final.MATK += (int)tempMATK;
        final.DEF += (int)tempDEF;
        final.MDEF += (int)tempMDEF;
        final.ACC += (int)tempACC;
        final.EVA += (int)tempEVA;
        final.CRIT += (int)tempCRIT;
        final.SPD += (int)tempSPD;

        var qualityData = _qualityData.ElementAtOrDefault((int)itemData.Quality);
        if (qualityData != null)
        {
            final.STR = (int)(final.STR * qualityData.Multi);
            final.DEX = (int)(final.DEX * qualityData.Multi);
            final.INT = (int)(final.INT * qualityData.Multi);
            final.VIT = (int)(final.VIT * qualityData.Multi);
            final.AGI = (int)(final.AGI * qualityData.Multi);
            final.LUK = (int)(final.LUK * qualityData.Multi);
            final.HP = (int)(final.HP * qualityData.Multi);
            final.MP = (int)(final.MP * qualityData.Multi);
            final.STA = (int)(final.STA * qualityData.Multi);
            final.ATK = (int)(final.ATK * qualityData.Multi);
            final.MATK = (int)(final.MATK * qualityData.Multi);
            final.DEF = (int)(final.DEF * qualityData.Multi);
            final.MDEF = (int)(final.MDEF * qualityData.Multi);
            final.ACC = (int)(final.ACC * qualityData.Multi);
            final.EVA = (int)(final.EVA * qualityData.Multi);
            final.CRIT = (int)(final.CRIT * qualityData.Multi);
            final.SPD = (int)(final.SPD * qualityData.Multi);
        }

        return final;
    }
}