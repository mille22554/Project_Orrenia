using System;
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

    private void Start()
    {
        EventMng.SetEvent(EventName.RefreshAbilityPoint, (Action)RefreshInfo);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.RefreshAbilityPoint, (Action)RefreshInfo);
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
}