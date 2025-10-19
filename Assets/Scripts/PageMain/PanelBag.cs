using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class PanelBag : MonoBehaviour
{
    public Text itemName;
    public Text type;
    public Text description;
    public Text ability;
    public Text gold;
    public Text textUse;
    public Button btnUse;
    public ScrollRect itemList;
    public BagItem bagItem;
    public Toggle toggleEquip;
    public Toggle toggleUse;
    public Toggle toggleMaterial;

    private ToggleGroup toggleItems;
    private readonly List<BagItem> bagItems = new();
    private BagItem selectedBagItem;

    private void Awake()
    {
        toggleItems = itemList.content.GetComponent<ToggleGroup>();
        foreach (Transform child in itemList.content)
            Destroy(child.gameObject);
    }

    private void Start()
    {
        toggleMaterial.isOn = true;
        toggleUse.isOn = true;
        toggleEquip.isOn = true;

        toggleEquip.onValueChanged.AddListener(SwitchToEquip);
        toggleUse.onValueChanged.AddListener(SwitchToUse);
        toggleMaterial.onValueChanged.AddListener(SwitchToMaterial);
        btnUse.onClick.AddListener(OnUse);
    }

    private void OnDestroy()
    {
        toggleEquip.onValueChanged.RemoveListener(SwitchToEquip);
        toggleUse.onValueChanged.RemoveListener(SwitchToUse);
        toggleMaterial.onValueChanged.RemoveListener(SwitchToMaterial);
        btnUse.onClick.RemoveListener(OnUse);
    }

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
        {
            ResetBagInfo();
            btnUse.gameObject.SetActive(false);
            gold.text = GameData.NowPlayerData.gold.ToString();

            foreach (var itemInfo in GameData.NowBagData.items)
            {
                var item = Instantiate(bagItem, itemList.content);
                item.SetInfo(itemInfo);
                item.refreshBagInfo = RefreshBagInfo;
                item.toggle.group = toggleItems;
                item.toggle.isOn = true;
                item.toggle.isOn = false;
                bagItems.Add(item);
                if (toggleEquip.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsEquipType(item.info.type));
                else if (toggleUse.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsUseType(item.info.type));
                else if (toggleMaterial.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsMaterialType(item.info.type));
            }
        }
    }

    private void OnDisable()
    {
        foreach (var item in bagItems)
            Destroy(item.gameObject);
        bagItems.Clear();
    }

    private void SwitchToEquip(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsEquipType(item.info.type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
                item.gameObject.SetActive(false);
        }
    }

    private void SwitchToUse(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsUseType(item.info.type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
                item.gameObject.SetActive(false);
        }
    }

    private void SwitchToMaterial(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsMaterialType(item.info.type))
            {
                item.gameObject.SetActive(true);
                item.toggle.isOn = false;
            }
            else
                item.gameObject.SetActive(false);
        }
    }

    private void RefreshBagInfo(BagItem item)
    {
        selectedBagItem = item;
        itemName.text = item.info.name;
        type.text = item.info.type;
        description.text = item.info.description;

        if (!ItemTypeCheck.IsMaterialType(item.info.type))
        {
            ability.text = item.info.GetAbilityString();
            if (ItemTypeCheck.IsEquipType(item.info.type))
            {
                if (PublicFunc.CheckIsPlayerEquip(item.info))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";
            }
            else if (ItemTypeCheck.IsUseType(item.info.type))
                textUse.text = "使用";
            btnUse.gameObject.SetActive(true);
        }
        else ability.text = "";
    }

    private void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        description.text = "";
        ability.text = "";
        btnUse.gameObject.SetActive(false);
    }

    private void OnUse()
    {
        if (ItemTypeCheck.IsEquipType(selectedBagItem.info.type))
            SwitchEquipStatus(selectedBagItem.info);
        else if (ItemTypeCheck.IsUseType(selectedBagItem.info.type))
        {
            selectedBagItem.info.count--;
            if (selectedBagItem.info.count == 0)
            {
                bagItems.Remove(selectedBagItem);
                GameData.NowBagData.items.Remove(selectedBagItem.info);
                Destroy(selectedBagItem.gameObject);
            }
            else selectedBagItem.SetInfo(selectedBagItem.info);

            if (selectedBagItem.info.ability.HP != 0)
                GameData.NowPlayerData.CurrentHp += selectedBagItem.info.ability.HP;
            if (selectedBagItem.info.ability.MP != 0)
                GameData.NowPlayerData.CurrentMp += selectedBagItem.info.ability.MP;
            if (selectedBagItem.info.ability.STA != 0)
                GameData.NowPlayerData.SetCurrentSTA(GameData.NowPlayerData.currentSTA + selectedBagItem.info.ability.STA);

            UseItemSpecial(selectedBagItem.info.name);

            foreach (var effectAction in GameData.NowPlayerData.effectActions.ToList())
                effectAction.Invoke(false);
        }
        if (GameData.NowPlayerData.currentTp >= GameData.tpCost)
            GameData.NowPlayerData.currentTp -= GameData.tpCost;

        PublicFunc.SaveData();
    }

    private void SwitchEquipStatus(ItemData item)
    {
        var isEquipped = PublicFunc.CheckIsPlayerEquip(item);
        RefreshEquipState(item.type, item, isEquipped);

        if (!isEquipped)
        {
            PublicFunc.SetEquipAbility(item.ability);
            textUse.text = "卸下";
        }
        else
            textUse.text = "裝備";
    }

    private void RefreshEquipState(string type, ItemData item, bool isEquipped)
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

        var equips = GameData.NowPlayerData.equips;
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

    private void UseItemSpecial(string itemName)
    {
        if (itemName == GameItem.Use.BerserkPotion.name)
        {
            PublicFunc.AddPlayerEffect(EffectType.Buff.Berserk, 2, 100);
        }
    }
}