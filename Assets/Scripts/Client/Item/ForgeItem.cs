using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ForgeItem : MonoBehaviour
{
    public Text Count;
    [NonSerialized] public BagItemData Data;

    [SerializeField] Text itemName;
    private Button _btnSelf;
    private UnityAction _callback;

    private void Start()
    {
        _btnSelf = GetComponent<Button>();

        _btnSelf.onClick.AddListener(_callback);
    }

    public void SetData(BagItemData data, UnityAction callback)
    {
        Data = data;
        itemName.text = ItemDataCenter.GetItemData(data.ItemID).Name;
        Count.text = data.Count.ToString();
        _callback = callback;
    }

    public void ResetInfo() => Count.text = Data.Count.ToString();
}