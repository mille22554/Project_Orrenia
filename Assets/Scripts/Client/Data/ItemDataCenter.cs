using System;
using System.Collections.Generic;

public static class ItemDataCenter
{
    static Dictionary<int, ItemData> _itemData;
    static Dictionary<EItemKind, ItemKind> _itemKind;
    static List<int> _gameShopItem;

    static void RefreshData(Action callback)
    {
        if (_itemData == null || _itemKind == null || _gameShopItem == null)
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
}