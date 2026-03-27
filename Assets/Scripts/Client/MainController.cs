using UnityEngine;

public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    public Transform PageContent;
    public Transform InfoContent;
    public MonoBehaviour CurrentPage;

    PanelInfo _panelInfo;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApiBridge.Initialize("");
    }

    void Start()
    {
        PageStart.Create();
    }

    public void Login()
    {
        PanelBtns.Create();
        _panelInfo = PanelInfo.Create();
        PageBattle.Create();
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

    public void RefreshUI(CharacterData characterData)
    {
        var data = RefreshInfoData.Create(characterData);
        _panelInfo.RefreshInfo(data);
    }
}