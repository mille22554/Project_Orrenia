using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class PageCharacter : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageCharacter";
    [SerializeField] ItemAbility STR;
    [SerializeField] ItemAbility VIT;
    [SerializeField] ItemAbility DEX;
    [SerializeField] ItemAbility INT;
    [SerializeField] ItemAbility AGI;
    [SerializeField] ItemAbility LUK;
    [SerializeField] Text abilityPoint;
    [SerializeField] Button btnReset;

    PlayerData _playerData => GameData.NowPlayerData;

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageCharacter>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(page);
    }

    void Awake()
    {
        btnReset.onClick.AddListener(OnReset);
    }

    void OnEnable()
    {
        if (GameData.gameData != null && _playerData != null)
            RefreshInfo();
    }

    public void RefreshInfo()
    {
        STR.SetInfo(_playerData.ability.STR_Point);
        VIT.SetInfo(_playerData.ability.VIT_Point);
        DEX.SetInfo(_playerData.ability.DEX_Point);
        INT.SetInfo(_playerData.ability.INT_Point);
        AGI.SetInfo(_playerData.ability.AGI_Point);
        LUK.SetInfo(_playerData.ability.LUK_Point);

        abilityPoint.text = _playerData.AbilityPoint.ToString();
    }

    void OnReset()
    {
        PublicFunc.SetAbilityPoint((_playerData.Level - 1) * 6);
        _playerData.ability = new()
        {
            STR_Point = 1,
            DEX_Point = 1,
            INT_Point = 1,
            VIT_Point = 1,
            AGI_Point = 1,
            LUK_Point = 1
        };
        PublicFunc.SetPlayerAbility();
        RefreshInfo();
        PublicFunc.SaveData();
    }
}