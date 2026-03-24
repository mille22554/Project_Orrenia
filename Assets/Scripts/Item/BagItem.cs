using System;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    public Toggle Toggle;
    public Action<BagItem> RefreshBagInfo;
    public EquipBase Equips;
    public GameObject IconEquip;
    [NonSerialized] public ItemData Info;

    [SerializeField] Text itemName;
    [SerializeField] Text count;
    [SerializeField] GameObject iconPick;

    void Start()
    {
        iconPick.SetActive(false);

        Toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    public void SetInfo(ItemData data) => SetInfo(data, null);
    public void SetInfo(ItemData data, EquipBase equips)
    {
        Info = data;
        Equips = equips;

        itemName.text = ItemBaseData.Get(data.ItemID).Name;

        if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(data.ItemID).Type))
            count.text = data.Durability.ToString();
        else
            count.text = data.Count.ToString();

        IconEquip.SetActive(PublicFunc.CheckIsPlayerEquip(Info, equips));
    }

    void OnToggleValueChange(bool isOn)
    {
        iconPick.SetActive(isOn);
        RefreshBagInfo.Invoke(this);
    }
}