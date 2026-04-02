using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class ItemDataCenter_Server
{
    public static readonly Dictionary<int, ItemData> ItemData = new();
    public static Dictionary<EItemKind, ItemKind> ItemKind { get; private set; }
    public static List<int> GameShopItem { get; private set; }

    static ItemDataCenter_Server()
    {
        LoadItemData();
        LoadGameShopItem();
        LoadItemKind();
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
}