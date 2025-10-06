using UnityEngine;
using UnityEngine.UI;

public class ItemAbility : MonoBehaviour
{
    public Text point;
    public Button btnPlus;
    public Button btnMinus;

    private void Start()
    {
        btnPlus.onClick.AddListener(OnClickPlus);
        btnMinus.onClick.AddListener(OnClickMinus);
    }

    private void OnDestroy()
    {
        btnPlus.onClick.RemoveListener(OnClickPlus);
        btnMinus.onClick.RemoveListener(OnClickMinus);
    }

    public void SetInfo(int _point)
    {
        point.text = _point.ToString();
        btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
        btnMinus.interactable = int.Parse(point.text) > 1;
    }

    private void OnClickPlus()
    {
        int p = int.Parse(point.text);
        if (GameData.NowPlayerData.AbilityPoint > 0)
        {
            p++;
            GameData.NowPlayerData.AbilityPoint--;
            btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
            btnMinus.interactable = p > 1;
            point.text = p.ToString();

            UpdateAbility(p);
            PublicFunc.SaveData();
        }
    }

    private void OnClickMinus()
    {
        int p = int.Parse(point.text);
        if (p > 1)
        {
            p--;
            GameData.NowPlayerData.AbilityPoint++;
            btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
            btnMinus.interactable = p > 1;
            point.text = p.ToString();

            UpdateAbility(p);
            PublicFunc.SaveData();
        }
    }
    

    private void UpdateAbility(int newValue)
    {
        // 更新 UI
        point.text = newValue.ToString();
        btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
        btnMinus.interactable = newValue > 1;

        // 根據 abilityType 更新對應欄位
        switch (gameObject.name)
        {
            case "STR":
                GameData.NowPlayerData.ability.STR = newValue;
                break;
            case "DEX":
                GameData.NowPlayerData.ability.DEX = newValue;
                break;
            case "INT":
                GameData.NowPlayerData.ability.INT = newValue;
                break;
            case "VIT":
                GameData.NowPlayerData.ability.VIT = newValue;
                break;
            case "AGI":
                GameData.NowPlayerData.ability.AGI = newValue;
                break;
            case "LUK":
                GameData.NowPlayerData.ability.LUK = newValue;
                break;
        }

        PublicFunc.SetPlayerAbility(GameData.NowPlayerData.ability);
        EventMng.EmitEvent(EventName.RefreshPlayerInfo);
        PublicFunc.SaveData();
    }
}