using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SkillItem : MonoBehaviour
{
    [SerializeField] GameObject _iconPick;
    [SerializeField] Text _textSkillName;

    SkillData _skillData;
    Action<SkillData> _onPick;

    Toggle Toggle => GetComponent<Toggle>();

    void Awake()
    {
        Toggle.onValueChanged.AddListener(isOn =>
        {
            _iconPick.SetActive(isOn);
            _onPick?.Invoke(isOn ? _skillData : null);
        });

        _iconPick.SetActive(false);
    }

    public void SetInfo(SkillData skill, ToggleGroup toggleGroup, Action<SkillData> onPick)
    {
        _skillData = skill;
        _textSkillName.text = skill.Name;
        Toggle.group = toggleGroup;
        Toggle.isOn = false;
        _onPick = onPick;
    }
}