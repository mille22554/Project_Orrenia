using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PanelSkill : MonoBehaviour
{
    [SerializeField] Button _btnUse;
    [SerializeField] Text _textSkillName;
    [SerializeField] Text _textWeaponType;
    [SerializeField] Text _textSkillType;
    [SerializeField] Text _textDescript;
    [SerializeField] Text _textCost;
    [SerializeField] Text _textCD;
    [SerializeField] ScrollRect _srSkillList;
    [SerializeField] SkillItem _skillItem;

    ESkillID _nowSelectSkillID;

    Button BtnBG => GetComponent<Button>();
    ToggleGroup ToggleGroup => _srSkillList.content.GetComponent<ToggleGroup>();
    Action<ESkillID> _onSkillSelect;
    readonly List<SkillItem> _skillItems = new();

    void Awake()
    {
        BtnBG.onClick.AddListener(OnSwitch);
        _btnUse.onClick.AddListener(() =>
        {
            if (_nowSelectSkillID != ESkillID.None)
                _onSkillSelect?.Invoke(_nowSelectSkillID);
        });
    }

    public void OnSwitch() => gameObject.SetActive(!gameObject.activeSelf);

    public void SetInfo(Action<ESkillID> onSkillSelect)
    {
        _onSkillSelect = onSkillSelect;

        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        RefreshUI(null);

        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            var skills = response.SaveData.Datas.CharacterData.Skills;
            foreach (var skill in skills.Values)
            {
                var item = ObjectPool.Get(_skillItem, ToggleGroup.transform);
                item.SetInfo(skill, ToggleGroup, RefreshUI);
                _skillItems.Add(item);
            }
        }
    }

    void RefreshUI(SkillData skillData)
    {
        if (skillData == null)
        {
            _textSkillName.text = "";
            _textWeaponType.text = "";
            _textSkillType.text = "";
            _textDescript.text = "";
            _textCost.text = "";
            _textCD.text = "";

            _btnUse.gameObject.SetActive(false);

            _nowSelectSkillID = ESkillID.None;
        }
        else
        {
            _textSkillName.text = skillData.Name;

            var weaponTypeString = new List<string>();
            foreach (var type in skillData.WeaponType)
            {
                var kind = DataCenter.GetItemKind(type);

                weaponTypeString.Add(kind.Name);
            }
            _textWeaponType.text = string.Join(", ", weaponTypeString);
            _textSkillType.text = DataCenter.GetDamageType(skillData.SkillType);
            _textDescript.text = skillData.Description;

            if (skillData.CoolDown == 0)
                _textCD.text = "";
            else
                _textCD.text = $"冷卻回合: {skillData.CoolDown}";

            if (skillData.SkillType == ESkillType.Passive)
            {
                _textCost.text = "";
                _btnUse.gameObject.SetActive(false);
            }
            else
            {
                _textCost.text = $"MP消耗: {skillData.Cost}";
                _btnUse.gameObject.SetActive(true);
            }

            _nowSelectSkillID = skillData.ID;
        }
    }

    void OnDisable()
    {
        foreach (var item in _skillItems)
            ObjectPool.Put(item);

        _skillItems.Clear();
    }
}