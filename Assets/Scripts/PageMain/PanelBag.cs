using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelBag : MonoBehaviour
{
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
            gold.text = GameData.NowPlayerData.gold.ToString();
            foreach (var itemInfo in GameData.NowBagData.items)
            {
                var item = Instantiate(bagItem, itemList.content);
                item.SetInfo(itemInfo);
                item.toggle.group = toggleItems;
                item.toggle.isOn = true;
                bagItems.Add(item);
                if (toggleEquip.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsEquipType(item.info.type));
                else if(toggleUse.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsUseType(item.info.type));
                else if(toggleMaterial.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsMaterialType(item.info.type));
            }
            if (bagItems.Count != 0)
                bagItems[^1].toggle.isOn = false;
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
        foreach (var item in bagItems)
        {
            item.gameObject.SetActive(ItemTypeCheck.IsEquipType(item.info.type));
        }
    }

    private void SwitchToUse(bool isOn)
    {
        foreach (var item in bagItems)
        {
            item.gameObject.SetActive(ItemTypeCheck.IsUseType(item.info.type));
        }
    }

    private void SwitchToMaterial(bool isOn)
    {
        foreach (var item in bagItems)
        {
            item.gameObject.SetActive(ItemTypeCheck.IsMaterialType(item.info.type));
        }
    }
}