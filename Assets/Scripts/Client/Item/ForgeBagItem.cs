using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ForgeBagItem : MonoBehaviour
{
    public BagItemData Data { get; private set; }
    public int Count { get; private set; }

    [SerializeField] Text _itemName;
    [SerializeField] Text _textCount;

    Button _btnSelf;
    UnityAction _callback;

    void Awake()
    {
        _btnSelf = GetComponent<Button>();

        _btnSelf.onClick.AddListener(OnClickSelf);

        void OnClickSelf() => _callback?.Invoke();
    }

    public void SetData(BagItemData data, UnityAction callback)
    {
        Data = data;
        Count = data.Count;

        _itemName.text = data.Name;
        _textCount.text = data.Count.ToString();
        _callback = callback;
    }

    public void UpdateItemCount() => UpdateItemCount(Data.Count);
    public void UpdateItemCount(int count)
    {
        Count = count;
        _textCount.text = count.ToString();
    }
}