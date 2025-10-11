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

        toggleSell.isOn = true;
        toggleSell.isOn = false;
        toggleBuy.isOn = true;
        toggleBuy.isOn = false;
    }

    private void Start()
    {
        shop.onClick.AddListener(OnShop);
        toggleBuy.onValueChanged.AddListener(OnSwitchBuy);
        toggleSell.onValueChanged.AddListener(OnSwitchSell);
        btnTrade.onClick.AddListener(OnTrade);
    }

    private void OnDestroy()
    {
        shop.onClick.RemoveListener(OnShop);
        toggleBuy.onValueChanged.RemoveListener(OnSwitchBuy);
        toggleSell.onValueChanged.RemoveListener(OnSwitchSell);
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
            gold.text = GameData.NowPlayerData.gold.ToString();
            toggleBuy.isOn = true;
            shop.gameObject.SetActive(true);
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

        foreach (var itemInfo in GameShopItem.list)
        {
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

        foreach (var itemInfo in GameData.NowBagData.items)
        {
            var item = Instantiate(shopItem, itemList.content);
            item.SetInfo(itemInfo);
            item.refreshBagInfo = RefreshBagInfo;
            item.toggle.group = toggleItems;
            item.toggle.isOn = true;
            item.toggle.isOn = false;
            shopItemList.Add(item);
        }
    }

    private void OnTrade()
    {
        if (selectedShopItem == null) return;

        if (toggleBuy.isOn)
        {
            if (GameData.NowPlayerData.gold >= selectedShopItem.info.price)
            {
                GameData.NowPlayerData.gold -= selectedShopItem.info.price;

                var existing = GameData.NowBagData.items.Find(item => item.id == selectedShopItem.info.id);
                if (ItemTypeCheck.IsEquipType(selectedShopItem.info.type) || existing == null)
                    GameData.NowBagData.items.Add(PublicFunc.GetItem(selectedShopItem.info));
                else existing.count++;
            }
        }
        else
        {
            GameData.NowPlayerData.gold += selectedShopItem.info.price / 2;

            var existing = GameData.NowBagData.items.Find(item => item.id == selectedShopItem.info.id);
            if (existing != null) existing.count--;

            if (ItemTypeCheck.IsEquipType(selectedShopItem.info.type) || existing?.count == 0)
            {
                GameData.NowBagData.items.Remove(selectedShopItem.info);
                shopItemList.Remove(selectedShopItem);
                Destroy(selectedShopItem.gameObject);
            }
            else
                selectedShopItem.SetInfo(existing);
        }
        gold.text = GameData.NowPlayerData.gold.ToString();

        PublicFunc.SaveData();
    }

    private void RefreshBagInfo(ShopItem item)
    {
        selectedShopItem = item;
        itemName.text = item.info.name;
        type.text = item.info.type;
        price.transform.parent.gameObject.SetActive(true);
        var itemPrice = toggleBuy.isOn ? item.info.price : item.info.price / 2;
        price.text = $"{itemPrice}";
        description.text = item.info.description;

        if (!ItemTypeCheck.IsMaterialType(item.info.type))
            ability.text = item.info.GetAbilityString();
        else ability.text = "";
    }

    private void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        price.transform.parent.gameObject.SetActive(false);
        price.text = "";
        description.text = "";
        ability.text = "";
    }
}