using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ItemEnemy : MonoBehaviour
{
    [NonSerialized] public Toggle Toggle;
    [NonSerialized] public MobData Info;

    [SerializeField] Text enemyName;
    [SerializeField] Text level;
    [SerializeField] Text hp;
    [SerializeField] GameObject iconSelected;

    void Awake()
    {
        Toggle = GetComponent<Toggle>();

        Toggle.onValueChanged.AddListener(SetToggle);
    }

    public void SetData(MobData data)
    {
        Info = data;
        enemyName.text = data.CharacterData.Name;
        level.text = "Lv " + data.CharacterData.Level;
        hp.text = "HP " + data.CharacterData.CurrentHP;
    }

    public void SetToggle(bool isOn)
    {
        iconSelected.SetActive(isOn);
    }

    public void GetDamage(decimal damage)
    {
        Info.CharacterData.CurrentHP -= damage;
        if (Info.CharacterData.CurrentHP < 0)
            Info.CharacterData.CurrentHP = 0;

        hp.text = "HP " + Info.CharacterData.CurrentHP;
    }
}