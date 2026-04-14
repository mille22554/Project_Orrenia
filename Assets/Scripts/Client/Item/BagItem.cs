using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    public GameObject IconEquip;
    [NonSerialized] public BagItemData Info;

    [SerializeField] Text _itemName;
    [SerializeField] Text _count;
    [SerializeField] Toggle _toggle;
    [SerializeField] GameObject _iconPick;

    Action<BagItem, bool> _refreshBagInfo;

    void Start()
    {
        _iconPick.SetActive(false);

        _toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    public void SetInfo(BagItemData data, ToggleGroup group, Action<BagItem, bool> refreshBagInfo,bool isEquiped)
    {
        Info = data;

        _itemName.text = data.Name;
        _toggle.group = group;
        _refreshBagInfo = refreshBagInfo;
        _toggle.isOn = false;

        DataCenter.DoActionAccordingToCategory(data.Kind, EquipCallBack, OtherCallBack, OtherCallBack);

        IconEquip.SetActive(isEquiped);

        void EquipCallBack() => _count.text = data.Durability.ToString();
        void OtherCallBack() => _count.text = data.Count.ToString();
    }

    public void UpdateItemCount(int count)
    {
        _count.text = count.ToString();
    }

    public void Show()
    {
        _toggle.isOn = false;
        gameObject.SetActive(true);
    }

    public void Remove()
    {
        _toggle.isOn = false;
        ObjectPool.Put(this);
    }

    void OnToggleValueChange(bool isOn)
    {
        _iconPick.SetActive(isOn);
        _refreshBagInfo.Invoke(this, isOn);
    }
}