using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public GameObject iconPick;
    public Text itemName;
    public Text count;
    public Toggle toggle;
    public Action<ShopItem> refreshBagInfo;
    [NonSerialized] public ItemData info;

    private void Start()
    {
        iconPick.SetActive(false);

        toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChange);
    }

    public void SetInfo(ItemData data)
    {
        info = data;
        itemName.text = ItemBaseData.Get(data.ItemID).Name;
        if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(data.ItemID).Type))
            count.text = data.Durability.ToString();
        else
            count.text = data.Count.ToString();
    }

    private void OnToggleValueChange(bool isOn)
    {
        iconPick.SetActive(isOn);
        refreshBagInfo.Invoke(this);
    }
}