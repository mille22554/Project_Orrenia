using UnityEngine;

public class MainController : MonoBehaviour
{
    public static MainController Instance { get; private set; }

    public Transform PageContent;
    public Transform InfoContent;
    public MonoBehaviour CurrentPage;

    void Awake()
    {
        Instance = this;
        ItemBaseData.BuildDatabase();
    }

    void Start()
    {
        PageStart.Create();
    }

    public void Login()
    {
        PanelBtns.Create();
        PanelInfo.Create();
        PageBattle.Create();
    }

    public void SwitchPage<T>(T page) where T : MonoBehaviour
    {
        if (CurrentPage != null)
            ObjectPool.Put(CurrentPage);

        CurrentPage = page;
    }
}