using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public ItemData Info { get; private set; }
    public long BagItemUID;

    [SerializeField] GameObject _iconPick;
    [SerializeField] Text _itemName;
    [SerializeField] Text _count;
    [SerializeField] Toggle _toggle;
    [SerializeField] Action<ShopItem, bool> _refreshBagInfo;

    void Awake()
    {
        _toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    void Start()
    {
        _iconPick.SetActive(false);
    }

    public void SetInfo(ItemData data, ToggleGroup group, Action<ShopItem, bool> refreshBagInfo)
    {
        Info = data;
        _toggle.group = group;
        _refreshBagInfo = refreshBagInfo;

        _itemName.text = data.Name;

        ItemDataCenter.DoActionAccordingToCategory(data.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

        _toggle.isOn = false;

        void EquipCallBack() => _count.text = data.Durability.ToString();
        void OtherCallBack() => _count.text = data.Count.ToString();
    }

    public void UpdateItemCount(int count)
    {
        _count.text = count.ToString();
    }

    void OnToggleValueChange(bool isOn)
    {
        _iconPick.SetActive(isOn);
        _refreshBagInfo.Invoke(this, isOn);
    }
}