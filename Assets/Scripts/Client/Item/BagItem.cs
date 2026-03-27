using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    public Toggle Toggle;
    public Action<BagItem> RefreshBagInfo;
    public List<long> Equips;
    public GameObject IconEquip;
    [NonSerialized] public BagItemData Info;

    [SerializeField] Text itemName;
    [SerializeField] Text count;
    [SerializeField] GameObject iconPick;

    void Start()
    {
        iconPick.SetActive(false);

        Toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    public void SetInfo(BagItemData data) => SetInfo(data, null);
    public void SetInfo(BagItemData data, List<long> equips)
    {
        Info = data;
        if (equips != null)
            Equips = equips;

        var itemData = ItemDataCenter.GetItemData(data.ItemID);

        itemName.text = itemData.Name;

        ItemDataCenter.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

        IconEquip.SetActive(equips.Contains(data.UID));

        void EquipCallBack() => count.text = data.Durability.ToString();
        void OtherCallBack() => count.text = data.Count.ToString();
    }

    void OnToggleValueChange(bool isOn)
    {
        iconPick.SetActive(isOn);
        RefreshBagInfo.Invoke(this);
    }
}