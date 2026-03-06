using UnityEngine;
using UnityEngine.UI;
using static GameItem;

public class PageRegister : MonoBehaviour
{
    const string resourcePath = "Prefabs/PageRegister";
    [SerializeField] Button btnRegister;
    [SerializeField] InputField inputUsername;
    PlayerData PlayerData => GameData.NowPlayerData;

    public static void Create()
    {
        var page = ObjectPool.Get(Resources.Load<GameObject>(resourcePath).GetComponent<PageRegister>(), MainController.Instance.PageContent);
        MainController.Instance.SwitchPage(page);
    }

    void Start()
    {
        btnRegister.onClick.AddListener(OnRegister);
    }

    void OnRegister()
    {
        GameData.gameData = GameSaveData.CreateDefault();
        PlayerData.PlayerName = inputUsername.text;
        PublicFunc.SetPlayerAbility(PlayerData.ability, PlayerData.equips, PlayerData.effects, PlayerData.effectActions);
        PublicFunc.SetHP(PlayerData.ability.HP);
        PublicFunc.SetMP(PlayerData.ability.MP);
        PublicFunc.SetCurrentSTA(PlayerData.ability.STA);

        PublicFunc.CheckFlags();

        MainController.Instance.Login();
    }
}