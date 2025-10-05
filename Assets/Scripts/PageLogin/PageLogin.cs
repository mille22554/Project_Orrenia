using System;
using UnityEngine;

public class PageLogin : PageBase
{
    public PanelStart panelStart;
    public PanelRegister panelRegister;

    private void Start()
    {
        EventMng.SetEvent(EventName.Login_Start_Switch_To_Register, (Action)PageSwitchToRegister);
    }

    private void OnDestroy()
    {
        EventMng.DelEvent(EventName.Login_Start_Switch_To_Register, (Action)PageSwitchToRegister);
    }

    private void PageSwitchToRegister()
    {
        panelStart.gameObject.SetActive(false);
        panelRegister.gameObject.SetActive(true);
    }
}