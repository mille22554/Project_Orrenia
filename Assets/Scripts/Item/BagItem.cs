using System;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    public GameObject iconPick;
    public Text itemName;
    public Text count;
    public Toggle toggle;
    public GameObject iconEquip;
    public Action<BagItem> refreshBagInfo;
    [NonSerialized] public ItemData info;

    void Start()
    {
        iconPick.SetActive(false);

        toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChange);
    }

    public void SetInfo(ItemData data)
    {
        info = data;
        itemName.text = ItemBaseData.Get(data.itemID).name;
        if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(data.itemID).type))
            count.text = data.durability.ToString();
        else
            count.text = data.count.ToString();

        iconEquip.SetActive(PublicFunc.CheckIsPlayerEquip(info));
    }

    void OnToggleValueChange(bool isOn)
    {
        iconPick.SetActive(isOn);
        refreshBagInfo.Invoke(this);
    }
}