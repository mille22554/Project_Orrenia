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

        ItemBaseData.BuildDatabase();
        ApiBridge.Initialize("");
    }

    void Start()
    {
        PageStart.Create();
    }

    public void Login()
    {
        var requestData = new GetAreaDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetAreaDataResponse response)
        {
            GameData.AreaData = response.AreaData;

            PanelBtns.Create();
            _panelInfo = PanelInfo.Create();
            PageBattle.Create();
        }
    }

    public void SwitchPage<T>(T page) where T : MonoBehaviour
    {
        if (CurrentPage != null)
            ObjectPool.Put(CurrentPage);

        CurrentPage = page;
    }

    public void RefreshUI()
    {
        var requestData = new GetSaveDataRequest();
        ApiBridge.Send(requestData, CallBack);

        void CallBack(GetSaveDataResponse response) => RefreshUI(response);
    }

    public void RefreshUI(GetSaveDataResponse response)
    {
        _panelInfo.RefreshInfo(response);
    }
}