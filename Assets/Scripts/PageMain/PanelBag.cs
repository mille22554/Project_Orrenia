using System;
using System.Collections.Generic;
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
                Debug.Log(item.itemName.text);
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
                if (CheckIsPlayerEquip(item.info))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";
            }
            else if (ItemTypeCheck.IsUseType(item.info.type))
                textUse.text = "使用";
            btnUse.gameObject.SetActive(true);
        }
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

            GameData.NowPlayerData.CurrentHp += selectedBagItem.info.ability.HP;
            GameData.NowPlayerData.CurrentMp += selectedBagItem.info.ability.MP;
        }

        PublicFunc.SaveData();
    }

    private bool CheckIsPlayerEquip(ItemData item)
    {
        return item.type switch
        {
            EquipType.One_Hand_Weapon => GameData.NowPlayerData.equips.Right_Hand == item.uid,
            EquipType.Two_Hand_Weapon => GameData.NowPlayerData.equips.Right_Hand == item.uid && GameData.NowPlayerData.equips.Left_Hand == -1,
            EquipType.Shield => GameData.NowPlayerData.equips.Left_Hand == item.uid,
            EquipType.Helmet => GameData.NowPlayerData.equips.Helmet == item.uid,
            EquipType.Armor => GameData.NowPlayerData.equips.Armor == item.uid,
            EquipType.Greaves => GameData.NowPlayerData.equips.Greaves == item.uid,
            EquipType.Shoes => GameData.NowPlayerData.equips.Shoes == item.uid,
            EquipType.Gloves => GameData.NowPlayerData.equips.Gloves == item.uid,
            EquipType.Cape => GameData.NowPlayerData.equips.Cape == item.uid,
            EquipType.Ring => GameData.NowPlayerData.equips.Ring == item.uid,
            EquipType.Pendant => GameData.NowPlayerData.equips.Pendant == item.uid,
            _ => false,
        };
    }

    private void SwitchEquipStatus(ItemData item)
    {
        var isEquipped = CheckIsPlayerEquip(item);
        switch (item.type)
        {
            case EquipType.One_Hand_Weapon:
                UnloadEquip(GameData.NowPlayerData.equips.Right_Hand);
                GameData.NowPlayerData.equips.Right_Hand = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Two_Hand_Weapon:
                GameData.NowPlayerData.equips.Right_Hand = isEquipped ? 0 : item.uid;
                UnloadEquip(GameData.NowPlayerData.equips.Right_Hand);
                GameData.NowPlayerData.equips.Left_Hand = isEquipped ? 0 : -1;
                break;
            case EquipType.Shield:
                UnloadEquip(GameData.NowPlayerData.equips.Left_Hand);
                GameData.NowPlayerData.equips.Left_Hand = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Helmet:
                UnloadEquip(GameData.NowPlayerData.equips.Helmet);
                GameData.NowPlayerData.equips.Helmet = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Armor:
                UnloadEquip(GameData.NowPlayerData.equips.Armor);
                GameData.NowPlayerData.equips.Armor = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Greaves:
                UnloadEquip(GameData.NowPlayerData.equips.Greaves);
                GameData.NowPlayerData.equips.Greaves = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Shoes:
                UnloadEquip(GameData.NowPlayerData.equips.Shoes);
                GameData.NowPlayerData.equips.Shoes = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Gloves:
                UnloadEquip(GameData.NowPlayerData.equips.Gloves);
                GameData.NowPlayerData.equips.Gloves = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Cape:
                UnloadEquip(GameData.NowPlayerData.equips.Cape);
                GameData.NowPlayerData.equips.Cape = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Ring:
                UnloadEquip(GameData.NowPlayerData.equips.Ring);
                GameData.NowPlayerData.equips.Ring = isEquipped ? 0 : item.uid;
                break;
            case EquipType.Pendant:
                UnloadEquip(GameData.NowPlayerData.equips.Pendant);
                GameData.NowPlayerData.equips.Pendant = isEquipped ? 0 : item.uid;
                break;
        }

        if (!isEquipped)
        {
            PublicFunc.SetEquipAbility(item.ability);
            textUse.text = "卸下";
        }
        else
            textUse.text = "裝備";
    }

    private void UnloadEquip(long uid)
    {
        var item = GameData.NowBagData.items.Find(x => x.uid == uid);
        if (item != null)
            PublicFunc.SetEquipAbility(item.ability, true);
    }
}