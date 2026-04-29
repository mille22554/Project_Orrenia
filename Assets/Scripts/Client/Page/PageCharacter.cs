using UnityEngine;
using UnityEngine.UI;

public class PageCharacter : MonoBehaviour
{
    static PageCharacter _ins;
    const string resourcePath = "Prefabs/PageCharacter";

    [SerializeField] ItemAbility STR;
    [SerializeField] ItemAbility VIT;
    [SerializeField] ItemAbility DEX;
    [SerializeField] ItemAbility INT;
    [SerializeField] ItemAbility AGI;
    [SerializeField] ItemAbility LUK;
    [SerializeField] Text _abilityPoint;
    [SerializeField] Button btnReset;

    public static void Create()
    {
        if (_ins == null || !_ins.gameObject.activeSelf)
        {
            _ins = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageCharacter>(), MainController.Instance.PageContent);
            MainController.Instance.SwitchPage(_ins);
        }
    }

    void Awake()
    {
        btnReset.onClick.AddListener(OnReset);
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
                RefreshInfo(response.CharacterData, response.AbilityPoint);
            }

            PanelLoading.Close();
        }
    }

    void RefreshInfo(CharacterData characterData, int abilityPoint)
    {
        var isHasAbilityPoint = abilityPoint > 0;

        STR.SetInfo(characterData.Ability.STR_Point, isHasAbilityPoint, OnAbilityPlus);
        VIT.SetInfo(characterData.Ability.VIT_Point, isHasAbilityPoint, OnAbilityPlus);
        DEX.SetInfo(characterData.Ability.DEX_Point, isHasAbilityPoint, OnAbilityPlus);
        INT.SetInfo(characterData.Ability.INT_Point, isHasAbilityPoint, OnAbilityPlus);
        AGI.SetInfo(characterData.Ability.AGI_Point, isHasAbilityPoint, OnAbilityPlus);
        LUK.SetInfo(characterData.Ability.LUK_Point, isHasAbilityPoint, OnAbilityPlus);

        _abilityPoint.text = abilityPoint.ToString();

        MainController.Instance.RefreshUI(characterData);
    }


    void OnAbilityPlus()
    {
        var ability = new AbilityBase
        {
            STR_Point = int.Parse(STR.Point.text),
            VIT_Point = int.Parse(VIT.Point.text),
            DEX_Point = int.Parse(DEX.Point.text),
            INT_Point = int.Parse(INT.Point.text),
            AGI_Point = int.Parse(AGI.Point.text),
            LUK_Point = int.Parse(LUK.Point.text),
        };

        PanelLoading.Create(PanelLoading.BGType.None);
        var requestData = new SetPlayerAbilityRequest
        {
            Account = DataCenter.Account,
            Ability = ability
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(SetPlayerAbilityResponse response)
        {
            if (response.Code == 0)
            {
                RefreshInfo(response.CharacterData, response.AbilityPoint);
            }

            PanelLoading.Close();
        }
    }

    void OnReset()
    {
        var ability = new AbilityBase
        {
            STR_Point = 1,
            DEX_Point = 1,
            INT_Point = 1,
            VIT_Point = 1,
            AGI_Point = 1,
            LUK_Point = 1
        };

        PanelLoading.Create(PanelLoading.BGType.None);
        var requestData = new SetPlayerAbilityRequest
        {
            Account = DataCenter.Account,
            Ability = ability
        };
        APIController.Ins.Send(requestData, CallBack);

        void CallBack(SetPlayerAbilityResponse response)
        {
            if (response.Code == 0)
            {
                RefreshInfo(response.CharacterData, response.AbilityPoint);
            }

            PanelLoading.Close();
        }
    }
}