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
            PanelLoading.Create(PanelLoading.BGType.None);
            var requestData = new GetSaveDataRequest
            {
                Account = DataCenter.Account,
            };
            APIController.Ins.Send(requestData, CallBack);

            void CallBack(GetSaveDataResponse response)
            {
                if (response.Code == 0)
                {
                    ResetBagInfo();
                    _gold.text = response.SaveData.PlayerData.Gold.ToString();
                    _btnShop.gameObject.SetActive(true);
                    _toggleSell.isOn = true;
                    _toggleBuy.isOn = true;
                }

                PanelLoading.Close();
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

        foreach (var itemID in DataCenter.GameShopItem)
        {
            var itemInfo = DataCenter.GetItemData(itemID);
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

        PanelLoading.Create(PanelLoading.BGType.None);
        var requestData = new GetSaveDataRequest
        {
            Account = DataCenter.Account,
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            if (response.Code == 0)
            {
                foreach (var itemInfo in response.SaveData.CharacterData.BagItems)
                {
                    var item = ObjectPool.Get(_shopItem, _itemList.content);
                    item.SetInfo(itemInfo, _toggleItems, RefreshBagInfo);
                    item.BagItemUID = itemInfo.UID;
                    _shopItemList.Add(item);
                }
            }

            PanelLoading.Close();
        }
    }

    void OnTrade()
    {
        if (_selectedShopItem == null || !int.TryParse(_inputTradeNum.text, out var itemNum) || itemNum == 0)
            return;

        PanelLoading.Create(PanelLoading.BGType.None);
        var requestData = new SetTradeActionRequest
        {
            Account = DataCenter.Account,
            TradeActionType = _toggleBuy.isOn ? ETradeActionType.Buy : ETradeActionType.Sell,
            ItemID = _selectedShopItem.Info.ID,
            TradeNum = itemNum,
            SelledItemUID = _selectedShopItem.BagItemUID
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(SetTradeActionResponse response)
        {
            if (response.Code == 0)
            {
                _gold.text = response.Gold.ToString();
                _inputTradeNum.text = "0";

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

            PanelLoading.Close();
        }
    }

    void RefreshBagInfo(ShopItem item, bool isOn)
    {
        if (isOn)
        {
            _selectedShopItem = item;
            _itemName.text = item.Info.Name;
            _type.text = DataCenter.GetItemKind(item.Info.Kind).Name;
            _price.transform.parent.gameObject.SetActive(true);
            var itemPrice = _toggleBuy.isOn ? item.Info.Price : item.Info.Price / 2;
            _price.text = $"{itemPrice}";
            _description.text = item.Info.Description;
            _inputTradeNum.text = "0";

            DataCenter.DoActionAccordingToCategory(item.Info.Kind, OtherCallBack, OtherCallBack, MaterialCallBack);

            void MaterialCallBack() => _ability.text = DataCenter.GetAbilityString(item.Info.Ability, 0);
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
        // else
        // {
        //     _inputTradeNum.text = "0";
        //     return;
        // }
    }

    void ClearList()
    {
        foreach (var item in _shopItemList)
            ObjectPool.Put(item);

        _shopItemList.Clear();
    }
}