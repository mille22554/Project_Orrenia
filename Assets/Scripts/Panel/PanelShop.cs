using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelShop : MonoBehaviour
{
    public Toggle toggleBuy;
    public Toggle toggleSell;
    public Text itemName;
    public Text type;
    public Text price;
    public Text description;
    public Text ability;
    public Text gold;
    public InputField inputTradeNum;
    public Button btnTrade;
    public Text textTrade;
    public ScrollRect itemList;
    public ShopItem shopItem;

    private Button shop;
    private ToggleGroup toggleItems;
    private readonly List<ShopItem> shopItemList = new();
    private ShopItem selectedShopItem;

    private void Awake()
    {
        shop = GetComponent<Button>();
        toggleItems = itemList.content.GetComponent<ToggleGroup>();
    }

    private void Start()
    {
        toggleBuy.isOn = true;
        toggleSell.isOn = true;

        shop.onClick.AddListener(OnShop);
        toggleBuy.onValueChanged.AddListener(OnSwitchBuy);
        toggleSell.onValueChanged.AddListener(OnSwitchSell);
        inputTradeNum.onEndEdit.AddListener(OnInputEnd);
        btnTrade.onClick.AddListener(OnTrade);
    }

    private void OnDestroy()
    {
        shop.onClick.RemoveListener(OnShop);
        toggleBuy.onValueChanged.RemoveListener(OnSwitchBuy);
        toggleSell.onValueChanged.RemoveListener(OnSwitchSell);
        inputTradeNum.onEndEdit.RemoveListener(OnInputEnd);
        btnTrade.onClick.RemoveListener(OnTrade);
    }

    public void OnShop()
    {
        if (shop.gameObject.activeSelf)
        {
            shop.gameObject.SetActive(false);
        }
        else
        {
            ResetBagInfo();
            gold.text = GameData_Server.NowPlayerData.Gold.ToString();
            shop.gameObject.SetActive(true);
            toggleBuy.isOn = true;
        }
    }

    private void OnSwitchBuy(bool isOn)
    {
        if (!isOn) return;

        ResetBagInfo();
        textTrade.text = "購買";
        foreach (var item in shopItemList)
            Destroy(item.gameObject);
        shopItemList.Clear();

        foreach (var itemBaseInfo in GameShopItem.list)
        {
            var itemInfo = PublicFunc.GetItem(itemBaseInfo);
            var item = Instantiate(shopItem, itemList.content);
            item.SetInfo(itemInfo);
            item.refreshBagInfo = RefreshBagInfo;
            item.toggle.group = toggleItems;
            item.toggle.isOn = true;
            item.toggle.isOn = false;
            shopItemList.Add(item);
        }
    }

    private void OnSwitchSell(bool isOn)
    {
        if (!isOn) return;

        ResetBagInfo();
        textTrade.text = "販賣";
        foreach (var item in shopItemList)
            Destroy(item.gameObject);
        shopItemList.Clear();

        foreach (var itemInfo in GameData_Server.NowBagData.Items)
        {
            // if (PublicFunc.CheckIsPlayerEquip(itemInfo)) continue;

            // var item = Instantiate(shopItem, itemList.content);
            // item.SetInfo(itemInfo);
            // item.refreshBagInfo = RefreshBagInfo;
            // item.toggle.group = toggleItems;
            // item.toggle.isOn = true;
            // item.toggle.isOn = false;
            // shopItemList.Add(item);
        }
    }

    private void OnTrade()
    {
        if (selectedShopItem == null || !int.TryParse(inputTradeNum.text, out var itemNum) || itemNum == 0) return;

        if (toggleBuy.isOn)
        {
            if (GameData_Server.NowPlayerData.Gold >= ItemBaseData.Get(selectedShopItem.info.ItemID).Price * itemNum)
            {
                GameData_Server.NowPlayerData.Gold -= ItemBaseData.Get(selectedShopItem.info.ItemID).Price * itemNum;

                var existing = GameData_Server.NowBagData.Items.Find(item => item.ItemID == selectedShopItem.info.ItemID);
                if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(selectedShopItem.info.ItemID).Type))
                {
                    for (int i = 0; i < itemNum; i++)
                        GameData_Server.NowBagData.Items.Add(PublicFunc.GetItem(ItemBaseData.Get(selectedShopItem.info.ItemID)));
                }
                else if (existing == null)
                {
                    var buyItem = PublicFunc.GetItem(ItemBaseData.Get(selectedShopItem.info.ItemID));
                    buyItem.Count = itemNum;
                    GameData_Server.NowBagData.Items.Add(buyItem);
                }
                else
                {
                    existing.Count += itemNum;
                }
            }
        }
        else
        {
            GameData_Server.NowPlayerData.Gold += ItemBaseData.Get(selectedShopItem.info.ItemID).Price / 2 * itemNum;

            var existing = GameData_Server.NowBagData.Items.Find(item => item.UID == selectedShopItem.info.UID);
            if (existing != null)
                existing.Count -= itemNum;

            if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(selectedShopItem.info.ItemID).Type) || existing?.Count == 0)
            {
                GameData_Server.NowBagData.Items.Remove(selectedShopItem.info);
                shopItemList.Remove(selectedShopItem);
                Destroy(selectedShopItem.gameObject);
            }
            else
            {
                selectedShopItem.SetInfo(existing);
            }
        }
        gold.text = GameData_Server.NowPlayerData.Gold.ToString();

        PublicFunc.SaveData();
    }

    private void RefreshBagInfo(ShopItem item)
    {
        selectedShopItem = item;
        itemName.text = ItemBaseData.Get(item.info.ItemID).Name;
        type.text = ItemBaseData.Get(item.info.ItemID).Type;
        price.transform.parent.gameObject.SetActive(true);
        var itemPrice = toggleBuy.isOn ? ItemBaseData.Get(item.info.ItemID).Price : ItemBaseData.Get(item.info.ItemID).Price / 2;
        price.text = $"{itemPrice}";
        description.text = ItemBaseData.Get(item.info.ItemID).Description;
        inputTradeNum.text = "";

        if (!ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.info.ItemID).Type))
            ability.text = ItemBaseData.Get(item.info.ItemID).GetAbilityString();
        else
            ability.text = "";
    }

    private void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        price.transform.parent.gameObject.SetActive(false);
        price.text = "";
        description.text = "";
        ability.text = "";
        inputTradeNum.text = "";
    }

    private void OnInputEnd(string str)
    {
        if (selectedShopItem == null || !int.TryParse(inputTradeNum.text, out var itemNum)) return;

        if (itemNum < 0)
        {
            inputTradeNum.text = "0";
            return;
        }

        if (toggleBuy.isOn && ItemBaseData.Get(selectedShopItem.info.ItemID).Price * itemNum > GameData_Server.NowPlayerData.Gold)
            inputTradeNum.text = (GameData_Server.NowPlayerData.Gold / ItemBaseData.Get(selectedShopItem.info.ItemID).Price).ToString();
        else if (toggleSell.isOn)
        {
            var haveNum = GameData_Server.NowBagData.Items.Find(x => x.UID == selectedShopItem.info.UID).Count;
            if (itemNum > haveNum)
                inputTradeNum.text = haveNum.ToString();
        }
    }
}