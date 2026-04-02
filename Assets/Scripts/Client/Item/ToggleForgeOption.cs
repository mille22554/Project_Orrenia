using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleForgeOption : MonoBehaviour
{
    [SerializeField] Text _optionName;
    Toggle _toggle;
    Action<bool> _onValueChange;

    void Awake()
    {
        _toggle = GetComponent<Toggle>();

        _toggle.onValueChanged.AddListener(OnValueChanged);

        void OnValueChanged(bool isOn) => _onValueChange?.Invoke(isOn);
    }

    public void SetInfo(string optionName, ToggleGroup toggleGroup, Action<bool> onValueChange)
    {
        _optionName.text = optionName;
        _toggle.group = toggleGroup;
        _onValueChange = onValueChange;

        _toggle.isOn = false;
    }
}