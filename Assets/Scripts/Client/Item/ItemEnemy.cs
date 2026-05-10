using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ItemEnemy : MonoBehaviour
{
    [NonSerialized] public Toggle Toggle;
    [NonSerialized] public CharacterData Info;

    [SerializeField] Text enemyName;
    [SerializeField] Text level;
    [SerializeField] Text hp;
    [SerializeField] GameObject iconSelected;

    void Awake()
    {
        Toggle = GetComponent<Toggle>();

        Toggle.onValueChanged.AddListener(SetToggle);
    }

    public void SetData(CharacterData data)
    {
        Info = data;
        enemyName.text = data.Name;
        level.text = $"Lv {data.Level}";
        hp.text = $"HP {data.CurrentHP:0}";
    }

    public void SetToggle(bool isOn)
    {
        iconSelected.SetActive(isOn);
    }

    public void GetDamage(decimal damage)
    {
        CharacterData.ChangeHP(Info, damage);
        if (Info.CurrentHP < 0)
            Info.CurrentHP = 0;

        hp.text = $"HP {Info.CurrentHP:0}";
    }
}