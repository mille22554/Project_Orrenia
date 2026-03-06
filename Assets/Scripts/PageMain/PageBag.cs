using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PageBag : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageBag";
    [SerializeField] Text itemName;
    [SerializeField] Text type;
    [SerializeField] Text description;
    [SerializeField] Text ability;
    [SerializeField] Text gold;
    [SerializeField] Text textUse;
    [SerializeField] Button btnUse;
    [SerializeField] ScrollRect itemList;
    [SerializeField] BagItem bagItem;
    [SerializeField] Toggle toggleEquip;
    [SerializeField] Toggle toggleUse;
    [SerializeField] Toggle toggleMaterial;

    ToggleGroup toggleItems;
    readonly List<BagItem> bagItems = new();
    BagItem selectedBagItem;
    PlayerData _playerData => GameData.NowPlayerData;

    public static void Create()
    {
        var panel = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBag>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(panel);
    }

    void Awake()
    {
        toggleItems = itemList.content.GetComponent<ToggleGroup>();

        toggleEquip.onValueChanged.AddListener(SwitchToEquip);
        toggleUse.onValueChanged.AddListener(SwitchToUse);
        toggleMaterial.onValueChanged.AddListener(SwitchToMaterial);
        btnUse.onClick.AddListener(OnUse);
        
        foreach (Transform child in itemList.content)
            Destroy(child.gameObject);
    }

    void Start()
    {
        toggleMaterial.isOn = true;
        toggleUse.isOn = true;
        toggleEquip.isOn = true;
    }

    void OnEnable()
    {
        if (GameData.gameData != null && _playerData != null)
        {
            ResetBagInfo();
            btnUse.gameObject.SetActive(false);
            gold.text = _playerData.Gold.ToString();

            foreach (var itemInfo in GameData.NowBagData.items)
            {
                var item = ObjectPool.Get(bagItem, itemList.content);
                item.SetInfo(itemInfo);
                item.refreshBagInfo = RefreshBagInfo;
                item.toggle.group = toggleItems;
                item.toggle.isOn = true;
                item.toggle.isOn = false;
                bagItems.Add(item);
                if (toggleEquip.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.info.itemID).type));
                else if (toggleUse.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsUseType(ItemBaseData.Get(item.info.itemID).type));
                else if (toggleMaterial.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.info.itemID).type));
            }
        }
    }

    void OnDisable()
    {
        foreach (var item in bagItems)
            ObjectPool.Put(item);

        bagItems.Clear();
    }

    void SwitchToEquip(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.info.itemID).type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void SwitchToUse(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsUseType(ItemBaseData.Get(item.info.itemID).type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void SwitchToMaterial(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.info.itemID).type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void RefreshBagInfo(BagItem item)
    {
        selectedBagItem = item;
        itemName.text = ItemBaseData.Get(item.info.itemID).name;
        type.text = ItemBaseData.Get(item.info.itemID).type;
        description.text = ItemBaseData.Get(item.info.itemID).description;

        if (!ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.info.itemID).type))
        {
            ability.text = ItemBaseData.Get(item.info.itemID).GetAbilityString();
            if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.info.itemID).type))
            {
                if (PublicFunc.CheckIsPlayerEquip(item.info))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";
            }
            else if (ItemTypeCheck.IsUseType(ItemBaseData.Get(item.info.itemID).type))
            {
                textUse.text = "使用";
            }
            btnUse.gameObject.SetActive(true);
        }
        else
        {
            ability.text = "";
        }
    }

    void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        description.text = "";
        ability.text = "";
        btnUse.gameObject.SetActive(false);
    }

    void OnUse()
    {
        if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(selectedBagItem.info.itemID).type))
        {
            SwitchEquipStatus(selectedBagItem.info);
        }
        else if (ItemTypeCheck.IsUseType(ItemBaseData.Get(selectedBagItem.info.itemID).type))
        {
            selectedBagItem.info.count--;
            if (selectedBagItem.info.count == 0)
            {
                bagItems.Remove(selectedBagItem);
                GameData.NowBagData.items.Remove(selectedBagItem.info);
                ObjectPool.Put(selectedBagItem);
            }
            else
            {
                selectedBagItem.SetInfo(selectedBagItem.info);
            }

            if (ItemBaseData.Get(selectedBagItem.info.itemID).ability.HP != 0)
                PublicFunc.SetHP(_playerData.CurrentHp + ItemBaseData.Get(selectedBagItem.info.itemID).ability.HP);

            if (ItemBaseData.Get(selectedBagItem.info.itemID).ability.MP != 0)
                PublicFunc.SetMP(_playerData.CurrentMp + ItemBaseData.Get(selectedBagItem.info.itemID).ability.MP);

            if (ItemBaseData.Get(selectedBagItem.info.itemID).ability.STA != 0)
                PublicFunc.SetCurrentSTA(_playerData.CurrentSTA + ItemBaseData.Get(selectedBagItem.info.itemID).ability.STA);

            UseItemSpecial(ItemBaseData.Get(selectedBagItem.info.itemID).name);

            foreach (var effectAction in _playerData.effectActions.ToList())
                effectAction.Invoke(false);
        }

        if (_playerData.CurrentTp >= GameData.tpCost)
            _playerData.CurrentTp -= GameData.tpCost;

        PublicFunc.SaveData();
    }

    void SwitchEquipStatus(ItemData item)
    {
        var isEquipped = PublicFunc.CheckIsPlayerEquip(item);
        RefreshEquipState(ItemBaseData.Get(item.itemID).type, item, isEquipped);

        if (!isEquipped)
        {
            PublicFunc.SetEquipAbility(ItemBaseData.Get(item.itemID).ability);
            textUse.text = "卸下";
        }
        else
            textUse.text = "裝備";
    }

    void RefreshEquipState(string type, ItemData item, bool isEquipped)
    {
        string fieldName = type switch
        {
            EquipType.One_Hand_Weapon.Sword or EquipType.One_Hand_Weapon.Dagger => "Right_Hand",
            EquipType.Shield => "Left_Hand",
            EquipType.Helmet => "Helmet",
            EquipType.Armor => "Armor",
            EquipType.Greaves => "Greaves",
            EquipType.Shoes => "Shoes",
            EquipType.Gloves => "Gloves",
            EquipType.Cape => "Cape",
            EquipType.Ring => "Ring",
            EquipType.Pendant => "Pendant",
            _ => null
        };

        if (fieldName == null)
            return;

        var equips = _playerData.equips;
        var field = typeof(EquipBase).GetField(fieldName);
        long currentUid = (long)field.GetValue(equips);

        // 解除當前裝備
        PublicFunc.UnloadEquip(currentUid);

        if (isEquipped)
        {
            field.SetValue(equips, 0L);
            selectedBagItem.iconEquip.SetActive(false);
        }
        else
        {
            var existingItem = bagItems.Find(x => x.info.uid == currentUid);
            if (existingItem != null)
                existingItem.iconEquip.SetActive(false);

            field.SetValue(equips, item.uid);
            selectedBagItem.iconEquip.SetActive(true);
        }
    }

    void UseItemSpecial(string itemName)
    {
        if (itemName == GameItem.Use.BerserkPotion.name)
        {
            PublicFunc.AddPlayerEffect(EffectType.Buff.Berserk, 2, 100);
        }
    }
}