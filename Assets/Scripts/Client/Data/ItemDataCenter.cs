using System;
using System.Collections.Generic;

public static class ItemDataCenter
{
    static Dictionary<int, ItemData> _itemData;
    static Dictionary<EItemKind, ItemKind> _itemKind;
    static List<int> _gameShopItem;

    public static void RefreshData(Action callback)
    {
        var requestData = new GetItemDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetItemDataResponse response)
        {
            _itemData = response.ItemData;
            _itemKind = response.ItemKind;
            _gameShopItem = response.GameShopItem;

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

    public static ItemKind GetItemKindByItemID(int id) => GetItemKind(GetItemData(id).Kind);
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
}