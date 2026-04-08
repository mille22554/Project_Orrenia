using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class ItemDataCenter_Server
{
    public static readonly Dictionary<int, ItemData> ItemData = new();
    public static Dictionary<EItemKind, ItemKind> ItemKind { get; private set; }
    public static List<QualityData> QualityData { get; private set; }
    public static List<int> GameShopItem { get; private set; }

    static ItemDataCenter_Server()
    {
        LoadItemData();
        LoadGameShopItem();
        LoadItemKind();
        LoadQualityData();
    }

    static void LoadItemData()
    {
        var path = GameData_Server.ItemDataPath;
        Debug.Log($"從 {path} 讀取物品資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var datas = JsonConvert.DeserializeObject<List<ItemData>>(json);
            foreach (var data in datas)
            {
                ItemData.Add(data.ID, data);
            }
        }
        else
        {
            Debug.LogError("ItemData檔案丟失!");
        }
    }

    static void LoadGameShopItem()
    {
        var path = GameData_Server.GameShopItemPath;
        Debug.Log($"從 {path} 讀取商店資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            GameShopItem = JsonConvert.DeserializeObject<List<int>>(json);
        }
        else
        {
            Debug.LogError("GameShopItem檔案丟失!");
        }
    }

    static void LoadItemKind()
    {
        var path = GameData_Server.ItemKindPath;
        Debug.Log($"從 {path} 讀取物品類別資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            ItemKind = JsonConvert.DeserializeObject<Dictionary<EItemKind, ItemKind>>(json);
        }
        else
        {
            Debug.LogError("ItemKind檔案丟失!");
        }
    }

    static void LoadQualityData()
    {
        var path = GameData_Server.QualityDataPath;
        Debug.Log($"從 {path} 讀取品質資料");

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            QualityData = JsonConvert.DeserializeObject<List<QualityData>>(json);
        }
        else
        {
            Debug.LogError("QualityData檔案丟失!");
        }
    }

    public static ItemData GetItemData(int id)
    {
        ItemData.TryGetValue(id, out var item);
        return item;
    }

    public static bool IsWeapon(EItemKind kind) => IsWeapon(GetItemKind(kind).Category);
    public static bool IsWeapon(EItemCategory category)
    {
        return category switch
        {
            EItemCategory.One_Hand or
            EItemCategory.Two_Hand => true,
            _ => false,
        };
    }

    public static ItemKind GetItemKind(EItemKind kind)
    {
        ItemKind.TryGetValue(kind, out var itemKind);
        return itemKind;
    }

    public static BagItemData GetNewItemByItemID(int id) => GetNewItem(GetItemData(id));
    public static BagItemData GetNewItem(ItemData source)
    {
        var target = new BagItemData
        {
            UID = Math.Abs(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0)),
            ID = source.ID,
            Name = source.Name,
            Kind = source.Kind,
            Description = source.Description,
            Ability = source.Ability,
            Price = source.Price,
            Durability = source.Durability,
            Count = source.Count,
        };
        return target;
    }

    public static void DoActionAccordingToCategory(EItemKind kind, Action equipCallBack, Action useCallBack, Action materialCallBack)
        => PublicFunc.DoActionAccordingToCategory(GetItemKind(kind).Category, equipCallBack, useCallBack, materialCallBack);

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

        final.STR = (int)(tempSTR * (1 + final.STR / 10m));
        final.DEX = (int)(tempDEX * (1 + final.DEX / 10m));
        final.INT = (int)(tempINT * (1 + final.INT / 10m));
        final.VIT = (int)(tempVIT * (1 + final.VIT / 10m));
        final.AGI = (int)(tempAGI * (1 + final.AGI / 10m));
        final.LUK = (int)(tempLUK * (1 + final.LUK / 10m));
        final.HP = (int)(tempHP * (1 + final.HP / 10m));
        final.MP = (int)(tempMP * (1 + final.MP / 10m));
        final.STA = (int)(tempSTA * (1 + final.STA / 10m));
        final.ATK = (int)(tempATK * (1 + final.ATK / 10m));
        final.MATK = (int)(tempMATK * (1 + final.MATK / 10m));
        final.DEF = (int)(tempDEF * (1 + final.DEF / 10m));
        final.MDEF = (int)(tempMDEF * (1 + final.MDEF / 10m));
        final.ACC = (int)(tempACC * (1 + final.ACC / 10m));
        final.EVA = (int)(tempEVA * (1 + final.EVA / 10m));
        final.CRIT = (int)(tempCRIT * (1 + final.CRIT / 10m));
        final.SPD = (int)(tempSPD * (1 + final.SPD / 10m));

        var qualityData = QualityData.ElementAtOrDefault((int)itemData.Quality);
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