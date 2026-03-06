using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ForgeItem : MonoBehaviour
{
    public Text Count;
    [NonSerialized] public ItemData Data;

    [SerializeField] Text itemName;
    private Button _btnSelf;
    private UnityAction _callback;

    private void Start()
    {
        _btnSelf = GetComponent<Button>();

        _btnSelf.onClick.AddListener(_callback);
    }

    public void SetData(ItemData data, UnityAction callback)
    {
        Data = data;
        itemName.text = ItemBaseData.Get(data.itemID).name;
        Count.text = data.count.ToString();
        _callback = callback;
    }

    public void ResetInfo() => Count.text = Data.count.ToString();
}