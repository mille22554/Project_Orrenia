using System;
using UnityEngine;
using UnityEngine.UI;

public class PanelInfo : MonoBehaviour
{
    const string resourcePath = "Prefabs/PanelInfo";
    [SerializeField] Text playerName;
    [SerializeField] Text playerHp;
    [SerializeField] Text playerMp;
    [SerializeField] Text playerSTA;
    [SerializeField] Text playerLv;
    [SerializeField] Text playerExp;

    public static PanelInfo Create()
    {
        return ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelInfo>(), MainController.Instance.InfoContent);
    }

    void OnEnable()
    {
        RefreshInfo();
    }

    public void RefreshInfo()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response) => RefreshInfo(response);
    }

    public void RefreshInfo(GetSaveDataResponse response)
    {
        var characterData = response.SaveData.Datas.CharacterData;
        playerName.text = characterData.Name;
        playerHp.text = $"HP {characterData.CurrentHP}/{response.FullAbility.HP}";
        playerMp.text = $"MP {characterData.CurrentMP}/{response.FullAbility.MP}";
        playerSTA.text = $"體力 {characterData.CurrentSTA}/{response.FullAbility.STA}";
        playerLv.text = $"Lv {characterData.Level}";
        playerExp.text = $"ExP {characterData.CurrentExp}/{response.Exp}";
    }
}