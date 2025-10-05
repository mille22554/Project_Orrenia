using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PageController : MonoBehaviour
{
    public List<PageBase> pages;
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
