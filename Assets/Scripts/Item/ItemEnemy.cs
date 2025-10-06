using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ItemEnemy : MonoBehaviour
{
    public Text enemyName;
    public Text level;
    public Text hp;
    public GameObject iconSelected;
    [NonSerialized] public Toggle toggle;
    [NonSerialized] public MobData info;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(SetToggle);
    }

    public void SetData(MobData data)
    {
        info = data;
        enemyName.text = data.name;
        level.text = "Lv " + data.level;
        hp.text = "HP " + data.currentHp;
    }

    public void SetToggle(bool isOn)
    {
        iconSelected.SetActive(isOn);
    }

    public void GetDamage(int damage)
    {
        info.currentHp -= damage;
        if (info.currentHp < 0) info.currentHp = 0;
        hp.text = "HP " + info.currentHp;
    }
}