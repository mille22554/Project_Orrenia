using Unity.Netcode;
using UnityEngine;

public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    public Transform PageContent;
    public Transform InfoContent;
    public Transform Loading;
    public Transform SystemMessage;

    MonoBehaviour CurrentPage;
    PanelInfo _panelInfo;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ApiBridge.Initialize("");

        NetworkManager.Singleton.OnClientConnectedCallback += OnConnectSuccess;

        void OnConnectSuccess(ulong id)
        {
            if (id == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("連線成功！已成功加入伺服器。");
            }
        }
    }

    void Start()
    {
        NetworkManager.Singleton.StartHost();
        // NetworkManager.Singleton.StartClient();
        // NetworkManager.Singleton.StartServer();
        PageStart.Create();
    }

    public void Login()
    {
        DataCenter.Init(() =>
        {
            _panelInfo = PanelInfo.Create();
            PanelBtns.Create();
            PageBattle.Create();
        });
    }

    public void SwitchPage<T>(T page) where T : MonoBehaviour
    {
        if (CurrentPage != null)
            ObjectPool.Put(CurrentPage);

        CurrentPage = page;
    }

    public void RefreshUI(GetSaveDataResponse response)
    {
        var data = RefreshInfoData.Create(response);
        _panelInfo.RefreshInfo(data);
    }

    public void RefreshUI(CharacterData characterData) => RefreshUI(characterData, null);
    public void RefreshUI(CharacterData characterData, FullAbilityBase fullAbility)
    {
        var data = RefreshInfoData.Create(characterData, fullAbility);
        _panelInfo.RefreshInfo(data);
    }
}