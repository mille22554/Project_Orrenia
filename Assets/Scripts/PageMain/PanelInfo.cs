using UnityEngine;
using UnityEngine.UI;

public class PanelInfo : MonoBehaviour
{
    public Text playerName;
    public Text playerHp;
    public Text playerMp;
    public Text playerLv;
    public Text playerExp;

    private void OnEnable()
    {
        if (GameData.gameData != null && GameData.gameData.datas.playerData != null)
        {
            playerName.text = GameData.gameData.datas.playerData.name;
            playerHp.text = $"HP {GameData.gameData.datas.playerData.currentHp}/{GameData.gameData.datas.playerData.maxHp}";
            playerMp.text = $"MP {GameData.gameData.datas.playerData.currentMp}/{GameData.gameData.datas.playerData.maxMp}";
            playerLv.text = $"Lv {GameData.gameData.datas.playerData.level}";
            playerExp.text = $"ExP {GameData.gameData.datas.playerData.currentExp}/{GameData.gameData.datas.playerData.maxExp}";
        }
    }
}