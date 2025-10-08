using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelBag : MonoBehaviour
{
    public Text itemName;
    public Text type;
    public Text description;
    public Text ability;
    public Text gold;
    public ScrollRect itemList;
    public BagItem bagItem;
    public Toggle toggleEquip;
    public Toggle toggleUse;
    public Toggle toggleMaterial;

    private ToggleGroup toggleItems;
    private readonly List<BagItem> bagItems = new();

    private void Start()
    {
        toggleItems = itemList.content.GetComponent<ToggleGroup>();
        foreach (Transform child in itemList.content)
            Destroy(child.gameObject);

        toggleMaterial.isOn = true;
        toggleUse.isOn = true;
        toggleEquip.isOn = true;

        toggleEquip.onValueChanged.AddListener(SwitchToEquip);
        toggleUse.onValueChanged.AddListener(SwitchToUse);
        toggleMaterial.onValueChanged.AddListener(SwitchToMaterial);
    }

    private void OnDestroy()
    {
        toggleEquip.onValueChanged.RemoveListener(SwitchToEquip);
        toggleUse.onValueChanged.RemoveListener(SwitchToUse);
        toggleMaterial.onValueChanged.RemoveListener(SwitchToMaterial);
    }

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
        {
            ResetBagInfo();
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
                if (item.toggle.isOn) RefreshBagInfo(item.info);
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
                if (item.toggle.isOn) RefreshBagInfo(item.info);
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
                if (item.toggle.isOn) RefreshBagInfo(item.info);
            }
            else
                item.gameObject.SetActive(false);
        }
    }

    private void RefreshBagInfo(ItemData data)
    {
        itemName.text = data.name;
        type.text = data.type;
        description.text = data.description;
        if (ItemTypeCheck.IsEquipType(data.type))
            ability.text = data.GetAbilityString();
    }

    private void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        description.text = "";
        ability.text = "";
    }
}