using System;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    public GameObject iconPick;
    public Text itemName;
    public Text count;
    public Toggle toggle;
    [NonSerialized] public ItemData info;

    private void Start()
    {
        iconPick.SetActive(false);

        toggle.onValueChanged.AddListener(OnToggleValueChange);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChange);
    }

    public void SetInfo(ItemData data)
    {
        info = data;
        itemName.text = data.name;
        count.text = data.count.ToString();
    }

    private void OnToggleValueChange(bool isOn)
    {
        iconPick.SetActive(isOn);
    }
}