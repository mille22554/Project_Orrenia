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
        RefreshInfo();
    }

    void RefreshInfo()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create();

        void CallBack(GetSaveDataResponse response)
        {
            var characterData = response.SaveData.Datas.CharacterData;
            var isHasAbilityPoint = response.AbilityPoint > 0;

            STR.SetInfo(characterData.Ability.STR_Point, isHasAbilityPoint, OnAbilityPlus);
            VIT.SetInfo(characterData.Ability.VIT_Point, isHasAbilityPoint, OnAbilityPlus);
            DEX.SetInfo(characterData.Ability.DEX_Point, isHasAbilityPoint, OnAbilityPlus);
            INT.SetInfo(characterData.Ability.INT_Point, isHasAbilityPoint, OnAbilityPlus);
            AGI.SetInfo(characterData.Ability.AGI_Point, isHasAbilityPoint, OnAbilityPlus);
            LUK.SetInfo(characterData.Ability.LUK_Point, isHasAbilityPoint, OnAbilityPlus);

            abilityPoint.text = response.AbilityPoint.ToString();

            MainController.Instance.RefreshUI(response);
            PanelLoading.Close();
        }
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

        var requestData = new SetPlayerAbilityRequest
        {
            Ability = ability
        };
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create();

        void CallBack(SetPlayerAbilityResponse response)
        {
            RefreshInfo();
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

        var requestData = new SetPlayerAbilityRequest
        {
            Ability = ability
        };
        ApiBridge.Send(requestData, CallBack);
        PanelLoading.Create();

        void CallBack(SetPlayerAbilityResponse response)
        {
            RefreshInfo();
            PanelLoading.Close();
        }
    }
}