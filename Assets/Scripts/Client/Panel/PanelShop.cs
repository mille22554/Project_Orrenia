using System.Collections.Generic;
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

    Button _btnShop;
    ToggleGroup _toggleItems;
    readonly List<ShopItem> _shopItemList = new();
    ShopItem _selectedShopItem;

    void Awake()
    {
        _btnShop = GetComponent<Button>();
        _toggleItems = _itemList.content.GetComponent<ToggleGroup>();

        _btnShop.onClick.AddListener(OnShop);
        _toggleBuy.onValueChanged.AddListener(OnSwitchBuy);
        _toggleSell.onValueChanged.AddListener(OnSwitchSell);
        _inputTradeNum.onEndEdit.AddListener(OnInputEnd);
        _btnTrade.onClick.AddListener(OnTrade);
    }

    void Start()
    {
        _toggleBuy.isOn = true;
        _toggleSell.isOn = true;
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
            TradeActionType = _toggleBuy.isOn ? TradeActionType.Buy : TradeActionType.Sell,
            ItemID = _selectedShopItem.Info.ID,
            TradeNum = itemNum,

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
                    ObjectPool.Put(_selectedShopItem);
                }
                else
                {
                    _selectedShopItem.UpdateItemCount(response.SelledItemSurplus);
                }
            }
        }
    }

    void RefreshBagInfo(ShopItem item)
    {
        // selectedShopItem = item;
        // itemName.text = ItemDataCenter.GetItemData(item.Info.ItemID).Name;
        // type.text = ItemDataCenter.GetItemData(item.Info.ItemID).Kind;
        // price.transform.parent.gameObject.SetActive(true);
        // var itemPrice = toggleBuy.isOn ? ItemDataCenter.GetItemData(item.Info.ItemID).Price : ItemDataCenter.GetItemData(item.Info.ItemID).Price / 2;
        // price.text = $"{itemPrice}";
        // description.text = ItemDataCenter.GetItemData(item.Info.ItemID).Description;
        // inputTradeNum.text = "";

        // if (!ItemTypeCheck.IsMaterialType(ItemDataCenter.GetItemData(item.Info.ItemID).Kind))
        //     ability.text = ItemDataCenter.GetItemData(item.Info.ItemID).GetAbilityString();
        // else
        //     ability.text = "";
    }

    void ResetBagInfo()
    {
        _itemName.text = "";
        _type.text = "";
        _price.text = "";
        _description.text = "";
        _ability.text = "";
        _inputTradeNum.text = "";

        _price.transform.parent.gameObject.SetActive(false);
    }

    void OnInputEnd(string str)
    {
        if (_selectedShopItem == null || !int.TryParse(_inputTradeNum.text, out var itemNum))
            return;

        if (itemNum < 0)
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
    }

    void ClearList()
    {
        foreach (var item in _shopItemList)
            ObjectPool.Put(item);

        _shopItemList.Clear();
    }
}