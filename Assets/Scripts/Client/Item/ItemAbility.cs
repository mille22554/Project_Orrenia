using System;
using UnityEngine;
using UnityEngine.UI;

public class ItemAbility : MonoBehaviour
{
    public Text Point;
    [SerializeField] Button _btnPlus;

    Action _onAbilityPlus;

    void Awake()
    {
        _btnPlus.onClick.AddListener(OnClickPlus);
    }

    public void SetInfo(int point, bool isBtnPlusInteractable, Action onAbilityPlus)
    {
        Point.text = point.ToString();
        _btnPlus.interactable = isBtnPlusInteractable;
        _onAbilityPlus = onAbilityPlus;
    }

    void OnClickPlus()
    {
        int p = int.Parse(Point.text);
        p++;
        Point.text = (int.Parse(Point.text) + 1).ToString();
        _onAbilityPlus?.Invoke();
    }
}