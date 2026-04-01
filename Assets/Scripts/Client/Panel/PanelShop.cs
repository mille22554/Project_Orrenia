using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelShop : MonoBehaviour
{
    [SerializeField] Toggle _toggleBuy;
    [SerializeField] Toggle _toggleSell;
    [SerializeField] Text _itemName;
    [SerializeField] Text _type;
    [SerializeField] Text _price;
    [SerializeField] Text _description;
    [SerializeField] Text _ability;
    [SerializeField] Text _gold;
    [SerializeField] InputField _inputTradeNum;
    [SerializeField] Button _btnTrade;
    [SerializeField] Text _textTrade;
    [SerializeField] ScrollRect _itemList;
    [SerializeField] ShopItem _shopItem;
    [SerializeField] Button _btnShop;
    [SerializeField] ToggleGroup _toggleItems;

    readonly List<ShopItem> _shopItemList = new();
    ShopItem _selectedShopItem;

    void Awake()
    {
        _btnShop.onClick.AddListener(OnShop);
        _toggleBuy.onValueChanged.AddListener(OnSwitchBuy);
        _toggleSell.onValueChanged.AddListener(OnSwitchSell);
        _inputTradeNum.onEndEdit.AddListener(OnInputEnd);
        _btnTrade.onClick.AddListener(OnTrade);
    }

    public void OnShop()
    {
        if (_btnShop.gameObject.activeSelf)
        {
            _btnShop.gameObject.SetActive(false);
        }
        else
        {
            var requestData = new GetSaveDataRequest();
            ApiBridge.Send(requestData, CallBack);

            void CallBack(GetSaveDataResponse response)
            {
                ResetBagInfo();
                _gold.text = response.SaveData.Datas.PlayerData.Gold.ToString();
                _btnShop.gameObject.SetActive(true);
                _toggleSell.isOn = true;
                _toggleBuy.isOn = true;
            }
        }
    }

    void OnSwitchBuy(bool isOn)
    {
        if (!isOn)
            return;

        ResetBagInfo();
        _textTrade.text = "購買";

        ClearList();

        foreach (var itemID in ItemDataCenter.GetShopList())
        {
            var itemInfo = ItemDataCenter.GetItemData(itemID);
            var item = ObjectPool.Get(_shopItem, _itemList.content);
            item.SetInfo(itemInfo, _toggleItems, RefreshBagInfo);
            _shopItemList.Add(item);
        }
    }

    void OnSwitchSell(bool isOn)
    {
        if (!isOn)
            return;

        ResetBagInfo();
        _textTrade.text = "販賣";

        ClearList();

        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            var bagData = response.SaveData.Datas.BagData;

            foreach (var itemInfo in bagData.Items)
            {
                var itemData = ItemDataCenter.GetItemData(itemInfo.ItemID);
                var item = ObjectPool.Get(_shopItem, _itemList.content);
                item.SetInfo(itemData, _toggleItems, RefreshBagInfo);
                item.BagItemUID = itemInfo.UID;
                _shopItemList.Add(item);
            }
        }
    }

    void OnTrade()
    {
        if (_selectedShopItem == null || !int.TryParse(_inputTradeNum.text, out var itemNum) || itemNum == 0)
            return;

        var requestData = new SetTradeActionRequest
        {
            TradeActionType = _toggleBuy.isOn ? ETradeActionType.Buy : ETradeActionType.Sell,
            ItemID = _selectedShopItem.Info.ID,
            TradeNum = itemNum,
            SelledItemUID = _selectedShopItem.BagItemUID
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetTradeActionResponse response)
        {
            _gold.text = response.Gold.ToString();

            if (_toggleSell.isOn)
            {
                if (response.SelledItemSurplus == 0)
                {
                    _shopItemList.Remove(_selectedShopItem);
                    _selectedShopItem.Remove();
                    _selectedShopItem = null;
                }
                else
                {
                    _selectedShopItem.UpdateItemCount(response.SelledItemSurplus);
                }
            }
        }
    }

    void RefreshBagInfo(ShopItem item, bool isOn)
    {
        if (isOn)
        {
            _selectedShopItem = item;
            _itemName.text = item.Info.Name;
            _type.text = ItemDataCenter.GetItemKind(item.Info.Kind).Name;
            _price.transform.parent.gameObject.SetActive(true);
            var itemPrice = _toggleBuy.isOn ? item.Info.Price : item.Info.Price / 2;
            _price.text = $"{itemPrice}";
            _description.text = item.Info.Description;
            _inputTradeNum.text = "0";

            ItemDataCenter.DoActionAccordingToCategory(item.Info.Kind, OtherCallBack, OtherCallBack, MaterialCallBack);

            void MaterialCallBack() => _ability.text = item.Info.GetAbilityString();
            void OtherCallBack() => _ability.text = "";
        }
        else
        {
            ResetBagInfo();
        }
    }

    void ResetBagInfo()
    {
        _itemName.text = "";
        _type.text = "";
        _price.text = "";
        _description.text = "";
        _ability.text = "";
        _inputTradeNum.text = "0";

        _price.transform.parent.gameObject.SetActive(false);
    }

    void OnInputEnd(string str)
    {
        if (_selectedShopItem == null || !int.TryParse(_inputTradeNum.text, out var itemNum) || itemNum < 0)
        {
            _inputTradeNum.text = "0";
            return;
        }


        if (_toggleBuy.isOn && int.TryParse(_gold.text, out var gold) && _selectedShopItem.Info.Price * itemNum > gold)
        {
            _inputTradeNum.text = (gold / _selectedShopItem.Info.Price).ToString();
        }
        else if (_toggleSell.isOn)
        {
            var haveNum = _selectedShopItem.Info.Count;
            if (itemNum > haveNum)
                _inputTradeNum.text = haveNum.ToString();
        }
        else
        {
            _inputTradeNum.text = "0";
            return;
        }
    }

    void ClearList()
    {
        foreach (var item in _shopItemList)
            ObjectPool.Put(item);

        _shopItemList.Clear();
    }
}