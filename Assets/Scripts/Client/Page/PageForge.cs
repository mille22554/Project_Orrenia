using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageForge : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageForge";
    [SerializeField] InputField inputItemName;
    [SerializeField] Transform transToggleTypes;
    [SerializeField] List<Transform> listTransToggles;
    [SerializeField] Text textMaterialNum;
    [SerializeField] ScrollRect srUsedMaterials;
    [SerializeField] ScrollRect srBagMaterials;
    [SerializeField] Button btnForge;
    [SerializeField] Button btnReset;
    [SerializeField] ForgeItem forgeItem;
    [SerializeField] Text itemUsedMaterial;

    readonly List<Toggle> toggleTypes = new();
    readonly List<List<Toggle>> listToggles = new();
    readonly Dictionary<ForgeItem, Text> usedMaterials = new();
    readonly List<ForgeItem> bagMaterials = new();
    readonly ItemData itemTemplate = new()
    {
        Description = "使用素材: ",
        Price = 500,
        ID = 1,
        Durability = 500,
    };
    int nowMaterialNum;
    int maxMaterialNum;

    public static void Create()
    {
        var panel = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageForge>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(panel);
    }

    void Awake()
    {
        for (int i = 0; i < transToggleTypes.childCount; i++)
        {
            var index = i;
            if (transToggleTypes.GetChild(index).TryGetComponent<Toggle>(out var toggleType))
            {
                var objectToggles = listTransToggles.ElementAtOrDefault(index);
                if (objectToggles != null)
                {
                    toggleType.onValueChanged.AddListener(OnToggleType);
                    toggleTypes.Add(toggleType);

                    var tempList = new List<Toggle>();
                    listToggles.Add(tempList);
                    SetToggleToList(objectToggles);



                    void OnToggleType(bool isOn)
                    {
                        objectToggles.gameObject.SetActive(isOn);

                        if (isOn)
                        {
                            switch (index)
                            {
                                case 0:
                                    maxMaterialNum = 6;
                                    break;
                                case 1:
                                    maxMaterialNum = 10;
                                    break;
                                case 2:
                                    maxMaterialNum = 8;
                                    break;
                            }

                            OnReset();

                            var tempToggles = listToggles.ElementAtOrDefault(index);
                            if (tempToggles != null)
                            {
                                var tempToggle = tempToggles.ElementAtOrDefault(0);
                                if (tempToggle != null)
                                    tempToggle.isOn = true;
                            }
                        }
                    }

                    void SetToggleToList(Transform transObject)
                    {
                        for (int j = 0; j < transObject.childCount; j++)
                        {
                            var transToggle = transObject.GetChild(j);
                            if (transToggle.TryGetComponent<Toggle>(out var toggle))
                            {
                                var index2 = j;
                                toggle.onValueChanged.AddListener(isOn =>
                                {
                                    if (isOn)
                                    {
                                        OnReset();
                                        CheckItemType(index, index2);
                                    }
                                });
                                tempList.Add(toggle);
                            }
                            else if (transToggle.childCount > 0)
                            {
                                SetToggleToList(transToggle);
                            }
                        }
                    }
                }
            }
        }
    }

    void OnEnable()
    {
        inputItemName.text = "";

        // foreach (var material in GameData_Server.NowBagData.Items.Where(x => ItemTypeCheck.IsMaterialType(ItemDataCenter.GetItemData(x.ItemID).Kind)))
        // {
        //     var tempItem = Instantiate(forgeItem, srBagMaterials.content);
        //     tempItem.SetData(material, OnBtnBagMaterial);
        //     bagMaterials.Add(tempItem);



        //     void OnBtnBagMaterial()
        //     {
        //         if (int.TryParse(tempItem.Count.text, out var count))
        //         {
        //             if (count == 0 || nowMaterialNum == maxMaterialNum)
        //             {
        //                 return;
        //             }
        //             else
        //             {
        //                 count--;
        //                 tempItem.Count.text = count.ToString();
        //             }
        //         }

        //         if (usedMaterials.TryGetValue(tempItem, out var textNum))
        //         {
        //             textNum.text = $"{ItemDataCenter.GetItemData(tempItem.Data.ItemID).Name} x{tempItem.Data.Count - count}";
        //         }
        //         else
        //         {
        //             var tempItem2 = Instantiate(itemUsedMaterial, srUsedMaterials.content);
        //             tempItem2.text = $"{ItemDataCenter.GetItemData(tempItem.Data.ItemID).Name} x1";
        //             usedMaterials.Add(tempItem, tempItem2);
        //         }

        //         nowMaterialNum++;
        //         textMaterialNum.text = $"已填入素材數:{nowMaterialNum}/{maxMaterialNum}";
        //     }
        // }

        var tempType = toggleTypes.ElementAtOrDefault(0);
        if (tempType != null)
            tempType.isOn = true;
    }

    void OnDisable()
    {
        foreach (var bagMaterial in bagMaterials)
            Destroy(bagMaterial.gameObject);
        bagMaterials.Clear();

        foreach (var usedMaterial in usedMaterials.Values)
            Destroy(usedMaterial.gameObject);
        usedMaterials.Clear();
    }

    void Start()
    {
        btnForge.onClick.AddListener(OnForge);
        btnReset.onClick.AddListener(OnReset);
    }

    void OnForge()
    {
        // if (nowMaterialNum < maxMaterialNum || string.IsNullOrWhiteSpace(inputItemName.text))
        //     return;

        // var newItem = PublicFunc.GetItem(itemTemplate);
        // ItemBaseData.Get(newItem.itemID).name = inputItemName.text;

        // foreach (var tempItem in usedMaterials.Keys)
        // {
        //     if (int.TryParse(tempItem.Count.text, out var count))
        //     {
        //         for (int i = 0; i < count; i++)
        //             SaveDataCenter.SetEquipAbility(ItemBaseData.Get(tempItem.Data.itemID).ability, ItemBaseData.Get(newItem.itemID).ability);

        //         tempItem.Data.count = count;
        //         if (count == 0)
        //         {
        //             GameData.NowBagData.items.Remove(tempItem.Data);
        //             bagMaterials.Remove(tempItem);
        //             Destroy(tempItem.gameObject);
        //         }
        //     }
        // }
    }

    void OnReset()
    {
        foreach (var usedMaterial in usedMaterials.Values)
            Destroy(usedMaterial.gameObject);
        usedMaterials.Clear();
        nowMaterialNum = 0;

        foreach (var bagMaterial in bagMaterials)
            bagMaterial.ResetInfo();

        textMaterialNum.text = $"已填入素材數:{nowMaterialNum}/{maxMaterialNum}";
    }

    void CheckItemType(int index, int index2)
    {
        // switch (index)
        // {
        //     case 0:
        //         switch (index2)
        //         {
        //             case 0:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Sword;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 5 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     STR = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 1:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Hammer;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 3 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     VIT = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 2:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Spear;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     SPD = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEX = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 3:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Staff;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     MATK = 5 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     INT = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 4:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Rapier;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 3 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     CRIT = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     AGI = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 5:
        //                 itemTemplate.Kind = EquipType.One_Hand_Weapon.Dagger;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     LUK = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //         }
        //         break;
        //     case 1:
        //         switch (index2)
        //         {
        //             case 0:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Axe;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 8 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     STR = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 1:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Aegis;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     VIT = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 2:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Bow;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 6 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     SPD = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     EVA = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEX = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 3:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Book;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     MATK = 8 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     INT = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 4:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Katana;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 6 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     CRIT = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     AGI = 8 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 5:
        //                 itemTemplate.Kind = EquipType.Two_Hand_Weapon.Tarot;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     LUK = 12 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //         }
        //         break;
        //     case 2:
        //         switch (index2)
        //         {
        //             case 0:
        //                 itemTemplate.Kind = EquipType.Shield;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ATK = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 1:
        //                 itemTemplate.Kind = EquipType.Helmet;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     ACC = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 2:
        //                 itemTemplate.Kind = EquipType.Armor;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     DEF = 5 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 5 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 3:
        //                 itemTemplate.Kind = EquipType.Greaves;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     STA = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 4:
        //                 itemTemplate.Kind = EquipType.Shoes;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     SPD = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 5:
        //                 itemTemplate.Kind = EquipType.Gloves;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     CRIT = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 6:
        //                 itemTemplate.Kind = EquipType.Cape;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     EVA = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 4 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 4 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 7:
        //                 itemTemplate.Kind = EquipType.Ring;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     STR = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     VIT = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEX = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     INT = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     AGI = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     LUK = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 2 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //             case 8:
        //                 itemTemplate.Kind = EquipType.Pendant;
        //                 itemTemplate.Ability = new()
        //                 {
        //                     HP = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MP = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     STA = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     ACC = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     EVA = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     CRIT = 1 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     DEF = 2 * GameData_Server.NowPlayerData.ForgeLevel,
        //                     MDEF = 2 * GameData_Server.NowPlayerData.ForgeLevel
        //                 };
        //                 break;
        //         }
        //         break;
        // }

    }
}