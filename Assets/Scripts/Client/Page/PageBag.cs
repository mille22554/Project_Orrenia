using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            var datas = response.SaveData.Datas;

            ResetBagInfo();
            btnUse.gameObject.SetActive(false);
            gold.text = datas.PlayerData.Gold.ToString();

            foreach (var itemInfo in datas.BagData.Items)
            {
                var item = ObjectPool.Get(bagItem, itemList.content);
                item.SetInfo(itemInfo, toggleItems, RefreshBagInfo, datas.CharacterData.Equips);
                bagItems.Add(item);

                var itemKind = ItemDataCenter.GetItemKind(item.Info.Kind);

                if (toggleEquip.isOn)
                    item.gameObject.SetActive(PublicFunc.IsEquipCategory(itemKind.Category));
                else if (toggleUse.isOn)
                    item.gameObject.SetActive(PublicFunc.IsUseCategory(itemKind.Category));
                else if (toggleMaterial.isOn)
                    item.gameObject.SetActive(PublicFunc.IsMaterialCategory(itemKind.Category));
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
            PublicFunc.DoActionAccordingToCategory
            (
                ItemDataCenter.GetItemKind(item.Info.Kind).Category,
                EquipCallBack,
                OtherCallBack,
                OtherCallBack
            );

            void EquipCallBack() => item.Show();
            void OtherCallBack() => item.gameObject.SetActive(false);
        }
    }

    void SwitchToUse(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            PublicFunc.DoActionAccordingToCategory
            (
                ItemDataCenter.GetItemKind(item.Info.Kind).Category,
                OtherCallBack,
                UseCallBack,
                OtherCallBack
            );

            void UseCallBack() => item.Show();
            void OtherCallBack() => item.gameObject.SetActive(false);
        }
    }

    void SwitchToMaterial(bool isOn)
    {
        ResetBagInfo();
        foreach (var item in bagItems)
        {
            PublicFunc.DoActionAccordingToCategory
            (
                ItemDataCenter.GetItemKind(item.Info.Kind).Category,
                OtherCallBack,
                OtherCallBack,
                MaterialCallBack
            );

            void MaterialCallBack() => item.Show();
            void OtherCallBack() => item.gameObject.SetActive(false);
        }
    }

    void RefreshBagInfo(BagItem item, bool isOn)
    {
        if (isOn)
        {
            selectedBagItem = item;

            var itemKind = ItemDataCenter.GetItemKind(item.Info.Kind);
            var itemQuality = ItemDataCenter.GetQualityData(item.Info.Quality);

            itemName.text = itemQuality.Name + item.Info.Name;
            itemName.color = PublicFunc.SetColorFromHex(itemQuality.Color);

            type.text = itemKind.Name;
            description.text = item.Info.Description;

            PublicFunc.DoActionAccordingToCategory(itemKind.Category, EquipCallBack, UseCallBack, MaterialCallBack);

            void EquipCallBack()
            {
                if (item.Equips.Contains(item.Info.UID))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";

                ability.text = ItemDataCenter.GetAbilityString(item.Info);
                btnUse.gameObject.SetActive(true);
            }

            void UseCallBack()
            {
                textUse.text = "使用";
                ability.text = ItemDataCenter.GetAbilityString(item.Info);
                btnUse.gameObject.SetActive(true);
            }

            void MaterialCallBack()
            {
                ability.text = "";
                btnUse.gameObject.SetActive(false);
            }
        }
        else
        {
            ResetBagInfo();
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
            BagItemData = selectedBagItem.Info
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetItemActionResponse response)
        {
            PublicFunc.DoActionAccordingToCategory(response.ItemCategory, EquipCallBack, UseCallBack, null);

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

            void EquipCallBack() => SwitchEquipStatus(response.BagItemData.UID, response.IsEquipped);
            void UseCallBack() => UseItem(requestData.BagItemData);
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

    void UseItem(BagItemData bagItemData)
    {
        if (bagItemData.Count == 0)
        {
            bagItems.Remove(selectedBagItem);
            selectedBagItem.Remove();
        }
        else
        {
            selectedBagItem.UpdateItemCount(bagItemData.Count);
        }
    }
}