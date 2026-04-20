using System.Collections.Generic;
using System.Linq;
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
    List<long> equips;

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
        PanelLoading.Create();

        void CallBack(GetSaveDataResponse response)
        {
            var datas = response.SaveData.Datas;
            var characterData = datas.CharacterData;

            ResetBagInfo();
            btnUse.gameObject.SetActive(false);
            gold.text = datas.PlayerData.Gold.ToString();
            equips = characterData.Equips;

            foreach (var itemInfo in characterData.BagItems)
            {
                var item = ObjectPool.Get(bagItem, itemList.content);
                item.SetInfo(itemInfo, toggleItems, RefreshBagInfo, equips.Contains(itemInfo.UID));
                bagItems.Add(item);

                var itemKind = DataCenter.GetItemKind(item.Info.Kind);

                if (toggleEquip.isOn)
                    item.gameObject.SetActive(PublicFunc.IsEquipCategory(itemKind.Category));
                else if (toggleUse.isOn)
                    item.gameObject.SetActive(PublicFunc.IsUseCategory(itemKind.Category));
                else if (toggleMaterial.isOn)
                    item.gameObject.SetActive(PublicFunc.IsMaterialCategory(itemKind.Category));
            }

            PanelLoading.Close();
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
                DataCenter.GetItemKind(item.Info.Kind).Category,
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
                DataCenter.GetItemKind(item.Info.Kind).Category,
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
                DataCenter.GetItemKind(item.Info.Kind).Category,
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

            var itemKind = DataCenter.GetItemKind(item.Info.Kind);
            var itemQuality = DataCenter.GetQualityData(item.Info.Quality);

            itemName.text = itemQuality.Name + item.Info.Name;
            itemName.color = PublicFunc.SetColorFromHex(itemQuality.Color);

            type.text = itemKind.Name;
            description.text = item.Info.Description;

            PublicFunc.DoActionAccordingToCategory(itemKind.Category, EquipCallBack, UseCallBack, MaterialCallBack);

            void EquipCallBack()
            {
                if (equips.Contains(item.Info.UID))
                    textUse.text = "卸下";
                else
                    textUse.text = "裝備";

                ability.text = DataCenter.GetAbilityString(item.Info);
                btnUse.gameObject.SetActive(true);
            }

            void UseCallBack()
            {
                textUse.text = "使用";
                ability.text = DataCenter.GetAbilityString(item.Info);
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
        var setItemActionRequestData = new SetItemActionRequest
        {
            BagItemData = selectedBagItem.Info
        };
        ApiBridge.Send(setItemActionRequestData, CallBack);
        PanelLoading.Create();

        void CallBack(SetItemActionResponse setItemActionResponse)
        {
            equips = setItemActionResponse.CharacterData.Equips;
            PublicFunc.DoActionAccordingToCategory(setItemActionResponse.ItemCategory, EquipCallBack, UseCallBack, null);

            if (setItemActionResponse.Enemies.Count > 0)
            {
                var getBattleStatusRequestData = new GetBattleStatusRequest();
                ApiBridge.Send(getBattleStatusRequestData, CallBack);

                void CallBack(GetBattleStatusResponse getBattleStatusResponse)
                {
                    var datas = getBattleStatusResponse.SaveData.Datas;
                    var battleResult = getBattleStatusResponse.BattleResult;

                    if (battleResult != null)
                    {
                        if (battleResult.IsAttackerDead && datas.CharacterData.Name == battleResult.Attacker ||
                            battleResult.Results.Any(x => x.IsDefenderDead && datas.CharacterData.Name == x.Defenderer))
                        {
                            // LeaveDungon(datas.PlayerData.Area, datas.CharacterData);
                        }
                        else
                        {
                            ApiBridge.Send(getBattleStatusRequestData, CallBack);
                        }
                    }

                    MainController.Instance.RefreshUI(datas.CharacterData);
                }
            }
            MainController.Instance.RefreshUI(setItemActionResponse.CharacterData, setItemActionResponse.FullAbility);
            PanelLoading.Close();

            void EquipCallBack()
            {
                if (setItemActionResponse.IsEquipped)
                {
                    selectedBagItem.IconEquip.SetActive(false);

                    textUse.text = "裝備";
                }
                else
                {
                    if (setItemActionResponse.UnEquiped.Count > 0)
                    {
                        foreach (var unEquiped in setItemActionResponse.UnEquiped)
                        {
                            var existingItem = bagItems.Find(x => x.Info.UID == unEquiped.UID);

                            if (existingItem != null)
                                existingItem.IconEquip.SetActive(false);
                        }
                    }

                    selectedBagItem.IconEquip.SetActive(true);

                    textUse.text = "卸下";
                }
            }

            void UseCallBack() => UseItem(setItemActionResponse.BagItemData);
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