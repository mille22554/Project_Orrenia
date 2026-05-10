using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageBag : MonoBehaviour
{
    static PageBag _ins;
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
    List<BagItemData> equips;

    public static void Create()
    {
        if (_ins == null || !_ins.gameObject.activeSelf)
        {
            _ins = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageBag>(), MainController.Instance.PageContent);
            MainController.Instance.SwitchPage(_ins);
        }
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
        PanelLoading.Create(PanelLoading.BGType.Full);
        var requestData = new GetBagInfoRequest
        {
            UID = DataCenter.UID,
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(GetBagInfoResponse response)
        {
            if (response.Code == 0)
            {
                ResetBagInfo();
                btnUse.gameObject.SetActive(false);
                gold.text = response.PlayerData.Gold.ToString();
                equips = response.Equips;

                foreach (var itemInfo in response.BagItems)
                {
                    var item = ObjectPool.Get(bagItem, itemList.content);
                    item.SetInfo(itemInfo, toggleItems, RefreshBagInfo, equips.Any(equip => equip.UID == itemInfo.UID));
                    bagItems.Add(item);

                    var itemKind = DataCenter.GetItemKind(item.Info.Kind);

                    if (toggleEquip.isOn)
                        item.gameObject.SetActive(PublicFunc.IsEquipCategory(itemKind.Category));
                    else if (toggleUse.isOn)
                        item.gameObject.SetActive(PublicFunc.IsUseCategory(itemKind.Category));
                    else if (toggleMaterial.isOn)
                        item.gameObject.SetActive(PublicFunc.IsMaterialCategory(itemKind.Category));
                }
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
            DataCenter.DoActionAccordingToCategory
            (
                item.Info.Kind,
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
            DataCenter.DoActionAccordingToCategory
            (
                item.Info.Kind,
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
            DataCenter.DoActionAccordingToCategory
            (
                item.Info.Kind,
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

            var itemData = DataCenter.GetItemData(item.Info.ID);
            var itemKind = DataCenter.GetItemKind(itemData.Kind);
            var itemQuality = DataCenter.GetQualityData(item.Info.Quality);

            itemName.text = itemQuality.Name + itemData.Name;
            itemName.color = PublicFunc.SetColorFromHex(itemQuality.Color);

            type.text = itemKind.Name;
            description.text = itemData.Description;

            DataCenter.DoActionAccordingToCategory(itemData.Kind, EquipCallBack, UseCallBack, MaterialCallBack);

            void EquipCallBack()
            {
                if (equips.Any(equip => equip.UID == item.Info.UID))
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
        SetItemAction();

        void SetItemAction()
        {
            PanelLoading.Create(PanelLoading.BGType.None);
            var requestData = new SetItemActionRequest
            {
                UID = DataCenter.UID,
                ItemUID = selectedBagItem.Info.UID
                // BagItemData = selectedBagItem.Info
            };
            APIController.Ins.Send(requestData, CallBack);

            void CallBack(SetItemActionResponse response)
            {
                if (response.Code == 0)
                {
                    equips = response.Equips;
                    PublicFunc.DoActionAccordingToCategory(response.ItemCategory, EquipCallBack, UseCallBack, null);

                    if (response.Enemies.Count > 0)
                    {
                        GetBattleStatus();
                    }
                    MainController.Instance.RefreshUI(response.CharacterData, response.FullAbility);

                    void EquipCallBack()
                    {
                        if (response.IsEquipped)
                        {
                            selectedBagItem.IconEquip.SetActive(false);

                            textUse.text = "裝備";
                        }
                        else
                        {
                            if (response.UnEquipped.Count > 0)
                            {
                                foreach (var unEquipped in response.UnEquipped)
                                {
                                    var existingItem = bagItems.Find(x => x.Info.UID == unEquipped.UID);

                                    if (existingItem != null)
                                        existingItem.IconEquip.SetActive(false);
                                }
                            }

                            selectedBagItem.IconEquip.SetActive(true);

                            textUse.text = "卸下";
                        }
                    }

                    void UseCallBack() => UseItem(response.BagItemData);
                }

                PanelLoading.Close();
            }
        }

        void GetBattleStatus()
        {
            PanelLoading.Create(PanelLoading.BGType.None);
            var requestData = new GetBattleStatusRequest
            {
                UID = DataCenter.UID,
            };
            APIController.Ins.Send(requestData, CallBack);

            void CallBack(GetBattleStatusResponse response)
            {
                if (response.Code == 0)
                {
                    var battleResult = response.ActionResult.BattleResult;

                    if (battleResult != null)
                    {
                        if (battleResult.IsAttackerDead && response.CharacterData.Name == battleResult.Attacker ||
                            battleResult.Results.Any(x => x.IsDefenderDead && response.CharacterData.Name == x.Defenderer))
                        {
                            // LeaveDungeon(response.PlayerData.Area, response.CharacterData);
                        }
                        else
                        {
                            GetBattleStatus();
                        }
                    }

                    MainController.Instance.RefreshUI(response.CharacterData);
                }

                PanelLoading.Close();
            }
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