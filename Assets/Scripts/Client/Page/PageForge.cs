using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageForge : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageForge";

    [SerializeField] InputField _inputItemName;
    [SerializeField] ToggleGroup _toggleTypesGroup;
    [SerializeField] ToggleGroup _toggleKindsGroup;
    [SerializeField] Text _textMaterialNum;
    [SerializeField] ScrollRect _srSelectedMaterials;
    [SerializeField] ScrollRect _srBagMaterials;
    [SerializeField] Button _btnForge;
    [SerializeField] Button _btnReset;
    [SerializeField] ForgeBagItem _forgeBagItem;
    [SerializeField] ForgeSelectedItem _forgeSelectedItem;
    [SerializeField] ToggleForgeOption _toggleForgeOptionType;
    [SerializeField] ToggleForgeOption _toggleForgeOptionKind;

    readonly List<ToggleForgeOption> _toggleOptionKinds = new();
    readonly List<Toggle> _toggleTypes = new();
    readonly Dictionary<ForgeBagItem, ForgeSelectedItem> _selectedMaterials = new();
    readonly List<ForgeBagItem> _bagMaterials = new();
    int _maxMaterialNum;
    EItemKind _nowSelectedKind = EItemKind.None;

    enum EToggleType
    {
        單手武器,
        雙手武器,
        防具
    }

    public static void Create()
    {
        var panel = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageForge>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(panel);
    }

    void Awake()
    {
        foreach (var type in (EToggleType[])Enum.GetValues(typeof(EToggleType)))
        {
            var optionType = ObjectPool.Get(_toggleForgeOptionType, _toggleTypesGroup.transform);
            optionType.SetInfo(type.ToString(), _toggleTypesGroup, OnOptionType);

            void OnOptionType(bool isOn)
            {
                if (isOn)
                {
                    foreach (var optionKind in _toggleOptionKinds)
                        ObjectPool.Put(optionKind);

                    _toggleOptionKinds.Clear();

                    switch (type)
                    {
                        case EToggleType.單手武器:
                            var categories = new List<EItemCategory> { EItemCategory.One_Hand };
                            SetBySwitch(6, categories);
                            break;
                        case EToggleType.雙手武器:
                            categories = new List<EItemCategory> { EItemCategory.Two_Hand };
                            SetBySwitch(10, categories);
                            break;
                        case EToggleType.防具:
                            categories = new List<EItemCategory>
                            {
                                EItemCategory.Shield,
                                EItemCategory.Helmet,
                                EItemCategory.Armor,
                                EItemCategory.Greaves,
                                EItemCategory.Shoes,
                                EItemCategory.Gloves,
                                EItemCategory.Cape,
                                EItemCategory.Ring,
                                EItemCategory.Pendant
                            };
                            SetBySwitch(8, categories);
                            break;
                    }

                    OnReset();

                    void SetBySwitch(int maxMaterialNum, List<EItemCategory> categories)
                    {
                        _maxMaterialNum = maxMaterialNum;

                        foreach (var kindData in DataCenter.ItemKind)
                        {
                            foreach (var category in categories)
                            {
                                if (kindData.Value.Category == category)
                                {
                                    var optionKind = ObjectPool.Get(_toggleForgeOptionKind, _toggleKindsGroup.transform);
                                    _toggleOptionKinds.Add(optionKind);
                                    optionKind.SetInfo(kindData.Value.Name, _toggleKindsGroup, OnOptionKind);
                                }
                            }

                            void OnOptionKind(bool isOn)
                            {
                                if (isOn)
                                {
                                    _nowSelectedKind = kindData.Key;
                                }
                                else
                                {
                                    _nowSelectedKind = EItemKind.None;
                                }
                            }
                        }
                    }
                }
            }
        }

        _btnForge.onClick.AddListener(OnForge);
        _btnReset.onClick.AddListener(OnReset);
    }

    void OnEnable()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            InitPage(response.SaveData.Datas.CharacterData.BagItems);
        }
    }

    void InitPage(List<BagItemData> bagItemDatas)
    {
        _inputItemName.text = "";
        OnReset();

        foreach (var bagItemData in bagItemDatas)
        {
            var kind = bagItemData.Kind;
            DataCenter.DoActionAccordingToCategory(kind, null, null, MaterialCallBack);

            void MaterialCallBack()
            {
                var bagItem = ObjectPool.Get(_forgeBagItem, _srBagMaterials.content);
                bagItem.SetData(bagItemData, OnBtnBagMaterial);
                _bagMaterials.Add(bagItem);

                void OnBtnBagMaterial()
                {
                    if (bagItem.Count == 0 || NowMaterialNum() == _maxMaterialNum)
                    {
                        return;
                    }
                    else
                    {
                        bagItem.UpdateItemCount(bagItem.Count - 1);
                    }

                    var bagItemName = bagItem.Data.Name;
                    var bagItemUID = bagItem.Data.UID;
                    if (_selectedMaterials.TryGetValue(bagItem, out var selectedItem))
                    {
                        selectedItem.SetData(bagItemName, bagItemUID, bagItemData.Count - bagItem.Count);
                    }
                    else
                    {
                        selectedItem = ObjectPool.Get(_forgeSelectedItem, _srSelectedMaterials.content);
                        selectedItem.SetData(bagItemName, bagItemUID, 1);
                        _selectedMaterials.Add(bagItem, selectedItem);
                    }

                    _textMaterialNum.text = $"已填入素材數:{NowMaterialNum()}/{_maxMaterialNum}";
                }

                var tempType = _toggleTypes.ElementAtOrDefault(0);
                if (tempType != null)
                    tempType.isOn = true;
            }
        }
    }

    void OnDisable() => Clear();

    void Clear()
    {
        foreach (var bagMaterial in _bagMaterials)
            ObjectPool.Put(bagMaterial);
        _bagMaterials.Clear();

        foreach (var usedMaterial in _selectedMaterials.Values)
            ObjectPool.Put(usedMaterial);
        _selectedMaterials.Clear();
    }

    void OnForge()
    {
        if (NowMaterialNum() < _maxMaterialNum || string.IsNullOrWhiteSpace(_inputItemName.text) || _nowSelectedKind == EItemKind.None)
            return;

        var materials = new List<long>();
        foreach (var selectedItem in _selectedMaterials.Values)
        {
            for (int i = 0; i < selectedItem.Count; i++)
                materials.Add(selectedItem.UID);
        }

        var requestData = new SetForgeActionRequest
        {
            ItemName = _inputItemName.text,
            ItemKind = _nowSelectedKind,
            Materials = materials
        };
        ApiBridge.Send(requestData, CallBack);

        void CallBack(SetForgeActionResponse response)
        {
            Clear();

            InitPage(response.BagItemDatas);
        }
    }

    void OnReset()
    {
        foreach (var selectedMaterial in _selectedMaterials.Values)
            ObjectPool.Put(selectedMaterial);

        _selectedMaterials.Clear();

        foreach (var bagMaterial in _bagMaterials)
            bagMaterial.UpdateItemCount();

        _textMaterialNum.text = $"已填入素材數:{NowMaterialNum()}/{_maxMaterialNum}";
    }

    int NowMaterialNum()
    {
        var num = 0;
        foreach (var item in _selectedMaterials.Values)
            num += item.Count;

        return num;
    }
}