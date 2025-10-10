using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class PanelCharacter : MonoBehaviour
{
    public ItemAbility STR;
    public ItemAbility VIT;
    public ItemAbility DEX;
    public ItemAbility INT;
    public ItemAbility AGI;
    public ItemAbility LUK;
    public Text abilityPoint;
    public Button btnReset;

    private void Start()
    {
        EventMng.SetEvent(EventName.RefreshAbilityPoint, (Action)RefreshInfo);
        btnReset.onClick.AddListener(OnReset);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.RefreshAbilityPoint, (Action)RefreshInfo);
        btnReset.onClick.RemoveListener(OnReset);
    }

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
            RefreshInfo();
    }

    public void RefreshInfo()
    {
        STR.SetInfo(GameData.NowPlayerData.ability.STR);
        VIT.SetInfo(GameData.NowPlayerData.ability.VIT);
        DEX.SetInfo(GameData.NowPlayerData.ability.DEX);
        INT.SetInfo(GameData.NowPlayerData.ability.INT);
        AGI.SetInfo(GameData.NowPlayerData.ability.AGI);
        LUK.SetInfo(GameData.NowPlayerData.ability.LUK);

        abilityPoint.text = GameData.NowPlayerData.AbilityPoint.ToString();
    }

    private void OnReset()
    {
        GameData.NowPlayerData.AbilityPoint = (GameData.NowPlayerData.level - 1) * 6;
        GameData.NowPlayerData.ability = new()
        {
            STR_Point = 1,
            DEX_Point = 1,
            INT_Point = 1,
            VIT_Point = 1,
            AGI_Point = 1,
            LUK_Point = 1
        };
        PublicFunc.SetPlayerAbility();
        if (GameData.NowPlayerData.CurrentHp > GameData.NowPlayerData.ability?.HP)
            GameData.NowPlayerData.CurrentHp = GameData.NowPlayerData.ability.HP;
        if (GameData.NowPlayerData.CurrentMp > GameData.NowPlayerData.ability?.MP)
            GameData.NowPlayerData.CurrentMp = GameData.NowPlayerData.ability.MP;

        RefreshInfo();
        PublicFunc.SaveData();
    }
}