using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using static GameItemData;

public class PageBag : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageBag";
    [SerializeField] Text itemName;
    [SerializeField] Text type;
    [SerializeField] Text description;
    [SerializeField] Text ability;
    [SerializeField] Text gold;
    [SerializeField] Text textUse;
    [SerializeField] Button btnUse;
    [SerializeField] ScrollRect itemList;
    [SerializeField] BagItem bagItem;
    [SerializeField] Toggle toggleEquip;
    [SerializeField] Toggle toggleUse;
    [SerializeField] Toggle toggleMaterial;

    ToggleGroup toggleItems;
    readonly List<BagItem> bagItems = new();
    BagItem selectedBagItem;

    public static void Create()
    {
        var panel = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBag>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(panel);
    }

    void Awake()
    {
        toggleItems = itemList.content.GetComponent<ToggleGroup>();

        toggleEquip.onValueChanged.AddListener(SwitchToEquip);
        toggleUse.onValueChanged.AddListener(SwitchToUse);
        toggleMaterial.onValueChanged.AddListener(SwitchToMaterial);
        btnUse.onClick.AddListener(OnUse);

        foreach (Transform child in itemList.content)
            Destroy(child.gameObject);
    }

    void Start()
    {
        toggleMaterial.isOn = true;
        toggleUse.isOn = true;
        toggleEquip.isOn = true;
    }

    void OnEnable()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, OnGetSaveData);

        void OnGetSaveData(GetSaveDataResponse response)
        {
            var datas = response.SaveData.Datas;

            ResetBagInfo();
            btnUse.gameObject.SetActive(false);
            gold.text = datas.PlayerData.Gold.ToString();

            foreach (var itemInfo in datas.BagData.Items)
            {
                var item = ObjectPool.Get(bagItem, itemList.content);
                item.SetInfo(itemInfo, datas.CharacterData.Equips);
                item.RefreshBagInfo = RefreshBagInfo;
                item.Toggle.group = toggleItems;
                item.Toggle.isOn = true;
                item.Toggle.isOn = false;
                bagItems.Add(item);

                if (toggleEquip.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.Info.ItemID).Type));
                else if (toggleUse.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsUseType(ItemBaseData.Get(item.Info.ItemID).Type));
                else if (toggleMaterial.isOn)
                    item.gameObject.SetActive(ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.Info.ItemID).Type));
            }
        }
    }

    void OnDisable()
    {
        foreach (var item in bagItems)
            ObjectPool.Put(item);

        bagItems.Clear();
    }

    void SwitchToEquip(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.Info.ItemID).Type))
            {
                item.gameObject.SetActive(true);
                item.Toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void SwitchToUse(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsUseType(ItemBaseData.Get(item.Info.ItemID).Type))
            {
                item.gameObject.SetActive(true);
                item.Toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void SwitchToMaterial(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            if (ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.Info.ItemID).Type))
            {
                item.gameObject.SetActive(true);
                item.Toggle.isOn = false;
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    void RefreshBagInfo(BagItem item)
    {
        selectedBagItem = item;
        itemName.text = ItemBaseData.Get(item.Info.ItemID).Name;
        type.text = ItemBaseData.Get(item.Info.ItemID).Type;
        description.text = ItemBaseData.Get(item.Info.ItemID).Description;

        if (!ItemTypeCheck.IsMaterialType(ItemBaseData.Get(item.Info.ItemID).Type))
        {
            ability.text = ItemBaseData.Get(item.Info.ItemID).GetAbilityString();

            if (ItemTypeCheck.IsEquipType(ItemBaseData.Get(item.Info.ItemID).Type))
            {
                if (PublicFunc.CheckIsPlayerEquip(item.Info, item.Equips))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";
            }
            else if (ItemTypeCheck.IsUseType(ItemBaseData.Get(item.Info.ItemID).Type))
            {
                textUse.text = "使用";
            }
            btnUse.gameObject.SetActive(true);
        }
        else
        {
            ability.text = "";
        }
    }

    void ResetBagInfo()
    {
        itemName.text = "";
        type.text = "";
        description.text = "";
        ability.text = "";
        btnUse.gameObject.SetActive(false);
    }

    void OnUse()
    {
        var requestData = new SetItemActionRequest
        {
            ItemData = selectedBagItem.Info
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetItemActionResponse response)
        {
            switch (response.ItemType)
            {
                case 0:
                    SwitchEquipStatus(response.ItemData.UID, response.IsEquipped);
                    break;
                case 1:
                    UseItem(requestData.ItemData);
                    break;
            }

            if (response.Enemies.Count > 0)
            {
                var requestData = new GetBattleStatusRequest();
                ApiBridge.Send(requestData, CallBack);

                void CallBack(GetBattleStatusResponse response)
                {
                    var datas = response.SaveData.Datas;
                    var result = response.BattleResult;

                    if (result != null)
                    {
                        if (result.IsAttackerDead && datas.CharacterData.Name == result.Attacker ||
                            result.IsDefenderDead && datas.CharacterData.Name == result.Defenderer)
                        {
                            // LeaveDungon(datas.PlayerData.Area, datas.CharacterData);
                        }
                        else
                        {
                            var requestData = new GetBattleStatusRequest();
                            ApiBridge.Send(requestData, CallBack);
                        }
                    }

                    MainController.Instance.RefreshUI(datas.CharacterData);
                }
            }
        }
    }

    void SwitchEquipStatus(long uid, bool isEquipped)
    {
        if (isEquipped)
        {
            selectedBagItem.IconEquip.SetActive(false);

            textUse.text = "裝備";
        }
        else
        {
            var existingItem = bagItems.Find(x => x.Info.UID == uid);

            if (existingItem != null)
                existingItem.IconEquip.SetActive(false);

            selectedBagItem.IconEquip.SetActive(true);

            textUse.text = "卸下";
        }
    }

    void UseItem(ItemData itemData)
    {
        if (selectedBagItem.Info.Count == 0)
        {
            bagItems.Remove(selectedBagItem);
            ObjectPool.Put(selectedBagItem);
        }
        else
        {
            selectedBagItem.SetInfo(itemData);
        }
    }
}