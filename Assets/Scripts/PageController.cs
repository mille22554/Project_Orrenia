using System;
using System.Collections.Generic;
using UnityEngine;

public class PageController : MonoBehaviour
{
    public List<MonoBehaviour> pages;
    [NonSerialized] public int currentPageIndex = 0;

    private void Awake()
    {
        pages.ForEach(page => page.gameObject.SetActive(true));
    }

    private void Start()
    {
        pages.ForEach(page => page.gameObject.SetActive(false));
        SwitchPage(PageName.Login);

        EventMng.SetEvent(EventName.SwitchPage, (Action<PageName>)SwitchPage);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.SwitchPage, (Action<PageName>)SwitchPage);
    }

    public void SwitchPage(PageName pageName)
    {
        pages[currentPageIndex].gameObject.SetActive(false);
        currentPageIndex = (int)pageName;
        pages[currentPageIndex].gameObject.SetActive(true);
    }
}

public enum PageName
{
    Login,
    Main
}
