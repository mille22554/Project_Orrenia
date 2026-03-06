using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelInfo : MonoBehaviour
{
    public static PanelInfo Instance { get; private set; }
    const string resourcePath = "Prefabs/PanelInfo";
    [SerializeField] Text playerName;
    [SerializeField] Text playerHp;
    [SerializeField] Text playerMp;
    [SerializeField] Text playerSTA;
    [SerializeField] Text playerLv;
    [SerializeField] Text playerExp;

    PlayerData PlayerData => GameData.NowPlayerData;

    public static void Create()
    {
        ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelInfo>(), MainController.Instance.InfoContent);
    }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        RefreshInfo();
    }

    public void RefreshInfo()
    {
        if (GameData.gameData != null && PlayerData != null)
        {
            playerName.text = PlayerData.PlayerName;
            playerHp.text = $"HP {PlayerData.CurrentHp}/{PlayerData.ability.HP}";
            playerMp.text = $"MP {PlayerData.CurrentMp}/{PlayerData.ability.MP}";
            playerSTA.text = $"體力 {PlayerData.CurrentSTA}/{PlayerData.ability.STA}";
            playerLv.text = $"Lv {PlayerData.Level}";
            playerExp.text = $"ExP {PlayerData.CurrentExp}/{PlayerData.MaxExp}";
        }
    }
}