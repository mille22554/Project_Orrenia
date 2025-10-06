using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelInfo : MonoBehaviour
{
    public Text playerName;
    public Text playerHp;
    public Text playerMp;
    public Text playerLv;
    public Text playerExp;

    private void Start()
    {
        EventMng.SetEvent(EventName.RefreshPlayerInfo, (Action)RefreshInfo);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.RefreshPlayerInfo, (Action)RefreshInfo);
    }

    private void OnEnable()
    {
        RefreshInfo();
    }

    public void RefreshInfo()
    {
        if (GameData.gameData != null && GameData.NowPlayerData != null)
        {
            playerName.text = GameData.NowPlayerData.name;
            playerHp.text = $"HP {GameData.NowPlayerData.CurrentHp}/{GameData.NowPlayerData.ability.HP}";
            playerMp.text = $"MP {GameData.NowPlayerData.CurrentMp}/{GameData.NowPlayerData.ability.MP}";
            playerLv.text = $"Lv {GameData.NowPlayerData.level}";
            playerExp.text = $"ExP {GameData.NowPlayerData.CurrentExp}/{GameData.NowPlayerData.maxExp}";
        }
    }
}