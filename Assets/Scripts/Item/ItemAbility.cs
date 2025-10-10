using UnityEngine;
using UnityEngine.UI;

public class ItemAbility : MonoBehaviour
{
    public Text point;
    public Button btnPlus;

    private void Start()
    {
        btnPlus.onClick.AddListener(OnClickPlus);
    }

    private void OnDestroy()
    {
        btnPlus.onClick.RemoveListener(OnClickPlus);
    }

    public void SetInfo(int _point)
    {
        point.text = _point.ToString();
        btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
    }

    private void OnClickPlus()
    {
        int p = int.Parse(point.text);
        if (GameData.NowPlayerData.AbilityPoint > 0)
        {
            p++;
            GameData.NowPlayerData.AbilityPoint--;
            btnPlus.interactable = GameData.NowPlayerData.AbilityPoint > 0;
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

        // 根據 abilityType 更新對應欄位
        switch (gameObject.name)
        {
            case "STR":
                GameData.NowPlayerData.ability.STR_Point = newValue;
                break;
            case "DEX":
                GameData.NowPlayerData.ability.DEX_Point = newValue;
                break;
            case "INT":
                GameData.NowPlayerData.ability.INT_Point = newValue;
                break;
            case "VIT":
                GameData.NowPlayerData.ability.VIT_Point = newValue;
                break;
            case "AGI":
                GameData.NowPlayerData.ability.AGI_Point = newValue;
                break;
            case "LUK":
                GameData.NowPlayerData.ability.LUK_Point = newValue;
                break;
        }

        PublicFunc.SetPlayerAbility();
    }
}