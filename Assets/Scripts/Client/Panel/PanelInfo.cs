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
    FullAbilityBase _fullAbility;
    int _exp;

    public static PanelInfo Create()
    {
        return ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PanelInfo>(), MainController.Instance.InfoContent);
    }

    void OnEnable()
    {
        RefreshInfo();
    }

    void RefreshInfo()
    {
        PanelLoading.Create(PanelLoading.BGType.None);
        var requestData = new GetSaveDataRequest
        {
            Account = DataCenter.Account,
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response)
        {
            if (response.Code == 0)
            {
                _fullAbility = response.FullAbility;
                _exp = response.Exp;

                var data = RefreshInfoData.Create(response);
                RefreshInfo(data);
            }

            PanelLoading.Close();
        }
    }

    public void RefreshInfo(RefreshInfoData data)
    {
        Debug.Log("UI刷新");
        if (data.PlayerHP != -1)
            _fullAbility.HP = data.PlayerHP;

        if (data.PlayerMP != -1)
            _fullAbility.MP = data.PlayerMP;

        if (data.PlayerSTA != -1)
            _fullAbility.STA = data.PlayerSTA;

        if (data.PlayerExp != -1)
            _exp = data.PlayerExp;

        playerName.text = data.PlayerName;
        playerHp.text = $"HP {data.PlayerCurrentHP:0}/{_fullAbility.HP:0}";
        playerMp.text = $"MP {data.PlayerCurrentMP:0}/{_fullAbility.MP:0}";
        playerSTA.text = $"體力 {data.PlayerCurrentSTA:0}/{_fullAbility.STA:0}";
        playerLv.text = $"Lv {data.PlayerLv:0}";
        playerExp.text = $"ExP {data.PlayerCurrentExp:0}/{_exp:0}";
    }
}

public class RefreshInfoData
{
    public string PlayerName;
    public int PlayerCurrentHP;
    public int PlayerHP;
    public int PlayerCurrentMP;
    public int PlayerMP;
    public int PlayerCurrentSTA;
    public int PlayerSTA;
    public int PlayerLv;
    public int PlayerCurrentExp;
    public int PlayerExp;

    public static RefreshInfoData Create(GetSaveDataResponse response)
    {
        var characterData = response.CharacterData;
        var fullAbility = response.FullAbility;

        return new RefreshInfoData
        {
            PlayerName = characterData.Name,
            PlayerCurrentHP = characterData.CurrentHP,
            PlayerCurrentMP = characterData.CurrentMP,
            PlayerCurrentSTA = characterData.CurrentSTA,
            PlayerCurrentExp = characterData.CurrentExp,
            PlayerLv = characterData.Level,
            PlayerHP = (int)fullAbility.HP,
            PlayerMP = fullAbility.MP,
            PlayerSTA = fullAbility.STA,
            PlayerExp = response.Exp
        };
    }

    public static RefreshInfoData Create(CharacterData characterData) => Create(characterData, null, -1);
    public static RefreshInfoData Create(CharacterData characterData, FullAbilityBase fullAbility) => Create(characterData, fullAbility, -1);
    public static RefreshInfoData Create(CharacterData characterData, FullAbilityBase fullAbility, int exp)
    {
        var data = new RefreshInfoData
        {
            PlayerName = characterData.Name,
            PlayerCurrentHP = characterData.CurrentHP,
            PlayerCurrentMP = characterData.CurrentMP,
            PlayerCurrentSTA = characterData.CurrentSTA,
            PlayerCurrentExp = characterData.CurrentExp,
            PlayerLv = characterData.Level,
        };

        if (fullAbility != null)
        {
            data.PlayerHP = (int)fullAbility.HP;
            data.PlayerMP = fullAbility.MP;
            data.PlayerSTA = fullAbility.STA;
        }
        else
        {
            data.PlayerHP = -1;
            data.PlayerMP = -1;
            data.PlayerSTA = -1;
        }

        data.PlayerExp = exp;

        return data;
    }
}